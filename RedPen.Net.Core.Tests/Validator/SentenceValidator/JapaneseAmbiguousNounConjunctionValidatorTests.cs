using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class JapaneseAmbiguousNounConjunctionValidatorTests
    {
        private ITestOutputHelper output;

        public JapaneseAmbiguousNounConjunctionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // no error
        [InlineData("001", "弊社の経営方針の説明を受けた。。", "", 1, "弊社の経営方針の説明")]
        [InlineData("002", "弊社の経営方針についての説明を受けた。", "", 0, "")]
        [InlineData("003", "不思議の国のアリスは面白い。", "不思議の国のアリス", 0, "")]
        [InlineData("004", "東京都の山手線の沿線のビルの屋根の看板を飛び越えていく。", "", 4,
            "東京都の山手線の沿線,山手線の沿線のビル,沿線のビルの屋根,ビルの屋根の看板")]
        public void JapaneseBasicTest(string nouse1, string text, string ignoreCases, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new JapaneseAmbiguousNounConjunctionConfiguration(
                ValidationLevel.ERROR,
                new HashSet<string>(ignoreCases.Split(','))
            );

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new JapaneseAmbiguousNounConjunctionValidator(
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
