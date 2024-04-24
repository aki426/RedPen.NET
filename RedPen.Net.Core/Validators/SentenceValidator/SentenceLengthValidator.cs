﻿using RedPen.Net.Core.Config;
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
        public List<ValidationError> Validate(Sentence sentence)
        {
            if (sentence.Content.Length > Config.MaxLength)
            {
                return new List<ValidationError>()
                {
                    new ValidationError(
                        ValidationType.SentenceLength,
                        this.Level,
                        sentence,
                        MessageArgs: new object[] { sentence.Content.Length, Config.MaxLength })
                };
            }
            else
            {
                return new List<ValidationError>();
            }
        }
    }
}
