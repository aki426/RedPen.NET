using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>DoubledWordのConfiguration</summary>
    public record DoubledWordConfiguration : ValidatorConfiguration
    {
        public DoubledWordConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>DoubledWordのValidator</summary>
    public class DoubledWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public DoubledWordConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        public DoubledWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            DoubledWordConfiguration config) :
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
                ValidationType.DoubledWord,
                this.Level,
                sentence,
                MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
