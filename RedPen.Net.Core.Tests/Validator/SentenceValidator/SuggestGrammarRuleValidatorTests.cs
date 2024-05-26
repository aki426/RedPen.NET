using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class SuggestGrammarRuleValidatorTests
    {
        private ITestOutputHelper output;

        public SuggestGrammarRuleValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "この文では常用漢字のみを使っています。", "この = は|此の～は", 1, "この文では")]
        [InlineData("002", "この文では常用漢字だけを使っています。", "だけ = ます|のみ～ます", 1, "だけを使っています")]
        public void BasicTest(string nouse1, string text, string rules, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            Dictionary<string, string> temp = new Dictionary<string, string>();
            var splited = rules.Split('|');
            for (int i = 0; i < splited.Length; i++)
            {
                temp.Add(splited[i].Trim(), splited[i + 1].Trim());
                i++;
            }

            // ValidatorConfiguration
            var validatorConfiguration = new SuggestGrammarRuleConfiguration(
                ValidationLevel.ERROR,
                temp);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new SuggestGrammarRuleValidator(
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
