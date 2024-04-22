using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>日本語の表記ゆれを検出するValidator。</summary>
    public class JapaneseExpressionVariationValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidationConfiguration</summary>
        public JapaneseExpressionVariationConfiguration Config { get; init; }

        // readingMapもsentenceMapもDocumentをキーとするDictionaryだが、このValidatorは複数のDocumentに共通する情報を持つ必要があるのか？
        // Validationはたかだが1つのDocument内に閉じた情報と出力だけなら複数Documentの情報を持つ必要はないはず。

        // TODO: Documentをまたいでゆらぎ表現を検出するニーズが無いのであれば、複数Documentの情報をValidatorが持つ必要はない。
        // また、LineOffsetはTokenElementが持っているので、TokenInSentenceは不要。

        // Dictionary<string, List<TokenElement>>へ修正する。
        public Dictionary<Document, Dictionary<string, List<TokenInSentence>>> readingMap;

        // sentenceMapはおそらく不要。
        public Dictionary<Document, List<Sentence>> sentenceMap;

        /// <summary>あらかじめ定義されたゆらぎ表現のMap。JapaneseExpressionVariationConfiguration.WordMapを用いる。</summary>
        private Dictionary<string, string> spellingVariationMap => Config.WordMap;

        /// <summary>サポート対象言語はja-JPのみ。</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: TokenElementが完全なOffsetMapを持つので、TokenInSentenceは不要。

        /// <summary>TokenInSentence</summary>
        public record TokenInSentence(TokenElement element, Sentence sentence);

        // MEMO: newするタイミングでデフォルトリソースから辞書データを読み込む。

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidator"/> class.
        /// </summary>
        public JapaneseExpressionVariationValidator(
            ValidationLevel level,
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseExpressionVariationConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
            Init();
        }

        /// <summary>
        /// 現在蓄積されている文章構造Map情報を初期化する。
        /// </summary>
        private void Init()
        {
            this.readingMap = new Dictionary<Document, Dictionary<string, List<TokenInSentence>>>();
            this.sentenceMap = new Dictionary<Document, List<Sentence>>();
        }

        /// <summary>
        /// PreProcessors to documents
        /// MEMO: Validateメソッドの前に実行される前処理関数。
        /// </summary>
        /// <param name="document">The document.</param>
        public void PreValidate(Document document)
        {
            // すべてのSentenceを抽出し、ReadingMapを作成する。
            sentenceMap[document] = document.GetAllSentences(); // GetAllSentences(document);
            foreach (Sentence sentence in sentenceMap[document])
            {
                // Token抽出。
                UpdateReadingMap(document, sentence);
            }
        }

        /// <summary>
        /// すべてのTokenを抽出する。
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="sentence">The sentence.</param>
        private void UpdateReadingMap(Document document, Sentence sentence)
        {
            // 対応readingMapが不在の場合初期化する。
            if (!this.readingMap.ContainsKey(document))
            {
                this.readingMap[document] = new Dictionary<string, List<TokenInSentence>>();
            }

            // 名詞のみ収集するためのリスト。
            List<TokenElement> nouns = new List<TokenElement>();

            foreach (TokenElement token in sentence.Tokens)
            {
                // 半角スペーススキップ。
                if (token.Surface.Equals(" "))
                {
                    continue;
                }

                // tokenの正規化された読みであるreadingを取得し、readingMapに登録する。
                string reading = GetNormalizedReading(token);
                if (!this.readingMap[document].ContainsKey(reading))
                {
                    this.readingMap[document][reading] = new List<TokenInSentence>();
                }
                // reading->token->sentenceの関係を登録。
                this.readingMap[document][reading].Add(new TokenInSentence(token, sentence));

                // 名詞を収集。
                if (token.Tags[0].Equals("名詞"))
                {
                    nouns.Add(token);
                }
                else
                {
                    // 名詞ではない場合で、nounsが2以上の場合、それは複合名詞がnouns内に格納されているということ。
                    if (nouns.Count > 1)
                    {
                        // 複合名詞として追加登録する。これはNeologdのTokenizeルールによって分割された単語を結合する働きをする。
                        // TODO: Sentenceが体言止めで終わっていた場合、Sentenceの終わりでこのケースに落ちてこないので
                        // 名詞登録処理がされないケースが想定される。要チェック。

                        //TokenInSentence compoundNoun = GenerateTokenFromNounsList(nouns, sentence);

                        TokenInSentence compoundNoun = new TokenInSentence(
                            new TokenElement(
                                string.Join("", nouns.Select(i => i.Surface)),
                                nouns[0].Tags.ToImmutableList(),
                                //nouns[0].LineNumber,
                                //nouns[0].Offset,
                                string.Join("", nouns.Select(i => i.Reading)),
                                nouns.SelectMany(n => n.OffsetMap).ToImmutableList()
                                ),
                            sentence);

                        // 読みを正規化してからreadingMapに登録する。
                        string compoundReading = GetNormalizedReading(compoundNoun.element);
                        if (!this.readingMap[document].ContainsKey(compoundReading))
                        {
                            this.readingMap[document][compoundReading] = new List<TokenInSentence>();
                        }
                        this.readingMap[document][compoundReading].Add(compoundNoun);
                    }

                    // 名詞意外の品詞が来たら、そこで名詞の連続は切れているのでクリアする。
                    // つまり複合名詞は最長のもの1つが登録されるし、たかだか1個の名詞のあと他品詞が来た場合は複合名詞化はされない。
                    if (nouns.Any()) { nouns.Clear(); }
                }
            }
        }

        /// <summary>
        /// 辞書から引かれたゆらぎの表現を含む、正規化されたReadingを取得する。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetNormalizedReading(TokenElement token) =>
            Normalize(GetPlainReading(token));

        /// <summary>
        /// 英語表現は小文字に、WordMapから取得できるReadingがあればそれを、なければTokenElementからReadingまたはSurfaceを取得する。
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>小文字化またはWordMapから取得されたReadingか元々のTokenのReadingまたはSurface</returns>
        private string GetPlainReading(TokenElement token)
        {
            string surface = token.Surface.ToLower(); // MEMO: ToLower()の時点でsurfaceは英語表現だと期待されている？

            // MEMO: むしろゆらぎ表現マップは英語表現の日本語読み辞書として機能している？

            // ゆらぎ表現マップに「誤」として存在するかチェック。
            return spellingVariationMap.ContainsKey(surface) ?
                spellingVariationMap[surface] :
                // surfaceが辞書に存在しない場合は、TokenElementからreadingまたはsurfaceを返す。
                (token.Reading == null ? surface : token.Reading);
        }

        /// <summary>
        /// normalizes the.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A string.</returns>
        private string Normalize(string input) =>
            // MEMO: 長音、促音、ヴ行、を正規化＝使用しない表現に置換。
            input.Replace("ー", "").Replace("ッ", "").Replace("ヴァ", "バ").Replace("ヴィ", "ビ")
                .Replace("ヴェ", "ベ").Replace("ヴォ", "ボ").Replace("ヴ", "ブ");

        /// <summary>
        /// Validate document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> errors = new List<ValidationError>();

            if (!sentenceMap.ContainsKey(document))
            {
                throw new InvalidOperationException
                    ("Document " + document.FileName + " does not have any sentence");
            }
            foreach (Sentence sentence in sentenceMap[document])
            {
                foreach (TokenElement token in sentence.Tokens)
                {
                    // readingを取得。ゆらぎ表現が含まれる。
                    string reading = GetNormalizedReading(token);
                    if (!this.readingMap[document].ContainsKey(reading))
                    {
                        continue;
                    }

                    // tokenに対して取得したreadingでエラーを生成する。
                    errors.AddRange(GenerateErrors(document, sentence, token, reading));

                    // TODO: なんでエラー1個生成したら削除？　ゆらぎの出現箇所は複数ある可能性がある。
                    this.readingMap[document].Remove(reading);
                }
            }

            return errors;
        }

        /// <summary>
        /// TargetTokenに対してReadingが同じだがSurfaceが異なるTokenを取得し、その出現位置を示すエラーメッセージを生成する。
        /// </summary>
        /// <param name="document"></param>
        /// <param name="sentence"></param>
        /// <param name="targetToken">評価対象のToken</param>
        /// <param name="reading">正規化されたtargetTokenのReading</param>
        private List<ValidationError> GenerateErrors(
            Document document,
            Sentence sentence,
            TokenElement targetToken,
            string reading)
        {
            List<ValidationError> errors = new List<ValidationError>();

            // ゆらぎ表現をリストアップする。
            var variations = GetVariationTokens(document, targetToken, reading);
            foreach (string surface in variations.Keys)
            {
                List<TokenElement> variationList = variations[surface];
                // ゆらぎ表現を説明する文字列
                string variationSurfaceStr = $"{surface}({variationList[0].Tags[0]})";
                // ゆらぎ表現の出現位置
                string positionsText = string.Join(", ", variationList.Select(t => t.OffsetMap[0].ConvertToShortText()));

                errors.Add(new ValidationError(
                    ValidationType.JapaneseExpressionVariation,
                    this.Level,
                    sentence,
                    targetToken.OffsetMap[0],
                    targetToken.OffsetMap[^1],
                    // Surface, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
                    MessageArgs: new object[] { targetToken.Surface, variationSurfaceStr, positionsText }
                ));
            }

            return errors;
        }

        /// <summary>
        /// TargetTokenに対してReadingが同じでSurfaceが異なるTokenを取得する。
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="targetToken">The target token.</param>
        /// <param name="reading">The reading.</param>
        /// <returns>A Dictionary.</returns>
        private Dictionary<string, List<TokenElement>> GetVariationTokens(
            Document document, TokenElement targetToken, string reading)
        {
            // SurfaceをキーとしてゆらぎのTokenリスト引くDictionary。
            Dictionary<string, List<TokenElement>> variationMap = new Dictionary<string, List<TokenElement>>();

            // readingを同じくするTokenのリストを取得。
            foreach (TokenInSentence token in this.readingMap[document][reading])
            {
                // 確認中のtargetTokenとSurfaceが異なるものを集める。品詞は考慮していない。
                // TODO: 複数の品詞のTokenが一緒くたにListに入ってしまうのでそのようなケースを考慮すべきか検討する。
                if (targetToken.Surface != token.element.Surface)
                {
                    // 存在しなければListを登録。
                    if (!variationMap.ContainsKey(token.element.Surface))
                    {
                        variationMap[token.element.Surface] = new List<TokenElement>();
                    }

                    // readingをキーとして取得したTokenを今度はSurfaceをキーとして登録する。
                    // つまりReadingが同じだけど、Surfaceが異なるToken＝言及対象のTokenのSurfaceに対してゆらいでいるとみなせる相手先のToken
                    // のDictionaryを作成。
                    variationMap[token.element.Surface].Add(token.element);
                }
            }
            return variationMap;
        }
    }
}
