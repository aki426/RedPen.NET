using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class SuccessiveWordValidatorTests
    {
        private ITestOutputHelper output;

        public SuccessiveWordValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "the item is is a good.", 1, "is")]
        [InlineData("002", "Welcome welcome to Estonia.", 1, "welcome")]
        [InlineData("003", "the item is a item good.", 0, "")]
        [InlineData("004", "Amount is $123,456,789.45", 0, "")]
        public void BasicEnglishTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // ValidatorConfiguration
            var validatorConfiguration = new SuccessiveWordConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SuccessiveWordValidator(
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
        [InlineData("001", "私はは嬉しい。", 1, "は")]
        [InlineData("002", "総出でで畑仕事をする。", 1, "で")]
        [InlineData("003", "彼のの色鉛筆と私の筆箱。", 1, "の")]
        [InlineData("004", "彼の色鉛筆と私の筆箱。", 0, "")]
        [InlineData("005", "トレイントレイン走っていく、トレイントレインどこまでも。", 2, "トレイン,トレイン")]
        [InlineData("006", "全部で1,000,000円です。", 0, "")]
        public void BasicJapaneseTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new SuccessiveWordConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SuccessiveWordValidator(
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
