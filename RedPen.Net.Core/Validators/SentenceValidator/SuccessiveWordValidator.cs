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

using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SuccessiveWordのConfiguration</summary>
    public record SuccessiveWordConfiguration : ValidatorConfiguration
    {
        public SuccessiveWordConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>SuccessiveWordのValidator</summary>
    public class SuccessiveWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SuccessiveWordConfiguration Config { get; init; }

        public SuccessiveWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SuccessiveWordConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            string prevSurface = "";
            foreach (TokenElement token in sentence.Tokens)
            {
                string currentSurface = token.Surface.ToLower();
                if (currentSurface.Length > 0
                    && prevSurface == currentSurface
                    && !IsPartOfNumber(sentence, token))
                {
                    result.Add(new ValidationError(
                        ValidationName,
                        this.Level,
                        sentence,
                        token.OffsetMap[0],
                        token.OffsetMap[^1],
                        MessageArgs: new object[] { token.Surface }));
                }
                prevSurface = currentSurface;
            }

            return result;
        }

        /// <summary>
        /// あるTokenの先頭文字前後3文字分が数字の一部に該当する表現かどうかを検証する関数。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <param name="token">The token.</param>
        /// <returns>A bool.</returns>
        private bool IsPartOfNumber(Sentence sentence, TokenElement token)
        {
            return Regex.IsMatch(sentence.Content.Substring(sentence.ConvertToIndex(token.OffsetMap[0]), 3), "\\d.\\d");
        }
    }
}
