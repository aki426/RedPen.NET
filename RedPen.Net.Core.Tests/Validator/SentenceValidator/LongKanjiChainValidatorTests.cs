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
using System.Text.RegularExpressions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class LongKanjiChainValidatorTests
    {
        private ITestOutputHelper output;

        public LongKanjiChainValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "圧倒的な当事者意識を一身に浴びている。", 4, "", 1, "当事者意識")]
        [InlineData("002", "圧倒的な当事者意識を一身に浴びている。", 6, "", 0, "")]
        [InlineData("003", "圧倒的な当事者意識を一身に浴びている。", 4, "当事者意識", 0, "")]
        [InlineData("004", "圧倒的当事者意識を一身に浴びることによって加速された物体は亜光速で運動する。その挙動は特殊相対性理論に従う。",
            4, "当事者意識", 2, "圧倒的当事者意識,特殊相対性理論")]
        [InlineData("005", "圧倒的当事者意識を一身に浴びることによって加速された物体は亜光速で運動する。その挙動は特殊相対性理論に従う。",
            4, "圧倒的当事者意識,特殊相対性理論", 0, "")]
        public void BasicTest(string nouse1, string text, int minLength, string ignoreCases, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new LongKanjiChainConfiguration(
                ValidationLevel.ERROR,
                minLength,
                ignoreCases == "" ? new HashSet<string>() : ignoreCases.Split(',').ToHashSet());

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new LongKanjiChainValidator(
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

        [Fact]
        public void GenerateRegexTest()
        {
            //string shard = "[\\u4e00-\\u9faf]{%d,}";
            //string shard = "{%d,}";
            // 次のFormatメソッドはエラーになる。
            //output.WriteLine(string.Format(shard, (3 + 1)));

            output.WriteLine($"[\\u4e00-\\u9faf]{{{3 + 1},}}");
        }
    }
}
