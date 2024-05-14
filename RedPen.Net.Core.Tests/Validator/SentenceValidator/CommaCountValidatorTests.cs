using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Lucene.Net.Util.Fst;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentecneValidator;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    /// <summary>
    /// CommaNumberValidatorのテストケース。
    /// </summary>
    public class CommaCountValidatorTests
    {
        private ITestOutputHelper output;

        public CommaCountValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "is it true, not true, but it should be ture, right, or not right.", 3, 1, "4")]
        [InlineData("002", "is it true, not true, but it should be ture, right, or not right.", 4, 1, "4")]
        [InlineData("003", "is it true, not true, but it should be ture, right, or not right.", 5, 0, "")]
        public void EnBasicTest(string nouse1, string text, int minCount, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // ValidatorConfiguration
            var validatorConfiguration = new CommaCountConfiguration(ValidationLevel.ERROR, minCount);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new CommaCountValidator(
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

        [Fact]
        public void NegativeTest()

        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
            string variant = "";
            ValidationLevel level = ValidationLevel.ERROR;

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // ValidatorConfiguration
            var validatorConfiguration = new CommaCountConfiguration(level, 3);

            // Validator
            var validator = new CommaCountValidator(
                cultureInfo,
                symbolTable,
                validatorConfiguration);

            // Sentence
            Sentence sentence = new Sentence("is it true.", 0);

            // Validate
            var errors = validator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(0); // カンマがないセンテンスなのでエラーは出てこない。

            // Sentence
            sentence = new Sentence("", 0);

            // Validate
            errors = validator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(0); // 空文字列でも正しくエラーは出てこない。
        }

        [Theory]
        [InlineData("001", "しかし、この仕組みで背中を押した結果、挑戦できない人は、タスクに挑戦するようになりました。", 4, 0, "")]
        [InlineData("002", "しかし、この仕組みで背中を押した結果、挑戦できない人は、タスクに挑戦するようになりました。", 3, 1, "3")]
        [InlineData("003", "今日は、とても、良い天気で、お花見日和だ、と思います。", 3, 1, "4")]
        [InlineData("004", "今日は、とても、良い天気で、お花見日和だ、と思います。", 4, 1, "4")]
        [InlineData("005", "今日は、とても、良い天気で、お花見日和だ、と思います。", 5, 0, "")]
        public void JaBasicTest(string nouse1, string text, int minCount, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new CommaCountConfiguration(ValidationLevel.ERROR, minCount);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new CommaCountValidator(
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
