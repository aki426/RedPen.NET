using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class TaigendomeValidatorTests
    {
        private readonly ITestOutputHelper output;

        public TaigendomeValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "吾輩は猫。", 1, "猫")]
        [InlineData("002", "吾輩は猫だ。", 0, "")]
        [InlineData("003", "ねこですよろしくおねがいします", 0, "")]
        [InlineData("004", "これは体言止め？", 1, "止め")]
        [InlineData("005", "今日の天気は晴れ。", 1, "晴れ")]
        [InlineData("006", "今日の天気は晴れです。", 0, "")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)
        {
            var validatorConfiguration = new TaigendomeConfiguration(ValidationLevel.ERROR);
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            var validator = new TaigendomeValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence(text, 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(errorCount);

            if (errors.Any())
            {
                var manager = ErrorMessageManager.GetInstance();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }
    }
}
