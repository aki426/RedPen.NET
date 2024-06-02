//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validators.SentenceValidator
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
