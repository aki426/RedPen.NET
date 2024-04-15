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
        private SentenceLengthValidator validator;// = new SentenceLengthValidator();
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Test SetUp.
        /// </summary>
        public SentenceLengthValidatorTests(ITestOutputHelper output)
        {
            this.output = output;

            var sentenceLengthConfiguration = new SentenceLengthConfiguration(ValidationLevel.ERROR, 30);
            validator = new SentenceLengthValidator(sentenceLengthConfiguration);

            validator.PreInit(
                sentenceLengthConfiguration,
                Configuration.Builder().Build());
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
            validator.ValidatorName.Should().Be("SentenceLength");

            // Cultureの設定
            validator.setLocale(new CultureInfo("en-US"));

            List<ValidationError> errors = new List<ValidationError>();
            validator.setErrorList(errors);
            validator.Validate(str);
            errors.Count.Should().Be(1);

            errors[0].ValidatorName.Should().Be("SentenceLength");
            errors[0].Message.Should().Be("The length of the sentence (84) exceeds the maximum of 30.");
        }

        /// <summary>
        /// エラーにならない場合のテスト
        /// </summary>
        [Fact]
        public void WithShortSentenceTest()
        {
            Sentence str = new Sentence("this is a sentence.", 0);
            List<ValidationError> errors = new List<ValidationError>();
            validator.setErrorList(errors);
            validator.Validate(str);

            errors.Count.Should().Be(0);
        }

        /// <summary>
        /// 空文字列に対してエラーを検出しないことのテスト
        /// </summary>
        [Fact]
        public void WithZeroLengthSentenceTest()
        {
            Sentence str = new Sentence("", 0);
            List<ValidationError> errors = new List<ValidationError>();
            validator.setErrorList(errors);
            validator.Validate(str);

            errors.Count.Should().Be(0);
        }
    }
}
