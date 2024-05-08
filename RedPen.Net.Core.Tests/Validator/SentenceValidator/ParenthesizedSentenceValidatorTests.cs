using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit.Abstractions;
using Xunit;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class ParenthesizedSentenceValidatorTests
    {
        private ITestOutputHelper output;

        public ParenthesizedSentenceValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 2, 2, 0, "")]
        // MaxLenght, MaxNumber, MaxLevelを超える場合
        [InlineData("002", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 10, 2, 2, 1, "12")]
        [InlineData("003", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 1, 2, 1, "2")]
        [InlineData("004", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 2, 1, 1, "2")]
        // 対応する括弧が無い場合
        [InlineData("005", "ピーターラビット（野兎(父はパイになった））はビアトリクス・ポッター（英国人）による創作です。", 20, 2, 2, 1, "）")]
        [InlineData("006", "ピーターラビット（野兎）父はパイになった)）はビアトリクス・ポッター（英国人）による創作です。", 20, 2, 2, 1, ")")]
        [InlineData("007", "ピーターラビット（野兎(父はパイになった)）はビアトリクス・ポッター（英国人による創作です。", 20, 2, 2, 1, "（")]
        public void BasicTest(string nouse1, string text, int maxLength, int maxNumber, int maxLevel, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new ParenthesizedSentenceConfiguration(ValidationLevel.ERROR, maxLength, maxNumber, maxLevel);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new ParenthesizedSentenceValidator(
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
