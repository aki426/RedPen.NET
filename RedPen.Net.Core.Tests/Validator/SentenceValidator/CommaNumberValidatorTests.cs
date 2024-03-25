using Xunit;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class CommaNumberValidatorTests
    {
        [Fact]
        public void WithSentenceContainingManyCommasTest()
        {
            //Validator commaNumberValidator = ValidatorFactory.GetInstance("CommaNumber");
            //String content = "is it true, not true, but it should be ture, right, or not right.";
            //Sentence str = new Sentence(content, 0);
            //List<ValidationError> errors = new ArrayList<>();
            //commaNumberValidator.setErrorList(errors);
            //commaNumberValidator.validate(str);
            //assertNotNull(errors);
            //assertEquals(1, errors.size());
            //assertEquals(content, errors.get(0).getSentence().getContent());
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
