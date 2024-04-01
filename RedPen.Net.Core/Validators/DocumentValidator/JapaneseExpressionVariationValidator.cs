using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>
    /// The japanese expression variation validator.
    /// </summary>
    public class JapaneseExpressionVariationValidator : KeyValueDictionaryValidator
    {
        private Dictionary<Document, Dictionary<string, List<TokenInfo>>> readingMap;
        private Dictionary<Document, List<Sentence>> sentenceMap;

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

        // TODO: リソースファイルのパス指定を検証する。

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidator"/> class.
        /// </summary>
        public JapaneseExpressionVariationValidator() : base("JapaneseSpellingVariation/SpellingVariation")
        {
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

        public override void PreValidate(Document document)
        {
            sentenceMap[document] = extractSentences(document);
            List<Sentence> sentences = sentenceMap[document];
            foreach (Sentence sentence in sentences)
            {
                extractTokensFromSentence(document, sentence);
            }
        }

        private List<Sentence> extractSentences(Document document)
        {
            List<Sentence> sentences = new List<Sentence>();
            foreach (Section section in document.Sections)
            {
                sentences.AddRange(extractSentencesFromSection(section));
            }
            return sentences;
        }

        private List<Sentence> extractSentencesFromSection(Section section)
        {
            List<Sentence> sentencesInSection = new List<Sentence>();
            foreach (Paragraph paragraph in section.Paragraphs)
            {
                sentencesInSection.AddRange(paragraph.Sentences);
            }

            sentencesInSection.AddRange(section.HeaderSentences);

            foreach (ListBlock listBlock in section.ListBlocks)
            {
                foreach (ListElement listElement in listBlock.ListElements)
                {
                    sentencesInSection.AddRange(listElement.Sentences);
                }
            }
            return sentencesInSection;
        }

        private void extractTokensFromSentence(Document document, Sentence sentence)
        {
            List<TokenElement> nouns = new List<TokenElement>();
            foreach (TokenElement token in sentence.Tokens)
            {
                if (token.Surface.Equals(" "))
                {
                    continue;
                }
                string reading = getReading(token);
                if (!this.readingMap.ContainsKey(document))
                {
                    this.readingMap[document] = new Dictionary<string, List<TokenInfo>>();
                }
                if (!this.readingMap[document].ContainsKey(reading))
                {
                    this.readingMap[document][reading] = new List<TokenInfo>();
                }
                this.readingMap[document][reading].Add(new TokenInfo(token, sentence));

                // handling compound nouns
                if (token.Tags[0].Equals("名詞"))
                {
                    nouns.Add(token);
                }
                else
                {
                    if (nouns.Count > 1)
                    {
                        TokenInfo compoundNoun = generateTokenFromNounsList(nouns, sentence);
                        if (!this.readingMap[document].ContainsKey(compoundNoun.element.Reading))
                        {
                            this.readingMap[document][compoundNoun.element.Reading] = new List<TokenInfo>();
                        }
                        this.readingMap[document][compoundNoun.element.Reading].Add(compoundNoun);
                    }

                    nouns.Clear();
                }
            }
        }

        private TokenInfo generateTokenFromNounsList(List<TokenElement> nouns, Sentence sentence)
        {
            StringBuilder surface = new StringBuilder();
            StringBuilder reading = new StringBuilder();
            foreach (TokenElement noun in nouns)
            {
                surface.Append(noun.Surface);
                reading.Append(noun.Reading);
            }
            TokenElement element = new TokenElement(
                surface.ToString(),
                nouns[0].Tags,
                nouns[0].Offset,
                reading.ToString());
            return new TokenInfo(element, sentence);
        }
    }
}
