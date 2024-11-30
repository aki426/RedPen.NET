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
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators.Tests;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.DocumentValidator.Tests
{
    public class SuccessiveSentenceValidatorTests
    {
        private ITestOutputHelper output;

        public SuccessiveSentenceValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "This is a pen. This is a pen.", 3, 5, 1, "This is a pen.")]
        [InlineData("002", "This is a pen. That is another pen.", 3, 5, 0, "")]
        [InlineData("003", "This is a pen. That is another pen.", 10, 5, 1, "This is a pen.")]
        [InlineData("004", "THIS IS A PEN. This is a pen.", 3, 5, 1, "THIS IS A PEN.")]
        [InlineData("005", "Hoge. Hoge.", 3, 10, 0, "")]
        [InlineData("006", "Hoge. Hoge.", 3, 3, 1, "Hoge.")]
        public void BasicTest(string nouse1, string text, int maxDistance, int minLength, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // ValidatorConfiguration
            var validatorConfiguration = new SuccessiveSentenceConfiguration(
                ValidationLevel.ERROR,
                maxDistance,
                minLength
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SuccessiveSentenceValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonDocumentWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Fact]
        public void LevenSteinDistanceTest()
        {
            LevenshteinDistanceUtility utility = new LevenshteinDistanceUtility();

            utility.GetDistance("This is a pen.", " This is a pen.").Should().Be(1);
        }

        [Theory]
        [InlineData("001", "これはペンです。これはペンです。", 3, 5, 1, "これはペンです。")]
        [InlineData("002", "これはペンです。これはシャープペンシルです。", 3, 5, 0, "")]
        [InlineData("003", "これはペンです。これはシャープペンシルです。", 10, 5, 1, "これはペンです。")]
        [InlineData("004", "これはペンです。これは筆です。", 3, 5, 1, "これはペンです。")]
        [InlineData("005", "ワッフル。ワッフル。", 3, 10, 0, "")]
        [InlineData("006", "ワッフル。ワッフル。", 3, 3, 1, "ワッフル。")]
        public void JapaneseBasicTest(string nouse1, string text, int maxDistance, int minLength, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new SuccessiveSentenceConfiguration(
                ValidationLevel.ERROR,
                maxDistance,
                minLength
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SuccessiveSentenceValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonDocumentWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }
    }
}
