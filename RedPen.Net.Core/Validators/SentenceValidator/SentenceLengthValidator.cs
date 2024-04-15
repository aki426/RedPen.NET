using RedPen.Net.Core.Config;
using System.Globalization;
using System.Resources;
using RedPen.Net.Core.Model;
using System.Collections.Generic;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: JAVA版ではpublic final class指定なので、sealed classに変更している。

    /// <summary>
    /// The sentence length validator.
    /// </summary>
    public sealed class SentenceLengthValidator : Validator, ISentenceValidatable
    {
        public SentenceLengthConfiguration Config { get; init; }

        private SentenceLengthValidator(
            ValidationLevel level,
            CultureInfo lang,
            ResourceManager errorMessages,
            SymbolTable symbolTable,
            SentenceLengthConfiguration config) :
            base(
                level,
                lang,
                errorMessages,
                symbolTable)
        {
            this.Config = config;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SentenceLengthValidator"/> class.
        ///// </summary>
        //public SentenceLengthValidator(SentenceLengthConfiguration config)
        //{
        //    this.Config = config;
        //}

        ///// <summary>
        ///// validates the.
        ///// </summary>
        ///// <param name="sentence">The sentence.</param>
        //public override void Validate(Sentence sentence)
        //{
        //    // int maxLength = GetProperty<int>("max_len");
        //    // int maxLength = GetInt("max_len");
        //    if (sentence.Content.Length > Config.MaxLength)
        //    {
        //        addLocalizedError(sentence, sentence.Content.Length, Config.MaxLength);
        //    }
        //}

        /// <summary>
        /// Pre validation.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        /// <summary>
        /// Validate sentence.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A ValidationError? .</returns>
        public ValidationError? Validate(Sentence sentence)
        {
            if (sentence.Content.Length > Config.MaxLength)
            {
                return GetLocalizedError(sentence, new object[] { sentence.Content.Length, Config.MaxLength });
            }
            else
            {
                return null;
            }
        }
    }
}
