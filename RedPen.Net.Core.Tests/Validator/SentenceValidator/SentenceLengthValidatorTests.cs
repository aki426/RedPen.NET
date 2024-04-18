using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    /// <summary>
    /// The sentence length validator tests.
    /// </summary>
    public class SentenceLengthValidatorTests
    {
        // テスト間で共通の設定を使う場合は、クラス変数を用いる。

        private readonly ITestOutputHelper output;

        // あるValidatorを実行するために必要な最低限の設定は次の1~の手順。

        // 1. 基本パラメータ
        private CultureInfo cultureInfo = new CultureInfo("en-US");

        private string variant = "";
        private ValidationLevel level = ValidationLevel.ERROR;
        private SentenceLengthValidator validator;
        private string messageKey = "";

        /// <summary>
        /// Test SetUp.
        /// </summary>
        public SentenceLengthValidatorTests(ITestOutputHelper output)
        {
            this.output = output;

            // 2. ValidatorConfiguration
            SentenceLengthConfiguration validatorConfiguration = new SentenceLengthConfiguration(level, 30);

            // 3. SymbolTable
            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // 4. validator
            validator = new SentenceLengthValidator(
                level,
                cultureInfo,
                //ValidationMessage.ResourceManager,
                symbolTable,
                validatorConfiguration);
        }

        /// <summary>
        /// tests the with long sentence.
        /// </summary>
        [Fact]
        public void testWithLongSentence()
        {
            validator.Config.MaxLength.Should().Be(30);
            validator.ValidationName.Should().Be("SentenceLength");

            // 5. テスト対象のオブジェクトを生成する。
            Sentence str = new Sentence(
                "this is a very long long long long long long long long long long long long sentence.",
                0);

            // 6. PreValidate, Validateを実行する。
            validator.PreValidate(str);
            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(1);
            errors[0].Type.Should().Be(ValidationType.SentenceLength);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("The length of the sentence (84) exceeds the maximum of 30.");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("文の長さ（84）が最大値（30）を超えています。");
        }

        /// <summary>
        /// エラーにならない場合のテスト
        /// </summary>
        [Fact]
        public void WithShortSentenceTest()
        {
            Sentence str = new Sentence("this is a sentence.", 0);

            validator.PreValidate(str);
            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(0);
        }

        /// <summary>
        /// 空文字列に対してエラーを検出しないことのテスト
        /// </summary>
        [Fact]
        public void WithZeroLengthSentenceTest()
        {
            Sentence str = new Sentence("", 0);

            validator.PreValidate(str);
            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(0);
        }
    }
}
