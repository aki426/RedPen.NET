using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>
    /// The japanese expression variation validator.
    /// </summary>
    public class JapaneseExpressionVariationValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        public JapaneseExpressionVariationConfiguration Config { get; init; }

        public Dictionary<Document, Dictionary<string, List<TokenInfo>>> readingMap;
        public Dictionary<Document, List<Sentence>> sentenceMap;

        /// <summary>あらかじめ定義されたゆらぎ表現のMap。JapaneseExpressionVariationConfiguration.WordMapを用いる。</summary>
        private Dictionary<string, string> spellingVariationMap => Config.WordMap;

        /// <summary>
        /// The token info.
        /// </summary>
        public class TokenInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TokenInfo"/> class.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <param name="sentence">The sentence.</param>
            public TokenInfo(TokenElement element, Sentence sentence)
            {
                this.element = element;
                this.sentence = sentence;
            }

            public TokenElement element;
            public Sentence sentence;
        }

        // MEMO: newするタイミングでデフォルトリソースから辞書データを読み込む。

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidator"/> class.
        /// </summary>
        public JapaneseExpressionVariationValidator(
            CultureInfo lang,
            ResourceManager errorMessages,
            SymbolTable symbolTable,
            JapaneseExpressionVariationConfiguration config) :
            base(
                config.Level,
                lang,
                errorMessages,
                symbolTable)
        {
            this.Config = config;
            Init();

            //// MEMO: KeyValueDictionaryValidatorの仕組みをバイパスするための処理。
            //spellingVariationMap = new Dictionary<string, string>();

            //// DefaultResourceの読み込み。
            //string v = DefaultResources.ResourceManager.GetString($"SpellingVariation_ja");
            //foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            //{
            //    string[] result = line.Split('\t');

            //    if (result.Length == 2)
            //    {
            //        spellingVariationMap[result[0]] = result[1];
            //    }
            //    else
            //    {
            //        log.Error("Skip to load line... Invalid line: " + line);
            //    }
            //}
        }

        /// <summary>
        /// KeyValueDictionaryValidatorのDictionaryアクセスをバイパスするためのメソッド。
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A bool.</returns>
        protected new bool InDictionary(string word) =>
            spellingVariationMap.ContainsKey(word);

        /// <summary>
        /// KeyValueDictionaryValidatorのDictionaryアクセスをバイパスするためのメソッド。
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A string? .</returns>
        protected new string? GetValue(string word)
        {
            if (spellingVariationMap != null && spellingVariationMap.ContainsKey(word))
            {
                return spellingVariationMap[word];
            }

            return null;
        }

        /// <summary>
        /// 現在蓄積されている文章構造Map情報を初期化する。
        /// </summary>
        private void Init()
        {
            this.readingMap = new Dictionary<Document, Dictionary<string, List<TokenInfo>>>();
            this.sentenceMap = new Dictionary<Document, List<Sentence>>();
        }

        /// <summary>サポート対象言語はja-JPのみ。</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        /// <summary>
        /// PreProcessors to documents
        /// MEMO: Validateメソッドの前に実行される前処理関数。
        /// </summary>
        /// <param name="document">The document.</param>
        public void PreValidate(Document document)
        {
            // Sentence抽出。
            sentenceMap[document] = ExtractSentences(document);
            foreach (Sentence sentence in sentenceMap[document])
            {
                // Token抽出。
                ExtractTokensFromSentence(document, sentence);
            }
        }

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
                    string reading = GetReading(token);
                    if (!this.readingMap[document].ContainsKey(reading))
                    {
                        continue;
                    }

                    // tokenに対して取得したreadingでエラーを生成する。
                    errors.AddRange(GenerateErrors(document, sentence, token, reading));

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
        /// <param name="targetToken"></param>
        /// <param name="reading"></param>
        private List<ValidationError> GenerateErrors(
            Document document,
            Sentence sentence,
            TokenElement targetToken,
            string reading)
        {
            List<ValidationError> errors = new List<ValidationError>();

            // ゆらぎ表現をリストアップする。
            Dictionary<string, List<TokenInfo>> variationMap = GenerateVariationMap(document, targetToken, reading);
            foreach (string surface in variationMap.Keys)
            {
                List<TokenInfo> variationList = variationMap[surface];
                // ゆらぎ表現を説明する文字列
                string variation = $"{surface}({variationList[0].element.Tags[0]})"; // generateErrorMessage(variationList, surface);
                // ゆらぎ表現の出現位置
                string positionList = AddVariationPositions(variationList);

                errors.Add(GetLocalizedErrorFromToken(sentence, targetToken, new object[] { variation, positionList }));
            }

            return errors;
        }

        private string AddVariationPositions(List<TokenInfo> tokenList) =>
            string.Join(", ", tokenList.Select(i => GetTokenPositionString(i)));

        private string GetTokenPositionString(TokenInfo token) =>
            $"(L{token.sentence.LineNumber},{token.element.Offset})";

        private Dictionary<string, List<TokenInfo>> GenerateVariationMap(
            Document document, TokenElement targetToken, string reading)
        {
            Dictionary<string, List<TokenInfo>> variationMap = new Dictionary<string, List<TokenInfo>>();

            // readingに対するTokenリストを取得。
            foreach (TokenInfo variation in this.readingMap[document][reading])
            {
                if (variation.element != targetToken && !targetToken.Surface.Equals(variation.element.Surface))
                {
                    // 存在しなければListを登録。
                    if (!variationMap.ContainsKey(variation.element.Surface))
                    {
                        variationMap[variation.element.Surface] = new List<TokenInfo>();
                    }

                    // readingをキーとして取得したTokenを今度はSurfaceをキーとして登録する。
                    // これによって、ゆらぎのマップを作成する。
                    variationMap[variation.element.Surface].Add(variation);
                }
            }
            return variationMap;
        }

        private string GenerateErrorMessage(List<TokenInfo> variationList, string surface) =>
            $"{surface}({variationList[0].element.Tags[0]})";

        /// <summary>
        /// 辞書から引かれたゆらぎの表現を含む、正規化されたReadingを取得する。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private string GetReading(TokenElement token) =>
            Normalize(GetPlainReading(token));

        /// <summary>
        /// gets the plain reading.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A string.</returns>
        private string GetPlainReading(TokenElement token)
        {
            string surface = token.Surface.ToLower();
            if (InDictionary(surface))
            {
                // surfaceが辞書に存在する場合は、その値を返す。
                // 例）surface:nodeに大して「ノード」を取得する。
                return GetValue(surface);
            }

            // surfaceが辞書に存在しない場合は、readingを返す。
            return token.Reading == null ? surface : token.Reading;
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

        // TODO: 以下のextract関数は汎用的なものなので他のクラスで実装すべきでは？

        /// <summary>
        /// DocumentからSentenceを抽出する関数。
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private List<Sentence> ExtractSentences(Document document)
        {
            // 全SectionからSentenceを抽出する。
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in document.Sections)
            {
                sentences.AddRange(ExtractSentencesFromSection(section));
            }
            return sentences;
        }

        /// <summary>
        /// SectionからSentenceを抽出する関数。
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private List<Sentence> ExtractSentencesFromSection(Section section)
        {
            List<Sentence> sentencesInSection = new List<Sentence>();

            // ParagraphsからSentenceを抽出する。
            foreach (Paragraph paragraph in section.Paragraphs)
            {
                sentencesInSection.AddRange(paragraph.Sentences);
            }

            // TODO: 先にParagraphからSentenceを抽出しているのはなぜ？　順番は関係ない？
            sentencesInSection.AddRange(section.HeaderSentences);

            // ListBlocksからSentenceを抽出する。
            foreach (ListBlock listBlock in section.ListBlocks)
            {
                foreach (ListElement listElement in listBlock.ListElements)
                {
                    sentencesInSection.AddRange(listElement.Sentences);
                }
            }

            return sentencesInSection;
        }

        /// <summary>
        /// すべてのTokenを抽出する。
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="sentence">The sentence.</param>
        private void ExtractTokensFromSentence(Document document, Sentence sentence)
        {
            List<TokenElement> nouns = new List<TokenElement>();
            foreach (TokenElement token in sentence.Tokens)
            {
                // 半角スペーススキップ。
                if (token.Surface.Equals(" "))
                {
                    continue;
                }

                // 対応readingMapが不在の場合初期化。
                if (!this.readingMap.ContainsKey(document))
                {
                    this.readingMap[document] = new Dictionary<string, List<TokenInfo>>();
                }
                string reading = GetReading(token);
                if (!this.readingMap[document].ContainsKey(reading))
                {
                    this.readingMap[document][reading] = new List<TokenInfo>();
                }

                // reading->Tokenの関係を登録。
                this.readingMap[document][reading].Add(new TokenInfo(token, sentence));

                // 名詞を収集。
                if (token.Tags[0].Equals("名詞"))
                {
                    nouns.Add(token);
                }
                else
                {
                    // Tagの筆頭が名詞ではない場合で、nounsが2以上の場合？
                    if (nouns.Count > 1)
                    {
                        // 複合名詞として追加登録する。これはNeologdのTokenizeルールによって分割された単語を結合する働きをする。
                        // TODO: Sentenceが体言止めで終わっていた場合の複合名詞登録処理がされないケースが想定される。要チェック。
                        TokenInfo compoundNoun = GenerateTokenFromNounsList(nouns, sentence);
                        if (!this.readingMap[document].ContainsKey(compoundNoun.element.Reading))
                        {
                            this.readingMap[document][compoundNoun.element.Reading] = new List<TokenInfo>();
                        }
                        this.readingMap[document][compoundNoun.element.Reading].Add(compoundNoun);
                    }

                    // 複合名詞を登録したら一旦リストはクリアする。つまり複合名詞は最長のもの1つが登録される。
                    nouns.Clear();
                }
            }
        }

        /// <summary>
        /// 複数の名詞から1つの複合名詞用TokenInfoを生成する。
        /// </summary>
        /// <param name="nouns">The nouns.</param>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A TokenInfo.</returns>
        private TokenInfo GenerateTokenFromNounsList(List<TokenElement> nouns, Sentence sentence)
        {
            // 単純連結。
            StringBuilder surface = new StringBuilder();
            StringBuilder reading = new StringBuilder();
            foreach (TokenElement noun in nouns)
            {
                surface.Append(noun.Surface);
                reading.Append(noun.Reading);
            }

            // Tag、Offsetは先頭のものを流用する。
            TokenElement element = new TokenElement(
                surface.ToString(),
                nouns[0].Tags,
                nouns[0].Offset,
                reading.ToString());
            return new TokenInfo(element, sentence);
        }
    }
}
