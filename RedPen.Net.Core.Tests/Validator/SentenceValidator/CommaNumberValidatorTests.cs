using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using RedPen.Net.Core.Validators.SentecneValidator;

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
        public void WithSentenceContainingManyCommasTest()
        {
            // ValidatorConfiguration
            var commaNumberConfiguration = new CommaNumberConfiguration(ValidationLevel.ERROR, 3);

            // Configuration
            var config = Configuration.Builder("en-US")
                .AddValidatorConfig(commaNumberConfiguration)
                .Build();

            // Sentence
            string content = "is it true, not true, but it should be ture, right, or not right.";
            Sentence str = new Sentence(content, 0);

            // Validator
            var commaNumberValidator = new CommaNumberValidator(
                config.CultureInfo,
                ValidationMessage.ResourceManager,
                config.SymbolTable,
                commaNumberConfiguration);

            // Validate
            commaNumberValidator.PreValidate(str);
            var errors = commaNumberValidator.Validate(str);

            // Assertion
            errors.Should().NotBeNull();
            errors.Count.Should().Be(1);
            errors[0].Sentence.Content.Should().Be(content);
            errors[0].Message.Should().Be("The number of commas (4) exceeds the maximum of 3.");
        }

        //    @Test
        //    void testWithtSentenceWithoutComma() throws RedPenException
        //    {
        //        Validator commaNumberValidator = ValidatorFactory.GetInstance("CommaNumber");
        //        String content = "is it true.";
        //        Sentence str = new Sentence(content, 0);
        //    List<ValidationError> errors = new ArrayList<>();
        //    commaNumberValidator.setErrorList(errors);
        //        commaNumberValidator.validate(str);
        //        assertNotNull(errors);
        //    assertEquals(0, errors.size());
        //}

        //@Test
        //    void testWithtZeroLengthSentence() throws RedPenException {
        //        Validator commaNumberValidator = ValidatorFactory.GetInstance("CommaNumber");
        //String content = "";
        //Sentence str = new Sentence(content, 0);
        //List<ValidationError> errors = new ArrayList<>();
        //commaNumberValidator.setErrorList(errors);
        //commaNumberValidator.validate(str);
        //assertNotNull(errors);
        //assertEquals(0, errors.size());
        //    }

        //    @Test
        //    void testJapaneseCornerCase() throws RedPenException {
        //        Configuration config = Configuration.builder("ja")
        //                .addValidatorConfig(new ValidatorConfiguration("CommaNumber"))
        //                .build();

        //Validator validator = ValidatorFactory.GetInstance(config.getValidatorConfigs().get(0), config);
        //List<ValidationError> errors = new ArrayList<>();
        //validator.setErrorList(errors);
        //validator.validate(new Sentence("しかし、この仕組みで背中を押した結果、挑戦できない人は、ゴールに到達するためのタスクに挑戦するようになりました。", 0));
        //assertEquals(0, errors.size());
        //    }
    }
}
