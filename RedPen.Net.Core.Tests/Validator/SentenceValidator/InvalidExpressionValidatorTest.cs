using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
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

            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(CultureInfo.GetCultureInfo("ja-JP")))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("オワタ。明日地球が滅亡するってマジですか。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var invalidExpressionValidator = new InvalidExpressionValidator(
                ValidationLevel.ERROR,
                documentLang,
                symbolTable,
                validatorConfiguration);

            invalidExpressionValidator.PreValidate(document.Sections[0].Paragraphs[0].Sentences[0]);
            var errors = invalidExpressionValidator.Validate(document.Sections[0].Paragraphs[0].Sentences[0]);

            errors.Count.Should().Be(2);

            // InvalidExpressionの場合、エラーの検出はWordListに入っている不正表現順に行われるので、エラーの順番もその順番で出力される。
            // TODO: 出力順が重要な場合はStartPositionなどでソートする必要がある。
            output.WriteLine(errors[0].ToString());
            output.WriteLine(errors[1].ToString());

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found invalid expression \"オワタ\".");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("不正な表現 \"オワタ\" が見つかりました。");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found invalid expression \"マジですか\".");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("不正な表現 \"マジですか\" が見つかりました。");
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
