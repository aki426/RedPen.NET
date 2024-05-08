using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SymbolWithSpaceのConfiguration</summary>
    public record SymbolWithSpaceConfiguration : ValidatorConfiguration
    {
        public SymbolWithSpaceConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>SymbolWithSpaceのValidator</summary>
    public class SymbolWithSpaceValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SymbolWithSpaceConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolWithSpaceValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public SymbolWithSpaceValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SymbolWithSpaceConfiguration config) :
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

            // validation
            foreach (SymbolType symbolType in SymbolTable.SymbolTypeDictionary.Keys)
            {
                //validateSymbol(sentence, symbolType);
                string sentenceStr = sentence.Content;
                Symbol symbol = SymbolTable.SymbolTypeDictionary[symbolType];

                if (!symbol.NeedBeforeSpace && !symbol.NeedAfterSpace)
                {
                    continue;
                }

                char target = symbol.Value;
                int position = sentenceStr.IndexOf(target);
                if (position != -1)
                {
                    string key = "";

                    if (position > 0 && symbol.NeedBeforeSpace && !char.IsWhiteSpace(sentenceStr[position - 1]))
                    {
                        key = "Before";
                    }

                    // MEMO: SymbolWithSpaceのMessageKeyはBefore, After, BeforeAfterのいずれかなのでこの並び順を維持すること。

                    if (position < sentenceStr.Length - 1 && symbol.NeedAfterSpace && char.IsLetterOrDigit(sentenceStr[position + 1]))
                    {
                        key += "After";
                    }

                    if (key != string.Empty)
                    {
                        result.Add(new ValidationError(
                            ValidationType.SymbolWithSpace,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(position),
                            sentence.ConvertToLineOffset(position),
                            MessageArgs: new object[] { sentenceStr[position] },
                            MessageKey: key
                        ));
                    }
                }
            }

            return result;
        }
    }
}
