using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>ParenthesizedSentenceのConfiguration</summary>
    public record ParenthesizedSentenceConfiguration : ValidatorConfiguration,
        IMaxLengthConfigParameter, IMaxNumberConfigParameter, IMaxLevelConfigParameter
    {
        /// <summary>括弧内に存在してもよい単語数の上限</summary>
        public int MaxLength { get; init; }

        /// <summary>一文内に存在してよい括弧の上限数</summary>
        public int MaxNumber { get; init; }

        /// <summary>一文に存在してよい括弧のネスト数</summary>
        public int MaxLevel { get; init; }

        public ParenthesizedSentenceConfiguration(ValidationLevel level, int maxLength, int maxNumber, int maxLevel) : base(level)
        {
            this.MaxLength = maxLength;
            this.MaxNumber = maxNumber;
            this.MaxLevel = maxLevel;
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>ParenthesizedSentenceのValidator</summary>
    public class ParenthesizedSentenceValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public ParenthesizedSentenceConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="ParenthesizedSentenceValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public ParenthesizedSentenceValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            ParenthesizedSentenceConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            result.Add(new ValidationError(
                ValidationType.ParenthesizedSentence,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
