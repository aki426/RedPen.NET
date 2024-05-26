using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
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
    public class JapaneseWordVariationValidatorTest
    {
        private readonly ITestOutputHelper output;

        private CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

        private JapaneseWordVariationConfiguration validatorConfiguration;

        private JapaneseWordVariationValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseWordVariationValidatorTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public JapaneseWordVariationValidatorTest(ITestOutputHelper output)
        {
            this.output = output;

            // MEMO: ゆらぎ表現のマップ、ValidatorConfigurationの設定、JapaneseExpressionVariationValidatorの生成までは、
            // 各テストケースで共通なので、コンストラクタで実行してしまってよい。
            // Documentの違いはValidateで対応可能。

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
            validatorConfiguration =
                new JapaneseWordVariationConfiguration(ValidationLevel.ERROR, expressionVariationMap);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            validator = new JapaneseWordVariationValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);
        }

        [Fact]
        public void BasicOperationTest()
        {
            // ValidatorConfiguration内設定値の確認。
            foreach (var item in validatorConfiguration.WordMap)
            {
                output.WriteLine($"{item.Key} => {item.Value}");
            }

            // PlainTextParseにより原文をParseする。複数センテンスの場合これでセンテンス分割ができる。
            string text = @"之は山です。これは川です。これはヴェトナムの地図です。
之も之も海です。
ヴェトナム大使館にはベトナム人の大使とベトナム人の職員が常駐しています。
";
            Document document = null;
            // Lang設定以外はデフォルト。
            Configuration configuration = Configuration.Builder()
                .SetLang(documentLang.Name)
                .Build();

            // Parse
            try
            {
                document = new PlainTextParser().Parse(
                    text,
                    new SentenceExtractor(configuration.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(configuration.CultureInfo));
            }
            catch (Exception e)
            {
                Assert.True(false, "Exception not expected.");
            }

            // 全センテンスのTokenを目視確認。
            foreach (var sentence in document.GetAllSentences())
            {
                output.WriteLine("★全Token:");
                foreach (var token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");
            }

            // Validation
            List<ValidationError> errors = validator.Validate(document);

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

            string.Join("", errors.Select(e => manager.GetErrorMessage(e, CultureInfo.GetCultureInfo("ja-JP")))).Should().Be(
                "\"これ(名詞)\" は \"之(名詞)\"（出現位置：(L1,0), (L2,0), (L2,2)）の揺らぎ表現と考えられます。" +
                "\"これ(名詞)\" は \"之(名詞)\"（出現位置：(L1,0), (L2,0), (L2,2)）の揺らぎ表現と考えられます。" +
                "\"ヴェトナム(名詞)\" と \"ベトナム(名詞)\"（出現位置：(L3,10), (L3,19)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。" +
                "\"ヴェトナム(名詞)\" と \"ベトナム(名詞)\"（出現位置：(L3,10), (L3,19)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。" +
                "\"ベトナム(名詞)\" と \"ヴェトナム(名詞)\"（出現位置：(L1,16), (L3,0)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。" +
                "\"ベトナム(名詞)\" と \"ヴェトナム(名詞)\"（出現位置：(L1,16), (L3,0)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。"
            );
        }

        /// <summary>
        /// デフォルトディクショナリに登録されているnode->ノードを検証するテスト。
        /// </summary>
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
            List<ValidationError> errors = validator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(2);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            // デフォルトディクショナリに登録されている辞書データは、あくまでReadingの指定であるため、Variationとして処理されるためには、
            // 本文中にそのReadingに対応するSurfaceが出現している必要がある。
            // また、辞書登録が優先されるわけではないので、同数出現していれば双方をエラーとして検出する。

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"node(名詞)\" と \"ノード(名詞)\"（出現位置：(L1,10)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"ノード(名詞)\" と \"node(名詞)\"（出現位置：(L1,0)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。");
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
            List<ValidationError> errors = validator.Validate(document);

            // MEMO: Excelはデフォルト辞書に「エクセル」というReadingが登録されていないのでゆらぎと判定されない。
            errors.Count().Should().Be(0);
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
            List<ValidationError> errors = validator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            // TODO: 現状、デフォルト辞書のReadingが本文中に現れない場合はエラーにはならない。
            // つまりデフォルト辞書はあくまでもReadingのマップであり、そのReadingを同じくする別のSrufaceが現れない限り
            // エラーを検出しない。これは仕様としてよいのか？
            errors.Count().Should().Be(0);
        }

        [Fact]
        public void SameCountVariationTest()
        {
            // Document
            Document document = Document.Builder(
                RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence("肖像権が正解で、昭三権は実在しない用語でありゆらぎ表現です。", 1))
                    .Build(); // TokenizeをBuild時に実行する。

            // Validation
            List<ValidationError> errors = validator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            errors.Count().Should().Be(2);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            // MEMO: Validatorの挙動を、同ReadingのSurfaceが同数出現している場合は双方をゆらぎとしてエラーとするように変更したため、
            // このテストケースはエラーが2つ出力されるようになった。

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations \"肖像(名詞)\", \"昭三(名詞)\" at (L1,8)");

            manager.GetErrorMessage(
                errors[0],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"肖像(名詞)\" と \"昭三(名詞)\"（出現位置：(L1,8)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("en-US"))
                    .Should().Be("Found possible Japanese word variations \"昭三(名詞)\", \"肖像(名詞)\" at (L1,0)");

            manager.GetErrorMessage(
                errors[1],
                CultureInfo.GetCultureInfo("ja-JP"))
                    .Should().Be("\"昭三(名詞)\" と \"肖像(名詞)\"（出現位置：(L1,0)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。");
        }

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
            List<ValidationError> errors = validator.Validate(document);

            // TODO: 数をカウントしただけではテストしたことにならないので、エラーの内容をテストできるようにする。
            //errors.Count().Should().Be(3);

            // 7. エラーメッセージを生成する。
            var manager = ErrorMessageManager.GetInstance();

            // 目視確認。
            errors.ForEach(e =>
            {
                output.WriteLine(manager.GetErrorMessage(e, CultureInfo.GetCultureInfo("ja-JP")));
            });

            string.Join("", errors.Select(e => manager.GetErrorMessage(e, CultureInfo.GetCultureInfo("ja-JP")))).Should().Be(
                "\"京(名詞)\" は \"今日(名詞)\"（出現位置：(L1,0), (L8,8)）の揺らぎ表現と考えられます。" +
                "\"京(名詞)\" と \"きょう(名詞)\"（出現位置：(L7,0)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。" +
                "\"きょう(名詞)\" は \"今日(名詞)\"（出現位置：(L1,0), (L8,8)）の揺らぎ表現と考えられます。" +
                "\"きょう(名詞)\" と \"京(名詞)\"（出現位置：(L4,18)）が同じ回数使用されています。どちらかが揺らぎ表現と考えられます。" +
                "\"野(名詞)\" は \"の(助詞)\"（出現位置：(L2,9), (L4,19), (L9,1), (L9,12), (L9,17)）の揺らぎ表現と考えられます。" +
                "\"市(名詞)\" は \"し(動詞)\"（出現位置：(L1,9), (L2,24), (L7,13)）の揺らぎ表現と考えられます。" + "" +
                "\"出来(動詞)\" は \"でき(動詞)\"（出現位置：(L10,0), (L10,3)）の揺らぎ表現と考えられます。"
            );
        }
    }
}
