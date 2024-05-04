using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class DoubleNegativeValidatorTests
    {
        private ITestOutputHelper output;

        public DoubleNegativeValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // no error
        [InlineData("000", "そういう話は理解できない。", 0, "")]
        [InlineData("001", "そういう話は理解できる。", 0, "")]

        // ず::ズ + に::ニ + い::イ + られ::ラレ + ない::ナイ
        // ず::ズ + に::ニ + い::イ + られ::ラレ + ませ::マセ + ん::ン
        // ず::ズ + に::ニ + *::* + い::イ + られ::ラレ + ない::ナイ
        // ず::ズ + に::ニ + *::* + い::イ + られ::ラレ + ませ::マセ + ん::ン
        [InlineData("002", "そう言わずに居られない。", 1, "ずに居られない")]
        [InlineData("003", "そう言わずに居られませんでした。", 1, "ずに居られません")]
        [InlineData("004", "そう言わずには居られない。", 1, "ずには居られない")]
        [InlineData("005", "そう言わずにも居られない。", 1, "ずにも居られない")]
        [InlineData("006", "そう言わずには居られませんでした。", 1, "ずには居られません")]

        // 無い::ナイ + *::* + 無い::ナイ
        [InlineData("007", "そんなこと無いじゃない。", 1, "無いじゃない")]
        [InlineData("008", "ないことないでしょう。", 1, "ないことない")]

        // 無い::ナイ + と::ト + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + *::* + 切れ::キレ + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + は::ハ + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + も::モ + *::* + 無い::ナイ
        [InlineData("009", "ないと言えない。", 1, "ないと言えない")]
        [InlineData("010", "無いと言えないです。", 1, "無いと言えない")]
        [InlineData("011", "無いと限らないです。", 1, "無いと限らない")]
        [InlineData("012", "ないとはいえない。", 1, "ないとはいえない")]
        [InlineData("013", "無いとも言えないな。", 1, "無いとも言えない")]
        [InlineData("014", "無いとは限らないよ。", 1, "無いとは限らない")]
        [InlineData("015", "無いとも限らないでしょ。", 1, "無いとも限らない")]
        [InlineData("016", "無いと断定出来ない。", 1, "無いと断定出来ない")]
        [InlineData("017", "無いと断定は出来ない。", 1, "無いと断定は出来ない")]
        [InlineData("018", "無いとは断定出来ない。", 1, "無いとは断定出来ない")]
        [InlineData("019", "無いと断定することも出来ない。", 1, "無いと断定することも出来ない")]

        // 無い::ナイ + 事::コト + *::* + 無い::ナイ
        [InlineData("020", "無いことも無いでしょう。", 1, "無いことも無い")]
        [InlineData("021", "ないことはない。", 1, "ないことはない")]
        [InlineData("022", "ないこともない。", 1, "ないこともない")]

        // 無い::ナイ + 訳::ワケ + で::デ + *::* + 無い::ナイ
        // 無い::ナイ + 訳::ワケ + で::デ + *::* + あり::アリ + ませ::マセ + ん::ン
        [InlineData("023", "この問題は決して解けないわけでは無い。", 1, "ないわけでは無い")]
        [InlineData("024", "この問題は決して解けないわけではありません。", 1, "ないわけではありません")]

        // 無く::ナク + は::ハ + 無い::ナイ
        // 無く::ナク + 無い::ナイ
        [InlineData("025", "無くは無いでしょう。", 1, "無くは無い")]
        [InlineData("026", "そうでも無くない？", 1, "無くない")]
        public void JapaneseBasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new DoubleNegativeConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubleNegativeValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }
    }
}
