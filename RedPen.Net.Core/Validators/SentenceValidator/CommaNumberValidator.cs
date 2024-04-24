using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentenceValidator;

namespace RedPen.Net.Core.Validators.SentecneValidator
{
    /// <summary>
    /// センテンス内のコンマの最大数を制限としてValidationを実行するValidator。
    /// </summary>
    public sealed class CommaNumberValidator : Validator, ISentenceValidatable
    {
        /// <summary>Configuration</summary>
        public CommaNumberConfiguration Config { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaNumberValidator"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public CommaNumberValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            CommaNumberConfiguration config) :
            base(config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// PreValidation.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        /// <summary>
        /// Validation and out ValidationErrors.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            int commaCount = sentence.Content.ToCharArray()
                .Where(c => c == this.SymbolTable.GetValueOrFallbackToDefault(SymbolType.COMMA))
                .Count();

            if (Config.MaxNumber < commaCount)
            {
                // コンマの数が最大数を超えている場合はエラーとする。
                result.Add(
                    new ValidationError(
                        ValidationType.CommaNumber,
                        this.Level,
                        sentence,
                        // メッセージ引数は実際のコンマの数、最大数の順番で格納する。
                        MessageArgs: new object[] { commaCount, Config.MaxNumber }));
            }

            return result;
        }
    }
}
