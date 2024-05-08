using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.DocumentValidator
{
    /// <summary>SuccessiveSentenceのConfiguration</summary>
    public record SuccessiveSentenceConfiguration : ValidatorConfiguration
    {
        public SuccessiveSentenceConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>SuccessiveSentenceのValidator</summary>
    public class SuccessiveSentenceValidator : Validator, IDocumentValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SuccessiveSentenceConfiguration Config { get; init; }

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
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="document">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            result.Add(new ValidationError(
                ValidationType.SuccessiveSentence,
                this.Level,
                document,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
