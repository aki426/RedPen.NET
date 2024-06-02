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
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.SentenceValidator
{
    public class InvalidSymbolValidatorTests
    {
        private ITestOutputHelper output;

        public InvalidSymbolValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "わたしは、カラオケが大好き!", 1, "!")]
        [InlineData("002", "わたしは、カラオケが大好き！", 0, "")]
        [InlineData("003", "Ubuntu v1.6 はいいね。", 0, "")]
        // MEMO: JAVA版ではFullstopのシンボルのInvalidCharsに「.」が含まれていなかったが、C#版では追加した。
        [InlineData("004", "私が好きな OS は Ubuntu v1.6.", 1, ".")]
        [InlineData("005", "私が好きな OS は Ubuntu v1.6．", 1, "．")]
        [InlineData("006", "Re:VIEW フォーマットが好きだ。", 0, "")]
        [InlineData("007", "わたしは，カラオケが大好き．(ついでに)ボウリングも大好き．", 5, "，,．,．,(,)")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new InvalidSymbolConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new InvalidSymbolValidator(
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
