using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators.SentecneValidator;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    /// <summary>
    /// CommaNumberValidatorのテストケース。
    /// </summary>
    public class CommaNumberValidatorTests
    {
        /// <summary>
        /// Withs the sentence containing many commas test.
        /// </summary>
        [Fact]
        public void BasicPositiveTest()
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
            string variant = "";
            ValidationLevel level = ValidationLevel.ERROR;

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // ValidatorConfiguration
            var commaNumberConfiguration = new CommaNumberConfiguration(level, 3);

            // Validator
            var commaNumberValidator = new CommaNumberValidator(
                cultureInfo,
                symbolTable,
                commaNumberConfiguration);

            // Sentence
            Sentence sentence = new Sentence("is it true, not true, but it should be ture, right, or not right.", 0);

            // Validate
            commaNumberValidator.PreValidate(sentence);
            var errors = commaNumberValidator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("The number of commas (4) exceeds the maximum of 3.");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("カンマの数（4）が最大値（3）を超えています。");
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
            var commaNumberConfiguration = new CommaNumberConfiguration(level, 3);

            // Validator
            var commaNumberValidator = new CommaNumberValidator(
                cultureInfo,
                symbolTable,
                commaNumberConfiguration);

            // Sentence
            Sentence sentence = new Sentence("is it true.", 0);

            // Validate
            commaNumberValidator.PreValidate(sentence);
            var errors = commaNumberValidator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(0); // カンマがないセンテンスなのでエラーは出てこない。

            // Sentence
            sentence = new Sentence("", 0);

            // Validate
            commaNumberValidator.PreValidate(sentence);
            errors = commaNumberValidator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(0); // 空文字列でも正しくエラーは出てこない。
        }

        [Fact]
        public void JaCornerCaseTest()

        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("ja-JP");
            string variant = "";
            ValidationLevel level = ValidationLevel.ERROR;

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // ValidatorConfiguration
            var commaNumberConfiguration = new CommaNumberConfiguration(level, 3);

            // Validator
            var commaNumberValidator = new CommaNumberValidator(
                cultureInfo,
                symbolTable,
                commaNumberConfiguration);

            // Sentence
            Sentence sentence = new Sentence("しかし、この仕組みで背中を押した結果、挑戦できない人は、タスクに挑戦するようになりました。", 0);

            // Validate
            commaNumberValidator.PreValidate(sentence);
            var errors = commaNumberValidator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(0);

            // Sentence
            sentence = new Sentence("今日は、とても、良い天気で、お花見日和だ、と思います。", 0);

            // Validate
            commaNumberValidator.PreValidate(sentence);
            errors = commaNumberValidator.Validate(sentence);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("The number of commas (4) exceeds the maximum of 3.");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("カンマの数（4）が最大値（3）を超えています。");
        }
    }
}
