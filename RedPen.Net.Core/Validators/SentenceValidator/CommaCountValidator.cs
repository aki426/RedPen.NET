﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentecneValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>CommaNumberのConfiguration</summary>
    public record CommaCountConfiguration : ValidatorConfiguration, IMinCountConfigParameter
    {
        public int MinCount { get; init; }

        public CommaCountConfiguration(ValidationLevel level, int minCount) : base(level)
        {
            MinCount = minCount;
        }
    }

    /// <summary>
    /// センテンス内のコンマの最大数を制限としてValidationを実行するValidator。
    /// </summary>
    public sealed class CommaCountValidator : Validator, ISentenceValidatable
    {
        /// <summary>Configuration</summary>
        public CommaCountConfiguration Config { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommaCountValidator"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public CommaCountValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            CommaCountConfiguration config) :
            base(config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
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

            if (Config.MinCount <= commaCount)
            {
                // コンマの数がエラーとして扱う最小値以上の場合はエラーとする。
                result.Add(
                    new ValidationError(
                        ValidationName,
                        this.Level,
                        sentence,
                        // メッセージ引数は実際のコンマの数、最大数の順番で格納する。
                        MessageArgs: new object[] { commaCount, Config.MinCount }));
            }

            return result;
        }
    }
}
