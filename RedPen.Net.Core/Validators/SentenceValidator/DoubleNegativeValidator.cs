using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubleNegativeのConfiguration</summary>
    public record DoubleNegativeConfiguration : ValidatorConfiguration
    {
        public DoubleNegativeConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>DoubleNegativeのValidator</summary>
    public class DoubleNegativeValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubleNegativeConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public DoubleNegativeValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubleNegativeConfiguration config) :
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
                ValidationType.DoubleNegative,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
