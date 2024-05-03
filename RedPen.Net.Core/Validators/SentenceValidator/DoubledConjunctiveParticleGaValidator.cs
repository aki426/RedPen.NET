using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubledConjunctiveParticleGaのConfiguration</summary>
    public record DoubledConjunctiveParticleGaConfiguration : ValidatorConfiguration
    {
        public DoubledConjunctiveParticleGaConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>DoubledConjunctiveParticleGaのValidator</summary>
    public class DoubledConjunctiveParticleGaValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubledConjunctiveParticleGaConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        public DoubledConjunctiveParticleGaValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubledConjunctiveParticleGaConfiguration config) :
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
                ValidationType.DoubledConjunctiveParticleGa,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
