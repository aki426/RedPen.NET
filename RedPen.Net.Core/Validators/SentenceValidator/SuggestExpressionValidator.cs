using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public class SuggestExpressionValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidationConfiguration</summary>
        public SuggestExpressionConfiguration Config { get; init; }

        public SuggestExpressionValidator(
            ValidationLevel level,
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SuggestExpressionConfiguration config) : base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        public void PreValidate(Sentence sentence)
        {
            throw new NotImplementedException();
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            throw new NotImplementedException();
        }
    }
}
