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
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.Tests;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    public class SymbolWithSpaceValidatorTests
    {
        private ITestOutputHelper output;

        public SymbolWithSpaceValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "I like apple/orange.", 0, "")]
        [InlineData("002", "I like her:yes it is.", 0, "")]
        [InlineData("003", "I like 1*10.", 0, "")]
        // char.IsLetterOrDigitがFalseの場合は、NeedAfterSpaceがTrueであっても、後ろのスペースが無くてよい。
        [InlineData("004", "Hello (world).", 0, "")]
        // 通常ケースではNeedBeforeSpaceとNeedAfterSpaceがTrueであるとき、それはスペース必須、という意味である。
        [InlineData("005", "I like her (Nancy) very much.", 0, "")]
        [InlineData("006", "I like her(Nancy)very much.", 2, "(,)")]
        [InlineData("007", "I like her(Nancy) very much.", 1, "(")]
        [InlineData("008", "I like her (Nancy)very much.", 1, ")")]
        // JAVAベタ移植版では、NeedBeforeSpaceとNeedAfterSpaceがFalseであるとき、それはスペース禁止、という意味ではない。
        [InlineData("009", "I like her ( Nancy) very much.", 0, "(")]
        [InlineData("010", "I like her (Nancy ) very much.", 0, "(")]
        [InlineData("011", "I like her ( Nancy ) very much.", 0, "(")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // ValidatorConfiguration
            var validatorConfiguration = new SymbolWithSpaceConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SymbolWithSpaceValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        // TODO: 日本語のテストケースを追加する。
        // 特にスペースを禁止する仕様を追加する。
    }
}
