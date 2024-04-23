using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tests.Parser;
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
            errors.Count().Should().Be(6);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            output.WriteLine("");
            output.WriteLine("--- Error messages. ---");
            errors.ForEach(e =>
            {
                output.WriteLine(manager.GetErrorMessage(e, CultureInfo.GetCultureInfo("ja-JP")));
            });

            //manager.GetErrorMessage(
            //    errors[0],
            //    CultureInfo.GetCultureInfo("en-US"))
            //        .Should().Be("Found possible Japanese word variations for \"之(名詞)\", \"これ(名詞)\" at (L1,6), (L1,13)");

            //manager.GetErrorMessage(
            //    errors[0],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"之(名詞)\" は \"これ(名詞)\"（出現位置：(L1,6), (L1,13)）の揺らぎ表現と考えられます。");

            //manager.GetErrorMessage(
            //    errors[1],
            //    CultureInfo.GetCultureInfo("en-US"))
            //        .Should().Be("Found possible Japanese word variations for \"ヴェトナム(名詞)\", \"ベトナム(名詞)\" at (L3,10), (L3,19)");

            //manager.GetErrorMessage(
            //    errors[1],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"ヴェトナム(名詞)\" は \"ベトナム(名詞)\"（出現位置：(L3,10), (L3,19)）の揺らぎ表現と考えられます。");

            //manager.GetErrorMessage(
            //    errors[2],
            //    CultureInfo.GetCultureInfo("en-US"))
            //        .Should().Be("Found possible Japanese word variations for \"大使館(名詞)\", \"ヴェトナム大使館(名詞)\" at (L3,0)");

            //manager.GetErrorMessage(
            //    errors[2],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"大使館(名詞)\" は \"ヴェトナム大使館(名詞)\"（出現位置：(L3,0)）の揺らぎ表現と考えられます。");
        }

        [Fact]
        public void DefaultDictionaryTest()
        {
            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("nodeは英語です。ノードはカタカナです。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"node(名詞)\" は \"ノード(名詞)\"（出現位置：(L1,10)）の揺らぎ表現と考えられます。");
        }

        [Fact]
        public void DefaultDictionaryNothingTest()
        {
            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("このExcelはあのエクセルとは違います。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // MEMO: Excelはデフォルト辞書に「エクセル」というReadingが登録されていないのでゆらぎと判定されない。
            errors.Count().Should().Be(0);

            //// 7. エラーメッセージを生成する。
            //var manager = ErrorMessageManager.GetInstance();

            //manager.GetErrorMessage(
            //    errors[0],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"Excel\" は \"エクセル(名詞)\"（出現位置：(L1,10)）の揺らぎ表現と考えられます。");
        }

        /// <summary>
        /// デフォルト辞書に登録されているnodeのReading「ノード」が本文中に現れない場合もゆらぎ表現としてエラーとなることを確認するテスト。
        /// </summary>
        [Fact]
        public void OnlyDefaultDictionaryVariationTest()
        {
            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("nodeは英語です。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            // TODO: 現状、デフォルト辞書のReadingが本文中に現れない場合はエラーにはならない。
            // つまりデフォルト辞書はあくまでもReadingのマップであり、そのReadingを同じくする別のSrufaceが現れない限り
            // エラーを検出しない。これは仕様としてよいのか？
            errors.Count().Should().Be(0);

            //// 7. エラーメッセージを生成する。
            //var manager = ErrorMessageManager.GetInstance();

            //manager.GetErrorMessage(
            //    errors[0],
            //    CultureInfo.GetCultureInfo("en-US"))
            //        .Should().Be("Found possible Japanese word variations for \"node\", \"ノード(名詞)\" at (L1,10)");

            //manager.GetErrorMessage(
            //    errors[0],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"node\" は \"ノード(名詞)\"（出現位置：(L1,10)）の揺らぎ表現と考えられます。");
        }

        [Fact]
        public void AppearPositionTest()
        {
            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("肖像権が正解で、昭三権は実在しない用語でありゆらぎ表現です。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(1);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations for \"肖像(名詞)\", \"昭三(名詞)\" at (L1,8)");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"肖像(名詞)\" は \"昭三(名詞)\"（出現位置：(L1,8)）の揺らぎ表現と考えられます。");

            // Document
            document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("昭三権は実在しない用語でありゆらぎ表現で、肖像権が正解です。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(1);

            // 7. エラーメッセージを生成する。
            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations for \"昭三(名詞)\", \"肖像(名詞)\" at (L1,21)");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"昭三(名詞)\" は \"肖像(名詞)\"（出現位置：(L1,21)）の揺らぎ表現と考えられます。");
        }

        // MEMO: 現在の仕様では、ゆらぎと判断される2つの表現があった場合、先に出現したものがゆらぎ、後に出現したものが正としてエラーが出力される。
        // このようなケースでは、例えば数が多いほうを正、少ないほうをゆらぎとするなどの判断が必要になる。
        // 完全に同数だった場合は、エラーメッセージのバリエーションを変えて対応する必要がある。
        // 例）『"XXX"（出現位置：(Lx,xx)）と "YYY"（出現位置：(Ly,yyy)）はどちらかが揺らぎ表現と考えられます。』
        // この場合すべての出現箇所をエラーメッセージとして出す必要がある？

        // TODO: ごく短い1文字の表現もゆらぎとしてカウントされると良くないので、テストケースを書いて検討する。
        // 最終的にはMinLengthパラメータを実装して、指定文字数以下の表現はゆらぎとしてカウントしないようにする必要がありそう。

        [Fact]
        public void ValidatePlainTextTest()
        {
            string sampleText = @"今日、メロスは激怒した。
必ず、かの邪智暴虐の王を除かなければならぬと決意した。

メロスには政治がわからぬ。メロスは、京の牧人である。笛を吹き、羊と遊んで暮して来た。
けれども邪悪に対しては、人一倍に敏感であった。

きょう未明メロスは村を出発し、
野を越え山越え、今
日の午後、十里はなれた此のシラクスの市にやってきた。
できたできた。ようやく出来た。";

            // Document
            var plainTextParserTests = new PlainTextParserTests(output);
            Document document = plainTextParserTests.GenerateDocument(sampleText, "ja-JP");

            // 目視確認。
            document.Sections[0].Paragraphs[0].Sentences.ForEach(sentence =>
            {
                output.WriteLine(sentence.Content);
                sentence.Tokens.ForEach(token =>
                {
                    output.WriteLine(token.ToString());
                });
            });

            // Validation
            japaneseExpressionVariationValidator.PreValidate(document);
            List<ValidationError> errors = japaneseExpressionVariationValidator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            //errors.Count().Should().Be(3);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            // 目視確認。
            errors.ForEach(e =>
            {
                output.WriteLine(manager.GetErrorMessage(e, CultureInfo.GetCultureInfo("ja-JP")));
            });

            //manager.GetErrorMessage(
            //    errors[2],
            //    CultureInfo.GetCultureInfo("ja-JP"))
            //        .Should().Be("\"大使館\" は \"ヴェトナム大使館(名詞)\"（出現位置：(L3,0)）の揺らぎ表現と考えられます。");
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
