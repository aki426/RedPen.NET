using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SuccessiveWordのConfiguration</summary>
    public record SuccessiveWordConfiguration : ValidatorConfiguration
    {
        public SuccessiveWordConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>SuccessiveWordのValidator</summary>
    public class SuccessiveWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SuccessiveWordConfiguration Config { get; init; }

        public SuccessiveWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SuccessiveWordConfiguration config) :
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
                ValidationType.SuccessiveWord,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
