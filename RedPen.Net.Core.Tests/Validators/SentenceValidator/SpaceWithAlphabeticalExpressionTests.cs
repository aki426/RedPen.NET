using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.SentenceValidator
{
    public class SpaceWithAlphabeticalExpressionTests
    {
        private ITestOutputHelper output;

        public SpaceWithAlphabeticalExpressionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "きょうはCoke を飲みたい。", 1, "C")]
        [InlineData("002", "きょうは Cokeを飲みたい。", 1, "を")]
        [InlineData("003", "きょうはCokeを飲みたい。", 2, "C,を")]
        [InlineData("004", "This Coke is cold", 0, "")]
        [InlineData("005", "きょうは,コーラを飲みたい。", 0, "")]
        [InlineData("006", "きょうは（Coke）を飲みたい。", 0, "")]
        [InlineData("007", "きょうは、Coke を飲みたい。", 0, "")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new SpaceWithAlphabeticalExpressionConfiguration(ValidationLevel.ERROR, false, "", "");

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SpaceWithAlphabeticalExpressionValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Theory]
        // 1単語のみ
        [InlineData("001", "きょうはCokeを飲みたい。", 0, "")]
        [InlineData("002", "きょうは Coke を飲みたい。", 1, "Coke")]
        [InlineData("003", "きょうはCoke を飲みたい。", 1, "Coke")]
        [InlineData("004", "きょうは Cokeを飲みたい。", 1, "Coke")]
        // スペース区切りの2単語
        [InlineData("005", "きょうはPepsi colaを飲みたい。", 0, "")]
        [InlineData("006", "きょうは Pepsi cola を飲みたい。", 1, "Pepsi cola")]
        [InlineData("007", "きょうはPepsi cola を飲みたい。", 1, "Pepsi cola")]
        [InlineData("008", "きょうは Pepsi colaを飲みたい。", 1, "Pepsi cola")]
        // 日本語文中英語文
        [InlineData("009", "私は「This Coke is cold.」と言った。", 0, "")]
        [InlineData("010", "私は「 This Coke is cold. 」と言った。", 1, "This Coke is cold.")]
        [InlineData("011", "私は「 This Coke is cold.」と言った。", 1, "This Coke is cold.")]
        [InlineData("012", "私は「This Coke is cold. 」と言った。", 1, "This Coke is cold.")]
        // 英単語のみ。TODO: 想定では日本語文で1センテンス内に英単語1つというケースは想定しえないが、Validatorとしては次のとおり。
        [InlineData("013", "Coke", 0, "")]
        [InlineData("014", " Coke ", 1, "Coke")]
        [InlineData("015", " Coke", 1, "Coke")]
        [InlineData("016", "Coke ", 1, "Coke")]
        // 英文のみ。TODO: 想定では日本語文で1センテンス内に英文1つというケースは想定しえないが、Validatorとしては次のとおり。
        [InlineData("017", "This Coke is cold.", 0, "")]
        [InlineData("018", " This Coke is cold.", 1, "This Coke is cold.")]
        [InlineData("019", "This Coke is cold. ", 1, "This Coke is cold.")]
        [InlineData("020", " This Coke is cold. ", 1, "This Coke is cold.")]
        // 記号
        [InlineData("021", "きょうは,Cokeを飲みたい。", 0, "")]
        [InlineData("022", "きょうは,Coke を飲みたい。", 1, ",Coke")]
        // 半角記号は英語表現の一部とみなされる。
        [InlineData("023", "きょうは, Coke を飲みたい。", 1, ", Coke")]
        [InlineData("024", "きょうは(Coke)を飲みたい。", 0, "")]
        [InlineData("025", "きょうは(Coke )を飲みたい。", 0, "")] // TODO: ()に隣接するスペースは許容されるべきか？
        [InlineData("026", "きょうは、Cokeを飲みたい。", 0, "")]
        [InlineData("027", "きょうは、Coke を飲みたい。", 1, "Coke")]
        // 複数
        [InlineData("028", "きょうは、Coke と Pepsi を飲みたい。", 2, "Coke,Pepsi")]
        public void NoSpaceTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new SpaceWithAlphabeticalExpressionConfiguration(
                ValidationLevel.ERROR,
                Forbidden: true
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SpaceWithAlphabeticalExpressionValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Theory]
        [InlineData("000", "きょうはCokeを飲みたい。", 2, "C,を")]
        // 「の後と」の前はスペース判定をスキップして良いというテストケース設定にしてある。
        [InlineData("001", "きょうは「Coke」を飲みたい。", 0, "")]
        [InlineData("002", "きょうは「 Coke」を飲みたい。", 0, "")]
        [InlineData("003", "きょうは「Coke 」を飲みたい。", 0, "")]
        [InlineData("004", "きょうは「 Coke 」を飲みたい。", 0, "")]
        // ・中黒はSkipAfterすなわち後ろのスペース判定をスキップして良いというテストケース設定にしてある。
        [InlineData("005", "きょうは「Coke・Pepsi」を飲みたい。", 1, "・")] // Cokeと・の間はスペース判定スキップしないのでエラー。
        [InlineData("006", "きょうは「Coke ・Pepsi」を飲みたい。", 0, "")]
        [InlineData("007", "きょうは「Coke・ Pepsi」を飲みたい。", 1, "・")] // Cokeと・の間はスペース判定スキップしないのでエラー。
        [InlineData("008", "きょうは「Coke ・ Pepsi」を飲みたい。", 0, "")]
        public void SkipSymbolTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new SpaceWithAlphabeticalExpressionConfiguration(
                ValidationLevel.ERROR,
                Forbidden: false,
                SkipBefore: "」",
                SkipAfter: "「・"
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SpaceWithAlphabeticalExpressionValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }
    }
}
