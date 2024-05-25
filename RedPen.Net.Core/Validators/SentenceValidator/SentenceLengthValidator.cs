using RedPen.Net.Core.Config;
using System.Globalization;
using System.Resources;
using RedPen.Net.Core.Model;
using System.Collections.Generic;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>SentenceLengthのConfiguration</summary>
    public record SentenceLengthConfiguration : ValidatorConfiguration, IMinLengthConfigParameter
    {
        public int MinLength { get; init; }

        public SentenceLengthConfiguration(ValidationLevel level, int minLength) : base(level)
        {
            MinLength = minLength;
        }
    }

    // MEMO: JAVA版ではpublic final class指定なので、sealed classに変更している。

    /// <summary>SentenceLengthのValidator</summary>
    public sealed class SentenceLengthValidator : Validator, ISentenceValidatable
    {
        public SentenceLengthConfiguration Config { get; init; }

        public SentenceLengthValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SentenceLengthConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                //errorMessages,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>
        /// Validate sentence.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A ValidationError? .</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            if (Config.MinLength <= sentence.Content.Length)
            {
                return new List<ValidationError>()
                {
                    new ValidationError(
                        ValidationType.SentenceLength,
                        this.Level,
                        sentence,
                        MessageArgs: new object[] { sentence.Content.Length, Config.MinLength })
                };
            }
            else
            {
                return new List<ValidationError>();
            }
        }
    }
}
