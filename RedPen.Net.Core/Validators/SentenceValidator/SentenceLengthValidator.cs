//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
                        ValidationName,
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
