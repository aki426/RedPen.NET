using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>KatakanaSpellCheckのConfiguration</summary>
    public record KatakanaSpellCheckConfiguration : ValidatorConfiguration
    {
        public KatakanaSpellCheckConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>KatakanaSpellCheckのValidator</summary>
    public class KatakanaSpellCheckValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public KatakanaSpellCheckConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="KatakanaSpellCheckValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public KatakanaSpellCheckValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            KatakanaSpellCheckConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            result.Add(new ValidationError(
                ValidationType.KatakanaSpellCheck,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
