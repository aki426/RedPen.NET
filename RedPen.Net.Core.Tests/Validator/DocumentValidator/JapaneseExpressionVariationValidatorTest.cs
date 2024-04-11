using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.DocumentValidator
{
    /// <summary>
    /// The japanese expression variation validator test.
    /// </summary>
    public class JapaneseExpressionVariationValidatorTest : BaseValidatorTest
    {
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidatorTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public JapaneseExpressionVariationValidatorTest(ITestOutputHelper output) : base("JapaneseExpressionVariation")
        {
            this.output = output;
        }

        [Fact]
        public void MyTestMethod()
        {
            ValidatorConfiguration validatorConfiguration = new ValidatorConfiguration("JapaneseExpressionVariation");
            // MEMO: Configuration.Builder("ja")でTokenizerはNeologdが割り当たっている。
            Configuration localConfig = Configuration.Builder("ja")
                .AddValidatorConfig(validatorConfiguration)
                .Build();

            // Build中にNeologdでTokenizeされる。
            Document document = Document.Builder(localConfig.Tokenizer)
              .AddSection(1)
              .AddParagraph()
              .AddSentence(new Sentence("之は山です。これは川です。", 1))
              .Build();

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            JapaneseExpressionVariationValidator jaExpVariationValidator
                = ValidatorFactory.GetInstance(validatorConfiguration, localConfig) as JapaneseExpressionVariationValidator;
            jaExpVariationValidator.PreValidate(document);

            List<ValidationError> errors = new List<ValidationError>();
            jaExpVariationValidator.setErrorList(errors);
            jaExpVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(1);

            // TODO: 次のテストケースはあくまで暫定。
            errors[0].Message.Should().Be("単語 ”之” の揺らぎと考えられる表現 ”これ(名詞)” が (L1,6)　で見つかりました。");

            output.WriteLine(errors[0].Message);
            output.WriteLine(errors[0].ValidatorName);
            output.WriteLine(errors[0].Sentence.Content);
            output.WriteLine(errors[0].LineNumber.ToString());

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            _ = jaExpVariationValidator.readingMap[document];

            foreach (KeyValuePair<string, List<JapaneseExpressionVariationValidator.TokenInfo>> readingInfo in
                jaExpVariationValidator.readingMap[document])
            {
                readingInfo.Value.ForEach(tokenInfo =>
                {
                    output.WriteLine($"{readingInfo.Key} => {tokenInfo}");
                });
            }
        }

        /// <summary>
        /// detects the same readings in japanese characters.
        /// </summary>
        [Fact]
        public void detectSameReadingsInJapaneseCharacters()
        {
            // MEMO: Configuration.Builder("ja")でTokenizerはNeologdが割り当たっている。
            config = Configuration.Builder("ja")
                         .AddValidatorConfig(new ValidatorConfiguration(validatorName))
                         .Build();

            Document document = prepareSimpleDocument("之は山です。これは川です。");

            RedPen redPen = new RedPen(config);
            Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors[document].Count().Should().Be(1);

            // TODO: 次のテストケースはあくまで暫定。
            errors[document][0].Message.Should().Be("単語 ”之” の揺らぎと考えられる表現 ”これ(名詞)” が (L1,6)　で見つかりました。");

            output.WriteLine(errors[document][0].Message);
            output.WriteLine(errors[document][0].ValidatorName);
            output.WriteLine(errors[document][0].Sentence.Content);
            output.WriteLine(errors[document][0].LineNumber.ToString());
        }

        [Fact]
        public void detectSameReadingsInJapaneseCharactersInDefaultDictionary()
        {
            // MEMO: Configuration.Builder("ja")でTokenizerはNeologdが割り当たっている。
            config = Configuration.Builder("ja")
                .AddValidatorConfig(new ValidatorConfiguration(validatorName))
                .Build();

            Document document = prepareSimpleDocument("nodeは英語です。ノードはカタカナです。");

            RedPen redPen = new RedPen(config);
            Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors[document].Count.Should().Be(1);

            // TODO: 次のテストケースはあくまで暫定。
            errors[document][0].Message.Should().Be("単語 ”node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");

            output.WriteLine(errors[document][0].Message);
            output.WriteLine(errors[document][0].ValidatorName);
            output.WriteLine(errors[document][0].Sentence.Content);
            output.WriteLine(errors[document][0].LineNumber.ToString());
        }

        [Fact]
        public void detectSameReadingsInJapaneseCharactersInDefaultDictionaryWithUpperCase()
        {
            // MEMO: Configuration.Builder("ja")でTokenizerはNeologdが割り当たっている。
            config = Configuration.Builder("ja")
                .AddValidatorConfig(new ValidatorConfiguration(validatorName))
                .Build();

            Document document = prepareSimpleDocument("Nodeは英語です。ノードはカタカナです。");

            RedPen redPen = new RedPen(config);
            Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors[document].Count.Should().Be(1);

            // TODO: 次のテストケースはあくまで暫定。
            errors[document][0].Message.Should().Be("単語 ”Node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");

            output.WriteLine(errors[document][0].Message);
            output.WriteLine(errors[document][0].ValidatorName);
            output.WriteLine(errors[document][0].Sentence.Content);
            output.WriteLine(errors[document][0].LineNumber.ToString());
        }

        [Fact(Skip = "JAVA版ベタ移植でもREDのため一旦SKIPして進める")]
        public void detectSameAlphabecicalReadings()
        {
            config = Configuration
                .Builder("ja")
                .AddValidatorConfig(new ValidatorConfiguration(validatorName))
                .Build();

            Document document = prepareSimpleDocument("このExcelはあのエクセルとは違います。");

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            RedPen redPen = new RedPen(config);
            Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });
            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors[document].Count.Should().Be(1);

            // TODO: 次のテストケースはあくまで暫定。
            errors[document][0].Message.Should().Be("単語 ”Node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");
        }

        //    @Test
        //    void detectSameAlphabecicalReadingsInUserDictionary() throws RedPenException {
        //        config = Configuration.builder("ja")
        //                         .addValidatorConfig(new ValidatorConfiguration(validatorName).addProperty("map", "{svm,サポートベクタマシン}"))
        //                         .build();

        //Document document = prepareSimpleDocument("このSVMはあのサポートベクタマシンとは違います。");

        //RedPen redPen = new RedPen(config);
        //Map<Document, List<ValidationError>> errors = redPen.validate(singletonList(document));
        //assertEquals(1, errors.get(document).size());
        //    }

        //    @Test
        //    void detectNormalizedReadings() throws RedPenException {
        //        config = Configuration.builder("ja")
        //                         .addValidatorConfig(new ValidatorConfiguration(validatorName))
        //                         .build();

        //Document document = prepareSimpleDocument("このインデックスはあのインデクスとは違います。");

        //RedPen redPen = new RedPen(config);
        //Map<Document, List<ValidationError>> errors = redPen.validate(singletonList(document));
        //assertEquals(1, errors.get(document).size());
        //    }

        //    @Test
        //    void detectNormalizedReadings2() throws RedPenException {
        //        config = Configuration.builder("ja")
        //                         .addValidatorConfig(new ValidatorConfiguration(validatorName))
        //                         .build();

        //Document document = prepareSimpleDocument("このヴェトナムはあのベトナムとは違います。");

        //RedPen redPen = new RedPen(config);
        //Map<Document, List<ValidationError>> errors = redPen.validate(singletonList(document));
        //assertEquals(1, errors.get(document).size());
        //    }

        //    @Test
        //    void detectSameReadingsInConcatinatedJapaneseWord() throws RedPenException {
        //        config = Configuration.builder("ja")
        //                         .addValidatorConfig(new ValidatorConfiguration(validatorName))
        //                         .build();

        //Document document = prepareSimpleDocument("身分証明書は紙です。身分証明所は間違い。");

        //RedPen redPen = new RedPen(config);
        //Map<Document, List<ValidationError>> errors = redPen.validate(singletonList(document));
        //assertEquals(1, errors.get(document).size());
        //    }

        //    @Test
        //    void detectMultipleSameReadings() throws RedPenException {
        //        config = Configuration.builder("ja")
        //                .addValidatorConfig(new ValidatorConfiguration(validatorName))
        //                .build();

        //Document document = prepareSimpleDocument("このExcelはあのエクセルともこのエクセルとも違います。");

        //RedPen redPen = new RedPen(config);
        //Map<Document, List<ValidationError>> errors = redPen.validate(singletonList(document));
        //assertEquals(1, errors.get(document).size());
        //    }
    }
}
