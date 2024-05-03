using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class DoubledConjunctiveParticleGaValidatorTests
    {
        private ITestOutputHelper output;

        public DoubledConjunctiveParticleGaValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "今日は早朝から出発したが、定刻通りではなかったが、無事会場に到着した。", 1, "が")]
        [InlineData("002", "今日は早朝から出発したことで、定刻通りではなかったが、無事会場に到着した。", 0, "")]
        [InlineData("003", "この関数がエラーになるのは、関数名が正しくないためです。", 0, "")]
        [InlineData("004", "この文章が問題となっているのが不可解です。", 0, "")]
        [InlineData("005", "今日は早朝から出発したが、定刻には間に合わなかった。が、無事会場に到着した。", 0, "")]
        [InlineData("007", "今日は早朝から出発したが、定刻には間に合わなかった。定刻には間に合わなかったが、無事会場に到着した。", 0, "")]
        [InlineData("008", "「今日は早朝から出発したが、定刻には間に合わなかった。定刻には間に合わなかったが、無事会場に到着した。」", 0, "")]
        [InlineData("009", "今日は早朝から出発しましたが、定刻には間に合いませんでしたが、無事会場に到着しました。", 1, "が")]
        [InlineData("010", "規模は小さいが、収益は多いが、実益は小さい。", 1, "が")]
        [InlineData("011", "キーワードが多く抽出されたが、クラスタの数が10ということもあるが、逆に欠点となるようなキーワードは表示されなかった。", 1, "が")]
        [InlineData("012", "キーワードが多く抽出されたが、逆に欠点となるようなキーワードが表示されなかった。", 0, "")]
        [InlineData("014", "こんにちは。\n今日は早朝から出発したが、定刻には間に合わなかったが、無事会場に到着した。", 1, "が")]
        [InlineData("015", "\n今日は早朝から出発したが、定刻には間に合わなかったが、無事会場に到着した。", 1, "が")]
        [InlineData("015", "今日は早朝から出発したが、みちすがらが一番大変で、それでも無事会場に到着した。", 0, "")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledConjunctiveParticleGaConfiguration(
                ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledConjunctiveParticleGaValidator(
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
