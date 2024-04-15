using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentenceValidator;

namespace RedPen.Net.Core.Validators.SentecneValidator
{
    public sealed class CommaNumberValidator : Validator, ISentenceValidatable
    {
        public CommaNumberConfiguration Config { get; init; }
        private char comma { get; init; }

        public CommaNumberValidator(
            ValidationLevel level,
            CultureInfo lang,
            ResourceManager errorMessages,
            SymbolTable symbolTable,
            CommaNumberConfiguration config) :
            base(level,
                lang,
                errorMessages,
                symbolTable)
        {
            this.Config = config;
            this.comma = this.SymbolTable.GetValueOrFallbackToDefault(SymbolType.COMMA);
        }

        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            string content = sentence.Content;
            int commaCount = 0;
            int position = 0;
            while ((position = content.IndexOf(this.comma)) != -1)
            {
                commaCount++;
                // position + 1 から最後までの文字列を取得。
                content = content.Substring(position + 1);
            }
            if (Config.MaxNumber < commaCount)
            {
                // コンマの数が最大数を超えている場合はエラーとする。
                result.Add(GetLocalizedError(sentence, new object[] { commaCount, Config.MaxNumber }));
            }

            return result;
        }
    }
}
