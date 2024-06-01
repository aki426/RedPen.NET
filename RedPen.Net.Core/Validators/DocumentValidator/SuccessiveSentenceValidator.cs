using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>SuccessiveSentenceのConfiguration</summary>
    public record SuccessiveSentenceConfiguration : ValidatorConfiguration, IMaxDistanceConfigParameter, IMinLengthConfigParameter
    {
        /// <summary>エラーとして検出するLevenStein距離の最大値＝Sentence同士がここまでは離れていてもよいとする値</summary>
        public int MaxDistance { get; init; }

        /// <summary>エラーとして検出するSentenceの文字数の最小値＝これ以上短いSentenceは類似していてもエラーにしない</summary>
        public int MinLength { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessiveSentenceConfiguration"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="maxDistance">The max distance.</param>
        /// <param name="minLength">The min length.</param>
        public SuccessiveSentenceConfiguration(ValidationLevel level, int maxDistance, int minLength) : base(level)
        {
            this.MaxDistance = maxDistance;
            this.MinLength = minLength;
        }
    }

    /// <summary>SuccessiveSentenceのValidator</summary>
    public class SuccessiveSentenceValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SuccessiveSentenceConfiguration Config { get; init; }

        private LevenshteinDistanceUtility levenshteinDistanceUtility;

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessiveSentenceValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public SuccessiveSentenceValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SuccessiveSentenceConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;

            levenshteinDistanceUtility = new LevenshteinDistanceUtility();
        }

        // TODO: PlainText以外のHeaderSentence、ListBox、Subsectionsを持つDocumentに対して期待通り機能するかテストケースで検証する。

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="document">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation

            Sentence prev = null;
            foreach (Sentence curr in document.GetAllSentences())
            {
                if (prev == null)
                {
                    // initial
                    prev = curr;
                }
                else if (prev.Content.Trim() == "" || curr.Content.Trim() == "")
                {
                    // nothing.
                }
                else
                {
                    // 最小文字数以上の場合にエラーとして検出する。
                    if (Config.MinLength <= curr.Content.Length)
                    {
                        // 完全一致
                        // または、LevenStein距離がMaxDistance以下の場合は類似したSentenceとしてエラーとする。
                        if (curr.Content.ToLower() == prev.Content.ToLower()
                            || levenshteinDistanceUtility.GetDistance(curr.Content.ToLower(), prev.Content.ToLower()) <= Config.MaxDistance)
                        {
                            // error.
                            result.Add(new ValidationError(
                                ValidationName,
                                this.Level,
                                curr,
                                curr.OffsetMap[0],
                                curr.OffsetMap[^1],
                                MessageArgs: new object[] { prev.Content, curr.Content }));
                        }
                    }
                }

                // iteration
                prev = curr;
            }

            return result;
        }
    }
}
