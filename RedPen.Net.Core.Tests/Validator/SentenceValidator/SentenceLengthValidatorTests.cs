using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validator;
using RedPen.Net.Core.Validator.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    /// <summary>
    /// The sentence length validator tests.
    /// </summary>
    public class SentenceLengthValidatorTests
    {
        private SentenceLengthValidator validator = new SentenceLengthValidator();
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Test SetUp.
        /// </summary>
        public SentenceLengthValidatorTests(ITestOutputHelper output)
        {
            this.output = output;

            validator.PreInit(
                new ValidatorConfiguration("SentenceLength").AddProperty("max_len", "30"),
                Configuration.Builder().Build());
        }

        /// <summary>
        /// tests the with long sentence.
        /// </summary>
        [Fact]
        public void testWithLongSentence()
        {
            object o = (object)120;
            (o is int).Should().BeTrue();

            Sentence str = new Sentence(
                "this is a very long long long long long long long long long long long long sentence.",
                0);

            validator.getProperties().Count.Should().Be(1);
            validator.getProperties().ContainsKey("max_len").Should().BeTrue();
            // Cultureの設定
            validator.setLocale(new CultureInfo("en-US"));

            List<ValidationError> errors = new List<ValidationError>();
            validator.setErrorList(errors);
            validator.Validate(str);
            errors.Count.Should().Be(1);

            errors[0].ValidatorName.Should().Be("SentenceLength");
            errors[0].Message.Should().Be("The length of the sentence (84) exceeds the maximum of 30.");
        }

        //    @Test

        //private void testWithShortSentence()
        //    {
        //        Sentence str = new Sentence("this is a sentence.", 0);
        //        List<ValidationError> errors = new ArrayList<>();
        //        validator.setErrorList(errors);
        //        validator.validate(str);
        //        assertEquals(0, errors.size());
        //    }

        //    @Test

        //private void testWithZeroLengthSentence()
        //    {
        //        Sentence str = new Sentence("", 0);
        //        List<ValidationError> errors = new ArrayList<>();
        //        validator.setErrorList(errors);
        //        validator.validate(str);
        //        assertEquals(0, errors.size());
        //    }
    }
}
