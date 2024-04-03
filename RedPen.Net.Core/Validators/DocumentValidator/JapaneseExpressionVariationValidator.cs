using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NLog;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>
    /// The japanese expression variation validator.
    /// </summary>
    public class JapaneseExpressionVariationValidator : KeyValueDictionaryValidator
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        private Dictionary<Document, Dictionary<string, List<TokenInfo>>> readingMap;
        private Dictionary<Document, List<Sentence>> sentenceMap;

        /// <summary>JAVA版のmapリソースロードロジックをバイパスするためのDictionary。</summary>
        private Dictionary<string, string> spellingVariationMap;

        /// <summary>
        /// The token info.
        /// </summary>
        private class TokenInfo
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
        public JapaneseExpressionVariationValidator() : base("SpellingVariation")
        {
            // MEMO: KeyValueDictionaryValidatorの仕組みをバイパスするための処理。
            spellingVariationMap = new Dictionary<string, string>();

            // DefaultResourceの読み込み。
            string v = DefaultResources.ResourceManager.GetString($"SpellingVariation_ja"); // $"SpellingVariation_{SymbolTable.Lang}");
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] result = line.Split('\t');

                if (result.Length == 2)
                {
                    spellingVariationMap[result[0]] = result[1];
                }
                else
                {
                    log.Error("Skip to load line... Invalid line: " + line);
                }
            }
        }

        /// <summary>
        /// KeyValueDictionaryValidatorのDictionaryアクセスをバイパスするためのメソッド。
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A bool.</returns>
        protected new bool inDictionary(string word)
        {
            return spellingVariationMap.ContainsKey(word);
        }

        /// <summary>
        /// KeyValueDictionaryValidatorのDictionaryアクセスをバイパスするためのメソッド。
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>A string? .</returns>
        protected new string? getValue(string word)
        {
            if (spellingVariationMap != null && spellingVariationMap.ContainsKey(word))
            {
                return spellingVariationMap[word];
            }

            return null;
        }

        protected override void Init()
        {
            base.Init();
            this.readingMap = new Dictionary<Document, Dictionary<string, List<TokenInfo>>>();
            this.sentenceMap = new Dictionary<Document, List<Sentence>>();
        }

        /// <summary>
        /// gets the supported languages.
        /// </summary>
        /// <returns>A list of string.</returns>
        public override List<string> getSupportedLanguages()
        {
            // TODO: TwoLetterISOLanguageNameにより"ja"が取得できるが、C#のカルチャ文字列フォーマット"ja-JP"へ統一することを検討する。
            return new List<string> { CultureInfo.GetCultureInfo("ja-JP").TwoLetterISOLanguageName };
        }

        public override void Validate(Document document)
        {
            if (!sentenceMap.ContainsKey(document))
            {
                throw new InvalidOperationException
                    ("Document " + document.FileName + " does not have any sentence");
            }
            foreach (Sentence sentence in sentenceMap[document])
            {
                foreach (TokenElement token in sentence.Tokens)
                {
                    string reading = getReading(token);
                    if (!this.readingMap[document].ContainsKey(reading))
                    {
                        continue;
                    }
                    generateErrors(document, sentence, token, reading);
                    this.readingMap[document].Remove(reading);
                }
            }
        }

        private void generateErrors(
            Document document,
            Sentence sentence,
            TokenElement targetToken,
            string reading)
        {
            Dictionary<string, List<TokenInfo>> variationMap = generateVariationMap(document, targetToken, reading);
            foreach (string surface in variationMap.Keys)
            {
                List<TokenInfo> variationList = variationMap[surface];
                string variation = generateErrorMessage(variationList, surface);
                string positionList = addVariationPositions(variationList);
                addLocalizedErrorFromToken(sentence, targetToken, variation, positionList);
            }
        }

        private string addVariationPositions(List<TokenInfo> tokenList)
        {
            StringBuilder builder = new StringBuilder();
            foreach (TokenInfo variation in tokenList)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(getTokenString(variation));
            }
            return builder.ToString();
        }

        private string getTokenString(TokenInfo token)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(L");
            stringBuilder.Append(token.sentence.LineNumber);
            stringBuilder.Append(",");
            stringBuilder.Append(token.element.Offset);
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        private Dictionary<string, List<TokenInfo>> generateVariationMap(
            Document document, TokenElement targetToken, string reading)
        {
            List<TokenInfo> tokens = (this.readingMap[document])[reading];
            Dictionary<string, List<TokenInfo>> variationMap = new Dictionary<string, List<TokenInfo>>();

            foreach (TokenInfo variation in tokens)
            {
                if (variation.element != targetToken && !targetToken.Surface.Equals(variation.element.Surface))
                {
                    if (!variationMap.ContainsKey(variation.element.Surface))
                    {
                        variationMap[variation.element.Surface] = new List<TokenInfo>();
                    }
                    variationMap[variation.element.Surface].Add(variation);
                }
            }
            return variationMap;
        }

        private string generateErrorMessage(List<TokenInfo> variationList, string surface)
        {
            StringBuilder variation = new StringBuilder();
            variation.Append(surface);
            variation.Append("(");
            variation.Append(variationList[0].element.Tags[0]);
            variation.Append(")");
            return variation.ToString();
        }

        private string getReading(TokenElement token)
        {
            return normalize(getPlainReading(token));
        }

        /// <summary>
        /// gets the plain reading.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A string.</returns>
        private string getPlainReading(TokenElement token)
        {
            string surface = token.Surface.ToLower();
            if (inDictionary(surface))
            {
                return getValue(surface);
            }
            string reading = token.Reading == null ? surface : token.Reading;
            return reading;
        }

        /// <summary>
        /// normalizes the.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A string.</returns>
        private string normalize(string input)
        {
            string normazlied;
            normazlied = input.Replace("ー", "")
                              .Replace("ッ", "")
                              .Replace("ヴァ", "バ")
                              .Replace("ヴィ", "ビ")
                              .Replace("ヴェ", "ベ")
                              .Replace("ヴォ", "ボ")
                              .Replace("ヴ", "ブ");
            return normazlied;
        }

        /// <summary>
        /// PreProcessors to documents
        /// MEMO: Validateメソッドの前に実行される前処理関数。
        /// </summary>
        /// <param name="document">The document.</param>
        public override void PreValidate(Document document)
        {
            // Sentence抽出。
            sentenceMap[document] = extractSentences(document);
            foreach (Sentence sentence in sentenceMap[document])
            {
                // Token抽出。
                extractTokensFromSentence(document, sentence);
            }
        }

        // TODO: 以下のextract関数は汎用的なものなので他のクラスで実装すべきでは？

        /// <summary>
        /// DocumentからSentenceを抽出する関数。
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private List<Sentence> extractSentences(Document document)
        {
            // 全SectionからSentenceを抽出する。
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in document.Sections)
            {
                sentences.AddRange(extractSentencesFromSection(section));
            }
            return sentences;
        }

        /// <summary>
        /// SectionからSentenceを抽出する関数。
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        private List<Sentence> extractSentencesFromSection(Section section)
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
        private void extractTokensFromSentence(Document document, Sentence sentence)
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
                string reading = getReading(token);
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
                        TokenInfo compoundNoun = generateTokenFromNounsList(nouns, sentence);
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
        private TokenInfo generateTokenFromNounsList(List<TokenElement> nouns, Sentence sentence)
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
