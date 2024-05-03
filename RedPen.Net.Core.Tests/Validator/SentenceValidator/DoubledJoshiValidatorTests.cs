using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class DoubledJoshiValidatorTests
    {
        private ITestOutputHelper output;

        public DoubledJoshiValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "私は彼は好き。", 1, "", 0, "")]
        [InlineData("002", "私は彼は好き。", 2, "", 1, "は")]
        [InlineData("003", "私は彼が好き。", 2, "", 0, "")]
        [InlineData("004", "私は彼は好き。", 2, "は", 0, "")]
        [InlineData("005", "彼の色鉛筆と私の筆箱。", 1, "", 0, "")]
        [InlineData("006", "彼の色鉛筆と私の筆箱。", 2, "", 0, "")]
        [InlineData("007", "彼の色鉛筆と私の筆箱。", 3, "", 0, "")]
        [InlineData("008", "彼の色鉛筆と私の筆箱。", 4, "", 1, "の")]
        [InlineData("009", "二人でで一人の仮面ライダーだ。", 1, "", 1, "で")]
        public void BasicTest(string nouse1, string text, int minInterval, string skipJoshi, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledJoshiConfiguration(
                ValidationLevel.ERROR,
                minInterval,
                skipJoshi.Split(',').ToHashSet());

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledJoshiValidator(
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
