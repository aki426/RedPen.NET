using System;
using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class InvalidExpressionValidatorTest
    {
        private readonly ITestOutputHelper output;

        public InvalidExpressionValidatorTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        //[Fact]
        //public void TestSimpleRun()
        //{
        //    var config = Configuration.Create()
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression").AddProperty("list", "may"));
        //    var validator = ValidatorFactory.GetInstance(config.ValidatorConfigs.First(), config);
        //    var errors = new List<ValidationError>();
        //    validator.SetErrorList(errors);
        //    validator.Validate(new Sentence("The experiments may be true.", 0));
        //    errors.Count.Should().Be(1);
        //}

        //[Fact]
        //public void TestVoid()
        //{
        //    var config = Configuration.Create()
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression").AddProperty("list", "may"));
        //    var validator = ValidatorFactory.GetInstance(config.ValidatorConfigs.First(), config);
        //    var errors = new List<ValidationError>();
        //    validator.SetErrorList(errors);
        //    validator.Validate(new Sentence("", 0));
        //    errors.Count.Should().Be(0);
        //}

        //[Fact]
        //public void TestLoadDefaultDictionary()
        //{
        //    var config = Configuration.Create()
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression"));

        //    var documents = new List<Document>
        //    {
        //        Document.Builder()
        //            .AddSection(1)
        //            .AddParagraph()
        //            .AddSentence(new Sentence("You know. He is a super man.", 1))
        //            .Build()
        //    };

        //    var redPen = new RedPen(config);
        //    var errors = redPen.Validate(documents).First().Value;
        //    errors.Count.Should().Be(1);
        //}

        // NOTE: デフォルトワードリストが日本語固定での仮実装なので、日本語のテストのみ実施する。

        [Fact]
        public void SimpleValidatoinOperationTest()
        {
            // DefaultResourceの読み込み。
            List<string> invalidWords = new List<string>();
            string v = DefaultResources.ResourceManager.GetString($"InvalidExpression_ja");
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                invalidWords.Add(line.Trim());
            }

            // ValidatorConfiguration
            var validatorConfiguration = new InvalidExpressionConfiguration(ValidationLevel.ERROR, invalidWords);

            // Configuration
            var config = Configuration.Builder("ja-JP")
                .AddValidatorConfig(validatorConfiguration)
                .Build();

            // Document
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(config.CultureInfo))
                .AddSection(1)
                .AddParagraph()
                .AddSentence(new Sentence("明日地球が滅亡するってマジですか。", 1))
                .Build();

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            // Validator
            var invalidExpressionValidator = new InvalidExpressionValidator(
                ValidationLevel.ERROR,
                config.CultureInfo,
                ValidationMessage.ResourceManager,
                config.SymbolTable,
                validatorConfiguration);

            invalidExpressionValidator.PreValidate(document.Sections[0].Paragraphs[0].Sentences[0]);
            var errors = invalidExpressionValidator.Validate(document.Sections[0].Paragraphs[0].Sentences[0]);

            errors.Count.Should().Be(1);

            //output.WriteLine(errors[0].Message);
            output.WriteLine(errors[0].Type.ToString());
            output.WriteLine(errors[0].Sentence.Content);
            output.WriteLine(errors[0].LineNumber.ToString());

            //errors[0].Message.Should().Be("不正な表現 \"マジですか\" がみつかりました。");
        }

        //[Fact]
        //public void TestLoadJapaneseDefaultDictionary()
        //{
        //    var config = Configuration.Builder("ja")
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression"))
        //        .Build();

        //    var documents = new List<Document>
        //    {
        //        Document.Builder(new NeologdJapaneseTokenizer())
        //            .AddSection(1)
        //            .AddParagraph()
        //            .AddSentence(new Sentence("明日地球が滅亡するってマジですか。", 1))
        //            .Build()
        //    };

        //    var redPen = new RedPen(config);
        //    var errors = redPen.Validate(documents);
        //    errors[documents.First()].Count.Should().Be(1);

        //    output.WriteLine(errors[documents.First()][0].Message);
        //    output.WriteLine(errors[documents.First()][0].ValidatorName);
        //    output.WriteLine(errors[documents.First()][0].Sentence.Content);
        //    output.WriteLine(errors[documents.First()][0].LineNumber.ToString());
        //}

        //[Fact]
        //public void TestLoadJapaneseInvalidList()
        //{
        //    var config = Configuration.Create("ja")
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression").AddProperty("list", "うふぉ,ガチ"));

        //    var documents = new List<Document>
        //    {
        //        Document.Builder(new NeologdJapaneseTokenizer())
        //            .AddSection(1)
        //            .AddParagraph()
        //            .AddSentence(new Sentence("うふぉっ本当ですか？", 1))
        //            .Build()
        //    };

        //    var redPen = new RedPen(config);
        //    var errors = redPen.Validate(documents).First().Value;
        //    errors.Count.Should().Be(1);
        //}

        //[Fact]
        //public void TestErrorPosition()
        //{
        //    var sampleText = "Hello You know."; // invalid expression "You know"
        //    var configuration = Configuration.Create()
        //        .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression"));
        //    var parser = DocumentParser.MARKDOWN;
        //    var documents = new List<Document>
        //    {
        //        parser.Parse(sampleText, new SentenceExtractor(configuration.SymbolTable), configuration.Tokenizer)
        //    };

        //    var redPen = new RedPen(configuration);
        //    var errors = redPen.Validate(documents).First().Value;
        //    errors.Count.Should().Be(1);
        //    errors[0].ValidatorName.Should().Be("InvalidExpression");
        //    errors[0].StartPosition.Value.Should().Be(new LineOffset(1, 6));
        //    errors[0].EndPosition.Value.Should().Be(new LineOffset(1, 14));
        //}
    }
}
