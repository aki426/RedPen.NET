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
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.Tests;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    public class DoubledJoshiValidatorTests
    {
        private ITestOutputHelper output;

        public DoubledJoshiValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "私は彼は好き。", 0, "", 0, "")]
        [InlineData("002", "私は彼は好き。", 2, "", 1, "は")]
        [InlineData("003", "私は彼が好き。", 2, "", 0, "")]
        [InlineData("004", "私は彼は好き。", 2, "は", 0, "")]
        [InlineData("005", "彼の色鉛筆と私の筆箱。", 2, "", 0, "")]
        [InlineData("006", "彼の色鉛筆と私の筆箱。", 3, "", 1, "の")]
        [InlineData("007", "彼の色鉛筆と私の筆箱。", 4, "", 1, "の")]
        [InlineData("008", "二人でで一人の仮面ライダーだ。", 1, "", 1, "で")]
        public void BasicTest(string nouse1, string text, int maxInterval, string skipJoshi, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledJoshiConfiguration(
                ValidationLevel.ERROR,
                maxInterval,
                skipJoshi.Split(',').ToHashSet());

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledJoshiValidator(
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
    }
}
