using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public class JapaneseStyleValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        public JapaneseStyleConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public JapaneseStyleValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseStyleConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            //// validation

            //// TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。
            //result.Add(new ValidationError(
            //    ValidationType.XXX,
            //    this.Level,
            //    sentence,
            //    MessageArgs: new object[] { argsForMessageArg }));

            return result;
        }
    }
}
