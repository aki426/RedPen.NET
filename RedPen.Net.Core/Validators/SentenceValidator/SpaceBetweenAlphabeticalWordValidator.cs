using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SpaceBetweenAlphabeticalWordのConfiguration</summary>
    public record SpaceBetweenAlphabeticalWordConfiguration : ValidatorConfiguration
    {
        public SpaceBetweenAlphabeticalWordConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>SpaceBetweenAlphabeticalWordのValidator</summary>
    public class SpaceBetweenAlphabeticalWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。
        /// <summary>ValidatorConfiguration</summary>
        public SpaceBetweenAlphabeticalWordConfiguration Config { get; init; }

        // TODO: サポート対象言語がANYではない場合overrideで再定義する。
        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public SpaceBetweenAlphabeticalWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SpaceBetweenAlphabeticalWordConfiguration config) :
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
                ValidationType.SpaceBetweenAlphabeticalWord,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
