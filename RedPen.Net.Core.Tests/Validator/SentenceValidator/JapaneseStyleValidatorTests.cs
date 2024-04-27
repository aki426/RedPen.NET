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
    public class JapaneseStyleValidatorTests
    {
        private readonly ITestOutputHelper output;

        public JapaneseStyleValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void BasicTest()
        {
            var validatorConfiguration = new JapaneseStyleConfiguration(ValidationLevel.ERROR, JodoshiStyle.DesuMasu);
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            var validator = new JapaneseStyleValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("これは山です。これは川だ。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            output.WriteLine("★複合助動詞Token:");
            foreach (var token in JapaneseStyleValidator.GetCompoundJodoshi(sentence))
            {
                output.WriteLine(token.ToString());
            }

            output.WriteLine(sentence.ToString());

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(1);

            var manager = ErrorMessageManager.GetInstance();

            output.WriteLine(errors[0].ToString());
            output.WriteLine(manager.GetErrorMessage(errors[0], CultureInfo.GetCultureInfo("ja-JP")));
            errors[0].MessageArgs[0].ToString().Should().Be("だ");
        }
    }
}
