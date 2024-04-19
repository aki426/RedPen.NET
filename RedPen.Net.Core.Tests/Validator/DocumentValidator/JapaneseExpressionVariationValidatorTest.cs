using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;
using RedPenTokenizerFactory = RedPen.Net.Core.Tokenizer.RedPenTokenizerFactory;

namespace RedPen.Net.Core.Tests.Validator.DocumentValidator
{
    /// <summary>
    /// The japanese expression variation validator test.
    /// </summary>
    public class JapaneseExpressionVariationValidatorTest : BaseValidatorTest
    {
        private readonly ITestOutputHelper output;

        private CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

        private Dictionary<string, string> expressionVariationMap;

        private JapaneseExpressionVariationConfiguration japaneseExpressionVariationConfiguration;

        private JapaneseExpressionVariationValidator japaneseExpressionVariationValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidatorTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public JapaneseExpressionVariationValidatorTest(ITestOutputHelper output) : base("JapaneseExpressionVariation")
        {
            this.output = output;

            // MEMO: ゆらぎ表現のマップ、ValidatorConfigurationの設定、JapaneseExpressionVariationValidatorの生成までは、
            // 各テストケースで共通なので、コンストラクタで実行してしまってよい。
            // Documentの違いはPreValidateとValidateで対応可能。

            // DefaultResourceの読み込み。
            Dictionary<string, string> expressionVariationMap = new Dictionary<string, string>();
            string v = DefaultResources.ResourceManager.GetString($"SpellingVariation_ja");
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] result = line.Split('\t');

                if (result.Length == 2)
                {
                    expressionVariationMap[result[0]] = result[1];
                }
                else
                {
                    // log.Error("Skip to load line... Invalid line: " + line);
                }
            }

            // ValidatorConfigurationの生成。
            japaneseExpressionVariationConfiguration =
                new JapaneseExpressionVariationConfiguration(ValidationLevel.ERROR, expressionVariationMap);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            japaneseExpressionVariationValidator = new JapaneseExpressionVariationValidator(
                ValidationLevel.ERROR,
                documentLang,
                symbolTable,
                japaneseExpressionVariationConfiguration);
        }

        [Fact]
        public void BasicOperationTest()
        {
            // ValidatorConfiguration内設定値の確認。
            foreach (var item in japaneseExpressionVariationConfiguration.WordMap)
            {
                output.WriteLine($"{item.Key} => {item.Value}");
            }

            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("之は山です。これは川です。これはヴェトナムの地図です。", 1))
                    .AddSentence(new Sentence("之も之も海です。", 2))
                    .AddSentence(new Sentence("ヴェトナム大使館にはベトナム人の大使とベトナム人の職員が常駐しています。", 3))
                    .Build(); // TokenizeをBuild時に実行する。

            document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
            {
                output.WriteLine(token.ToString());
            });

            document.Sections[0].Paragraphs[0].Sentences.ForEach(sentence =>
            {
                output.WriteLine(sentence.Content);
            });

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(3);

            errors.ForEach(e =>
            {
                foreach (object arg in e.MessageArgs)
                {
                    output.WriteLine(arg.ToString());
                }
            });

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations for \"之\", \"これ(名詞)\" at (L1,6), (L1,13)");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"之\" は \"これ(名詞)\"（出現位置：(L1,6), (L1,13)）の揺らぎ表現と考えられます。");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations for \"ヴェトナム\", \"ベトナム(名詞)\" at (L3,10), (L3,19)");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"ヴェトナム\" は \"ベトナム(名詞)\"（出現位置：(L3,10), (L3,19)）の揺らぎ表現と考えられます。");

            manager.GetErrorMessage(
                errors[2],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations for \"大使館\", \"ヴェトナム大使館(名詞)\" at (L3,0)");

            manager.GetErrorMessage(
                errors[2],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"大使館\" は \"ヴェトナム大使館(名詞)\"（出現位置：(L3,0)）の揺らぎ表現と考えられます。");
        }

        ///// <summary>
        ///// detects the same readings in japanese characters.
        ///// </summary>
        //[Fact]
        //public void detectSameReadingsInJapaneseCharacters()
        //{
        //    // MEMO: Configuration.Builder("ja-JP")でTokenizerはNeologdが割り当たっている。
        //    config = Configuration.Builder("ja-JP")
        //                 .AddValidatorConfig(new ValidatorConfiguration(validatorName))
        //                 .Build();

        //    Document document = prepareSimpleDocument("之は山です。これは川です。");

        //    RedPen redPen = new RedPen(config);
        //    Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

        //    // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
        //    errors[document].Count().Should().Be(1);

        //    // TODO: 次のテストケースはあくまで暫定。
        //    errors[document][0].Message.Should().Be("単語 ”之” の揺らぎと考えられる表現 ”これ(名詞)” が (L1,6)　で見つかりました。");

        //    output.WriteLine(errors[document][0].Message);
        //    output.WriteLine(errors[document][0].ValidationName);
        //    output.WriteLine(errors[document][0].Sentence.Content);
        //    output.WriteLine(errors[document][0].LineNumber.ToString());
        //}

        //[Fact]
        //public void detectSameReadingsInJapaneseCharactersInDefaultDictionary()
        //{
        //    // MEMO: Configuration.Builder("ja-JP")でTokenizerはNeologdが割り当たっている。
        //    config = Configuration.Builder("ja-JP")
        //        .AddValidatorConfig(new ValidatorConfiguration(validatorName))
        //        .Build();

        //    Document document = prepareSimpleDocument("nodeは英語です。ノードはカタカナです。");

        //    RedPen redPen = new RedPen(config);
        //    Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

        //    // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
        //    errors[document].Count.Should().Be(1);

        //    // TODO: 次のテストケースはあくまで暫定。
        //    errors[document][0].Message.Should().Be("単語 ”node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");

        //    output.WriteLine(errors[document][0].Message);
        //    output.WriteLine(errors[document][0].ValidationName);
        //    output.WriteLine(errors[document][0].Sentence.Content);
        //    output.WriteLine(errors[document][0].LineNumber.ToString());
        //}

        //[Fact]
        //public void detectSameReadingsInJapaneseCharactersInDefaultDictionaryWithUpperCase()
        //{
        //    // MEMO: Configuration.Builder("ja-JP")でTokenizerはNeologdが割り当たっている。
        //    config = Configuration.Builder("ja-JP")
        //        .AddValidatorConfig(new ValidatorConfiguration(validatorName))
        //        .Build();

        //    Document document = prepareSimpleDocument("Nodeは英語です。ノードはカタカナです。");

        //    RedPen redPen = new RedPen(config);
        //    Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });

        //    // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
        //    errors[document].Count.Should().Be(1);

        //    // TODO: 次のテストケースはあくまで暫定。
        //    errors[document][0].Message.Should().Be("単語 ”Node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");

        //    output.WriteLine(errors[document][0].Message);
        //    output.WriteLine(errors[document][0].ValidationName);
        //    output.WriteLine(errors[document][0].Sentence.Content);
        //    output.WriteLine(errors[document][0].LineNumber.ToString());
        //}

        //[Fact(Skip = "JAVA版ベタ移植でもREDのため一旦SKIPして進める")]
        //public void detectSameAlphabecicalReadings()
        //{
        //    config = Configuration
        //        .Builder("ja-JP")
        //        .AddValidatorConfig(new ValidatorConfiguration(validatorName))
        //        .Build();

        //    Document document = prepareSimpleDocument("このExcelはあのエクセルとは違います。");

        //    document.Sections[0].Paragraphs[0].Sentences[0].Tokens.ForEach(token =>
        //    {
        //        output.WriteLine(token.ToString());
        //    });

        //    RedPen redPen = new RedPen(config);
        //    Dictionary<Document, List<ValidationError>> errors = redPen.Validate(new List<Document>() { document });
        //    // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
        //    errors[document].Count.Should().Be(1);

        //    // TODO: 次のテストケースはあくまで暫定。
        //    errors[document][0].Message.Should().Be("単語 ”Node” の揺らぎと考えられる表現 ”ノード(名詞)” が (L1,10)　で見つかりました。");
        //}

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
