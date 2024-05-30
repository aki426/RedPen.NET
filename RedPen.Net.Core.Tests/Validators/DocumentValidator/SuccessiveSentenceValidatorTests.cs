using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.DocumentValidator
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
