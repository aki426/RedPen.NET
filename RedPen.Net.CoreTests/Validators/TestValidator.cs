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
using System.Collections.Immutable;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;

namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit
    { }
}

namespace RedPen.Net.Core.Validators.Tests
{
    // NOTE: 応用アプリケーション側で新規追加するValidationのテスト用。

    /// <summary>TestのConfiguration</summary>
    public record TestConfiguration : ValidatorConfiguration
    {
        public TestConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>TestのValidator</summary>
    public class TestValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public TestConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="TestValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public TestValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            TestConfiguration config) :
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
            // あくまでテストなので、「a」または「あ」を検出するValidationとする。
            var indexOfAlpha = sentence.Content.IndexOf('a');
            if (indexOfAlpha >= 0)
            {
                result.Add(new ValidationError(
                    Config.ValidationName,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(indexOfAlpha),
                    sentence.ConvertToLineOffset(indexOfAlpha),
                    MessageArgs: new object[] { "a" }));
            }

            var indexOfHiraganaA = sentence.Content.IndexOf('あ');
            if (indexOfHiraganaA >= 0)
            {
                result.Add(new ValidationError(
                    Config.ValidationName,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(indexOfHiraganaA),
                    sentence.ConvertToLineOffset(indexOfHiraganaA),
                    MessageArgs: new object[] { "あ" }));
            }

            return result;
        }
    }

    public static class TestErrorMessages
    {
        public static ImmutableList<ErrorMessageDefinition> Definitions = new List<ErrorMessageDefinition>()
        {
            new("Test", "", new CultureInfo("ja-JP"), "「a」または「あ」を検出しました。今回は \"{0}\"が見つかりました。"),
            new("Test", "", new CultureInfo("en-US"), " \"a\" or \"あ\" was found. This sentence contains \"{0}\".")
        }.ToImmutableList();
    }
}
