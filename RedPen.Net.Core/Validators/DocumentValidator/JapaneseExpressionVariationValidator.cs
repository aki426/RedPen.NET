﻿using System;
using System.Collections.Generic;
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

        public Dictionary<Document, Dictionary<string, List<TokenInSentence>>> readingMap;
        public Dictionary<Document, List<Sentence>> sentenceMap;

        /// <summary>あらかじめ定義されたゆらぎ表現のMap。JapaneseExpressionVariationConfiguration.WordMapを用いる。</summary>
        private Dictionary<string, string> spellingVariationMap => Config.WordMap;

        /// <summary>サポート対象言語はja-JPのみ。</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

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
            sentenceMap[document] = GetAllSentences(document);
            foreach (Sentence sentence in sentenceMap[document])
            {
                // Token抽出。
                UpdateReadingMap(document, sentence);
            }
        }

        /// <summary>
        /// DocumentからSentenceを抽出する関数。
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private List<Sentence> GetAllSentences(Document document)
        {
            // 全SectionからSentenceを抽出する。
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in document.Sections)
            {
                // ParagraphsからSentenceを抽出する。
                sentences.AddRange(section.Paragraphs.SelectMany(i => i.Sentences).ToList());
                // TODO: 先にParagraphからSentenceを抽出しているのはなぜ？　順番は関係ない？
                sentences.AddRange(section.HeaderSentences);
                // ListBlocksからSentenceを抽出する。
                sentences.AddRange(section.ListBlocks.SelectMany(i => i.ListElements.SelectMany(j => j.Sentences)).ToList());
            }

            return sentences;
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
                                nouns[0].Tags,
                                nouns[0].LineNumber,
                                nouns[0].Offset,
                                string.Join("", nouns.Select(i => i.Reading))), // Normalizeしたいが、他TokenのReadingもNormalizeされているわけではないので保留。
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
        /// gets the plain reading.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A string.</returns>
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

                    // なんでエラー1個生成したら削除？
                    this.readingMap[document].Remove(reading);
                }
            }

            return errors;
        }

        // 仮に「単語 ”Node” の揺らぎと考えられる表現 ”ノード(名詞)”」という表現が正とすると、
        // Nodeが正しい表現で、ノードがゆらぎ表現としてエラーだ、という意味になる。
        // ところがRedPenの辞書表現は1列目に誤、2列目に正とする「誤正表」になっている。
        // デフォルト辞書は「node	ノード」なので、この場合は「ノード」が正しい表現で、nodeがゆらぎ表現＝エラーだ、
        // という意味になる。
        // エラーメッセージと辞書の定義が逆転している。

        /// <summary>
        /// エラーメッセージを生成する。
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
            Dictionary<string, List<TokenInSentence>> variationMap = GenerateVariationMap(document, targetToken, reading);
            foreach (string surface in variationMap.Keys)
            {
                List<TokenInSentence> variationList = variationMap[surface];
                // ゆらぎ表現を説明する文字列
                string variation = $"{surface}({variationList[0].element.Tags[0]})"; // generateErrorMessage(variationList, surface);
                // ゆらぎ表現の出現位置
                string positionsText = ConvertToTokenPositionsText(variationList);

                // errors.Add(GetLocalizedErrorFromToken(sentence, targetToken, new object[] { variation, positionList }));
                errors.Add(new ValidationError(
                    ValidationType.JapaneseExpressionVariation,
                    this.Level,
                    sentence,
                    targetToken.Offset, // start
                    targetToken.Offset + targetToken.Surface.Length, // end
                    MessageArgs: new object[] { targetToken.Surface, variation, positionsText } // Surface, ゆらぎ表現, ゆらぎ出現位置、の順で登録。
                ));
            }

            return errors;
        }

        private Dictionary<string, List<TokenInSentence>> GenerateVariationMap(
            Document document, TokenElement targetToken, string reading)
        {
            // SurfaceをキーとしてTokenとその出現位置をValueとするマップとして機能している。
            Dictionary<string, List<TokenInSentence>> variationMap = new Dictionary<string, List<TokenInSentence>>();

            // readingを同じくするTokenのリストを取得。
            foreach (TokenInSentence token in this.readingMap[document][reading])
            {
                // 確認中のtargetTokenとSurfaceが異なるものについて、
                if (token.element != targetToken && !targetToken.Surface.Equals(token.element.Surface))
                {
                    // 存在しなければListを登録。
                    if (!variationMap.ContainsKey(token.element.Surface))
                    {
                        variationMap[token.element.Surface] = new List<TokenInSentence>();
                    }

                    // readingをキーとして取得したTokenを今度はSurfaceをキーとして登録する。
                    // つまりReadingが同じだけど、Surfaceが異なるマップ＝Surfaceに大してゆらいでいるとみなせる相手先のTokenのDictionaryを作成。
                    variationMap[token.element.Surface].Add(token);
                }
            }
            return variationMap;
        }

        private string ConvertToTokenPositionsText(List<TokenInSentence> tokenList) =>
            string.Join(", ", tokenList.Select(i => $"(L{i.sentence.LineNumber},{i.element.Offset})"));
    }
}
