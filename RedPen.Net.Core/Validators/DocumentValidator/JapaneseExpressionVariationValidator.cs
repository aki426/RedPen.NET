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

        // MEMO: 一旦、複数Documentをまたいだゆらぎ表現検出は対応しないものとする。
        // 1Documentにつき1Validationを原則とする。

        /// <summary>ドキュメント内のすべてのTokenの正規化されたReadingをキーとしたマップ</summary>
        public Dictionary<string, List<TokenElement>> readingMap;

        /// <summary>あらかじめ定義されたゆらぎ検出のためのReadingのMap。JapaneseExpressionVariationConfiguration.WordMapを用いる。</summary>
        private Dictionary<string, string> spellingVariationMap => Config.WordMap;

        /// <summary>サポート対象言語はja-JPのみ。</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // MEMO: デフォルトリソースからの辞書データ読み込みはJapaneseExpressionVariationConfiguration生成時に行う。
        // ValidatorはJapaneseExpressionVariationConfigurationの設定情報のみを用いて処理を行う。

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
            this.readingMap = new Dictionary<string, List<TokenElement>>();
        }

        /// <summary>現在Validatorが持つReadingMapを初期化する。</summary>
        private void Init()
        {
            if (this.readingMap == null || this.readingMap.Any())
            {
                this.readingMap = new Dictionary<string, List<TokenElement>>();
            }
        }

        /// <summary>
        /// PreProcessors to documents
        /// MEMO: Validateメソッドの前に実行される前処理関数。
        /// </summary>
        /// <param name="document">The document.</param>
        public void PreValidate(Document document)
        {
            // readingMap初期化。
            Init();

            // すべてのSentenceを抽出し、ReadingMapを作成する。
            foreach (Sentence sentence in document.GetAllSentences())
            {
                // Token抽出。
                UpdateReadingMap(sentence);
            }
        }

        /// <summary>
        /// すべてのTokenを抽出する。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        private void UpdateReadingMap(Sentence sentence)
        {
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
                if (!this.readingMap.ContainsKey(reading))
                {
                    this.readingMap[reading] = new List<TokenElement>();
                }
                // reading->tokenの関係を登録。
                this.readingMap[reading].Add(token);

                // 名詞を収集。
                if (token.Tags[0].Equals("名詞"))
                {
                    nouns.Add(token);
                }
                else
                {
                    // 名詞ではない場合で、nounsが2以上の場合、それは複合名詞がnouns内に格納されておりかつ複合名詞の区切りということ。
                    if (nouns.Count > 1)
                    {
                        // 複合名詞として追加登録する。これはNeologdのTokenizeルールによって分割された単語を結合する働きをする。
                        // TODO: Sentenceが体言止めで終わっておりかつFullStopが無い場合、Sentenceの終わりでこのケースに落ちてこないので
                        // 名詞登録処理がされないケースが想定される。要チェック。

                        var compoundNounToken = new TokenElement(
                            string.Join("", nouns.Select(i => i.Surface)),
                            nouns[0].Tags.ToImmutableList(),
                            string.Join("", nouns.Select(i => GetNormalizedReading(i))),
                            nouns.SelectMany(n => n.OffsetMap).ToImmutableList()
                        );

                        // 読みを正規化してからreadingMapに登録する。
                        string compoundReading = GetNormalizedReading(compoundNounToken);
                        if (!this.readingMap.ContainsKey(compoundReading))
                        {
                            this.readingMap[compoundReading] = new List<TokenElement>();
                        }
                        this.readingMap[compoundReading].Add(compoundNounToken);
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
        /// <param name="document">※不要。</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> errors = new List<ValidationError>();

            foreach (Sentence sentence in document.GetAllSentences())
            {
                foreach (TokenElement token in sentence.Tokens)
                {
                    // readingを取得。ゆらぎ表現が含まれる。
                    string reading = GetNormalizedReading(token);
                    if (!this.readingMap.ContainsKey(reading))
                    {
                        continue;
                    }

                    // tokenに対して取得したreadingでエラーを生成する。
                    errors.AddRange(GenerateErrors(sentence, token, reading));

                    // TODO: なんでエラー1個生成したら削除？　ゆらぎの出現箇所は複数ある可能性がある。
                    this.readingMap.Remove(reading);
                }
            }

            return errors;
        }

        /// <summary>
        /// TargetTokenに対してReadingが同じだがSurfaceが異なる複数のTokenを取得し、その出現位置を示すエラーメッセージを生成する。
        /// </summary>
        /// <param name="document"></param>
        /// <param name="sentence"></param>
        /// <param name="targetToken">評価対象のToken</param>
        /// <param name="reading">正規化されたtargetTokenのReading</param>
        private List<ValidationError> GenerateErrors(
            Sentence sentence,
            TokenElement targetToken,
            string reading)
        {
            List<ValidationError> errors = new List<ValidationError>();

            // ゆらぎ表現をリストアップする。
            var variations = GetVariationTokens(targetToken, reading);
            foreach (string surface in variations.Keys)
            {
                // targetTokenとSurfaceが異なる、Surfaceを同じくするTokenのリストを取得。
                List<TokenElement> variationList = variations[surface];
                // ゆらぎ表現の出現位置（先頭位置で良い）
                string positionsText = string.Join(", ", variationList.Select(t => t.OffsetMap[0].ConvertToShortText()));

                errors.Add(new ValidationError(
                    ValidationType.JapaneseExpressionVariation,
                    this.Level,
                    sentence,
                    targetToken.OffsetMap[0],
                    targetToken.OffsetMap[^1],
                    // エラー箇所のToken表現, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
                    MessageArgs: new object[]
                    {
                        targetToken.GetSurfaceAndTagString(),
                        variationList[0].GetSurfaceAndTagString(),
                        positionsText
                    }
                ));
            }

            return errors;
        }

        /// <summary>
        /// TargetTokenに対してReadingが同じでSurfaceが異なる複数のTokenを取得する。
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="targetToken">The target token.</param>
        /// <param name="reading">The reading.</param>
        /// <returns>A Dictionary.</returns>
        private Dictionary<string, List<TokenElement>> GetVariationTokens(
            TokenElement targetToken, string reading)
        {
            // SurfaceをキーとしてゆらぎのTokenリストを引くDictionary。
            Dictionary<string, List<TokenElement>> variationMap = new Dictionary<string, List<TokenElement>>();

            // readingを同じくするTokenのリストを取得。
            foreach (TokenElement token in this.readingMap[reading])
            {
                // 確認中のtargetTokenとSurfaceが異なるものを集める。品詞は考慮していない。

                // TODO: 複数の品詞のTokenが一緒くたにListに入ってしまうのでそのようなケースを考慮すべきか検討する。つまり品詞ごとに分ける。

                // TODO: Readingが全く同じでSurfaceが異なるTokenが3種類以上ある場合、2者間でのゆらぎ判定ではなく、3者間でのゆらぎ判定となる。
                // つまり最も数が多い種類を正しい表現と推測し、他をゆらぎ表現とみなす。
                // 3者以上が同数だった場合は、両者をお互いにゆらぎ表現とみなす。

                // TODO: ゆらぎの判定は、2種類のSurfaceがあった場合に片方に偏っている場合、数が少ないほうをタイポ＝ゆらぎと推測することができる。
                // 同数だった場合は、両者をお互いにゆらぎ表現とみなす。

                if (targetToken.Surface != token.Surface)
                {
                    // 存在しなければListを登録。
                    if (!variationMap.ContainsKey(token.Surface))
                    {
                        variationMap[token.Surface] = new List<TokenElement>();
                    }

                    // readingをキーとして取得したTokenを今度はSurfaceをキーとして登録する。
                    // つまりReadingが同じだけど、Surfaceが異なるToken＝言及対象のTokenのSurfaceに対してゆらいでいるとみなせる相手先のToken
                    // のDictionaryを作成。
                    variationMap[token.Surface].Add(token);
                }
            }
            return variationMap;
        }
    }
}
