using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.DocumentValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.DocumentValidator
{
    public class InvalidParenthesisValidatorTests
    {
        private ITestOutputHelper output;

        public InvalidParenthesisValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 3, 3, 0, "")]
        // MaxLenght, MaxNumber, MaxLevelを超える場合
        [InlineData("002", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 10, 3, 3, 1, "12")]
        [InlineData("003", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 2, 3, 1, "2")]
        [InlineData("004", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 3, 2, 1, "2")]
        // 対応する括弧が無い場合
        [InlineData("005", "ピーターラビット（野兎(父はパイになった））はビアトリクス・ポッター（英国人）による創作です。", 20, 3, 3, 1, "）")]
        [InlineData("006", "ピーターラビット（野兎）父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 3, 3, 1, ")")]
        [InlineData("007", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人による創作です。", 20, 3, 3, 1, "（")]
        // 括弧内に句読点がある場合
        // NOTE: Sentence単位でのValidationだと左右括弧の対応関係は括弧中の文の区切りで切れてしまうため正しく検出できない。
        // 対応方針として2つあり、1つはParagraph単位で左右の括弧の対応関係を取る方法。
        // もう1つは（）や「」に対応して文を区切るSentenceExtractorを実装する方法。
        // 今回はParagraph単位で左右の括弧の対応関係を取る方法を採用する。
        [InlineData("008", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人）による創作です。", 20, 3, 3, 0, "")]
        [InlineData("009", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人）による創作です", 20, 3, 3, 0, "")]
        [InlineData("010", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人）による創作です。有名な童話ですよ。", 20, 3, 3, 0, "")]
        [InlineData("011", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人()です。）による創作です。", 9, 2, 2, 3, "15,2,2")]
        [InlineData("012", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人()です。）による創作です", 9, 2, 2, 3, "15,2,2")]
        [InlineData("013", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人()です。）による創作です。有名な童話ですよ。", 9, 2, 2, 3, "15,2,2")]
        // 複数パラグラフ。
        [InlineData("013", "ピーターラビット（野兎です。父はパイになりました）はビアトリクス・ポッター（英国人()です。）による創作です。有名な童話ですよ。\n\n兎を擬人化（人とみなすこと）する文化は世界共通(ex.鳥獣戯画)ですね。",
            9, 2, 2, 4, "15,2,2,2")]
        public void BasicTest(string nouse1, string text, int minLength, int minCount, int minLevel, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new InvalidParenthesisConfiguration(ValidationLevel.ERROR, minLength, minCount, minLevel);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new InvalidParenthesisValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonDocumentWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }
    }
}
