using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>InvalidSymbolのConfiguration</summary>
    public record InvalidSymbolConfiguration : ValidatorConfiguration
    {
        public InvalidSymbolConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>InvalidSymbolのValidator</summary>
    public class InvalidSymbolValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public InvalidSymbolConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSymbolValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public InvalidSymbolValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            InvalidSymbolConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // 全SymbolについてSentence内にInvalidCharsが含まれていないか検証する。
            foreach (Symbol symbol in SymbolTable.SymbolTypeDictionary.Values)
            {
                foreach (char invalidChar in symbol.InvalidChars)
                {
                    int charPosition = sentence.Content.IndexOf(invalidChar);

                    while (charPosition != -1)
                    {
                        if (invalidChar != '.' || !IsDigitPeriod(charPosition, sentence.Content))
                        {
                            result.Add(new ValidationError(
                                ValidationName,
                                this.Level,
                                sentence,
                                sentence.ConvertToLineOffset(charPosition),
                                sentence.ConvertToLineOffset(charPosition),
                                MessageArgs: new object[] { invalidChar }));
                        }

                        charPosition = sentence.Content.IndexOf(invalidChar, charPosition + 1);
                    }
                }
            }

            return result;
        }

        // NOTE: Even when selecting Japanese or Chinese style period such as '。', '．', the Ascii period
        // is used in floating numbers (Ex. Ubuntu v1.04 or 200.00).

        /// <summary>
        /// ある文字列中のIndex箇所に出現したピリオド記号が数値表現中のピリオドかどうかを判定する。
        /// </summary>
        /// <param name="periodIndex">The period index.</param>
        /// <param name="str">The str.</param>
        /// <returns>数値表現中のピリオドであればTrueを返す。それ以外の場合はFalse。</returns>
        private bool IsDigitPeriod(int periodIndex, string str)
        {
            // 先頭または末尾の場合は数値表現中のピリオドではないのでFalse。
            if (periodIndex == 0 || periodIndex == str.Length - 1)
            {
                return false;
            }

            // ピリオドの前後が数字である場合のみTrue。
            return char.IsDigit(str[periodIndex - 1]) && char.IsDigit(str[periodIndex + 1]);
        }
    }
}
