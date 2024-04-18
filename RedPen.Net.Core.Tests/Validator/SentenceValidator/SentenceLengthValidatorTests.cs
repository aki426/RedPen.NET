using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
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
        private SentenceLengthValidator validator;
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Test SetUp.
        /// </summary>
        public SentenceLengthValidatorTests(ITestOutputHelper output)
        {
            this.output = output;

            // あるValidatorを実行するために必要な最低限の設定は次の通り。

            // 1. 基本パラメータ
            CultureInfo cultureInfo = new CultureInfo("en-US");
            string variant = "";
            ValidationLevel level = ValidationLevel.ERROR;

            // 2. ValidatorConfiguration
            SentenceLengthConfiguration validatorConfiguration = new SentenceLengthConfiguration(level, 30);

            // 3. SymbolTable
            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルによるSymbolTableが生成される。
            SymbolTable symbolTable = new SymbolTable(cultureInfo, variant, new List<Symbol>());

            // 4. validator
            validator = new SentenceLengthValidator(
                level,
                cultureInfo,
                ValidationMessage.ResourceManager,
                symbolTable,
                validatorConfiguration);
        }

        /// <summary>
        /// tests the with long sentence.
        /// </summary>
        [Fact]
        public void testWithLongSentence()
        {
            Sentence str = new Sentence(
                "this is a very long long long long long long long long long long long long sentence.",
                0);

            validator.Config.MaxLength.Should().Be(30);
            validator.ValidationName.Should().Be("SentenceLength");

            validator.PreValidate(str);
            List<ValidationError> errors = validator.Validate(str);

            errors.Count.Should().Be(1);

            errors[0].Type.ToString().Should().Be("SentenceLength");
            errors[0].Message.Should().Be("The length of the sentence (84) exceeds the maximum of 30.");
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
