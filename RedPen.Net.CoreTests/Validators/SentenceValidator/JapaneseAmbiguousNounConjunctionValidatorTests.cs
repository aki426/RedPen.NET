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
    public class JapaneseAmbiguousNounConjunctionValidatorTests
    {
        private ITestOutputHelper output;

        public JapaneseAmbiguousNounConjunctionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "弊社の経営方針の説明を受けた。。", "", 1, "弊社の経営方針の説明")]
        [InlineData("002", "弊社の経営方針についての説明を受けた。", "", 0, "")]
        [InlineData("003", "不思議の国のアリスは面白い。", "不思議の国のアリス", 0, "")]
        // 「XのYのZ」という表現を検出するので連続する場合は複数検出される。
        [InlineData("004", "東京都の山手線の沿線のビルの屋根の看板を飛び越えていく。", "", 4,
            "東京都の山手線の沿線,山手線の沿線のビル,沿線のビルの屋根,ビルの屋根の看板")]
        public void JapaneseBasicTest(string nouse1, string text, string ignoreCases, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new JapaneseAmbiguousNounConjunctionConfiguration(
                ValidationLevel.ERROR,
                new HashSet<string>(ignoreCases.Split(','))
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new JapaneseAmbiguousNounConjunctionValidator(
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
