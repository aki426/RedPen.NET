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

using System;
using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>InvalidExpressionのConfiguration</summary>
    public record InvalidExpressionConfiguration : ValidatorConfiguration, IExpressionSetConfigParameter
    {
        public HashSet<string> ExpressionSet { get; init; }

        public InvalidExpressionConfiguration(ValidationLevel level, HashSet<string> expressionSet) : base(level)
        {
            ExpressionSet = expressionSet;
        }
    }

    /// <summary>InvalidExpressionのValidator</summary>
    public sealed class InvalidExpressionValidator : Validator, ISentenceValidatable // DictionaryValidator
    {
        public InvalidExpressionConfiguration Config { get; init; }

        public InvalidExpressionValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            InvalidExpressionConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                //errorMessages,
                symbolTable)
        {
            this.Config = config;
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            foreach (string invalidWord in Config.ExpressionSet)
            {
                // Invalidな表現を1つのセンテンス内から複数探索する。
                int offset = 0;
                while (true)
                {
                    // MEMO: String.IndexOf(string, int, StringComparison)は.NET 2.0以降で使用可能。
                    // MEMO: StringComparison.Ordinalは.NET 1.1以降で使用可能。
                    // MEMO: StringComparison.Ordinalは大文字と小文字を区別する。
                    int matchStartPosition = sentence.Content.IndexOf(invalidWord, offset, StringComparison.Ordinal);
                    if (matchStartPosition <= -1)
                    {
                        // not found
                        break;
                    }

                    // マッチしたInvalid Expressionの全文字位置を登録する。
                    int matchEndPosition = matchStartPosition + invalidWord.Length - 1;
                    result.Add(
                        new ValidationError(
                            ValidationName,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(matchStartPosition),
                            //matchStartPosition,
                            sentence.ConvertToLineOffset(matchEndPosition),
                            //matchEndPosition,
                            MessageArgs: new object[] { invalidWord }));

                    // next loop. マッチしたinvalidWordの次の文字から再検索するため+1。
                    offset = matchEndPosition + 1;
                }
            }

            return result;
        }
    }
}
