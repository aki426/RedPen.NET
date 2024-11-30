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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    public class KatakanaEndHyphenTests
    {
        private readonly ITestOutputHelper output;

        private CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

        private KatakanaEndHyphenConfiguration validatorConfiguration;

        private KatakanaEndHyphenValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseExpressionVariationValidatorTest"/> class.
        /// MEMO: デフォルト設定でvalidatorをセットアップする。
        /// </summary>
        /// <param name="output">The output.</param>
        public KatakanaEndHyphenTests(ITestOutputHelper output)
        {
            this.output = output;

            // DefaultResourceの読み込み。
            HashSet<string> wordSet = new HashSet<string>();
            //string v = DefaultResources.ResourceManager.GetString($"KatakanaSpellcheck");
            string v = DefaultResources.KatakanaSpellcheck;
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                wordSet.Add(line);
            }

            // ValidatorConfigurationの生成。
            validatorConfiguration =
                new KatakanaEndHyphenConfiguration(ValidationLevel.ERROR, wordSet);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validatorの生成。
            validator = new KatakanaEndHyphenValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);
        }

        /// <summary>
        /// 入力文字列に対して、何個のエラーを検出するか、またエラー箇所の文字列は何かを検証するテスト。
        /// </summary>
        /// <param name="nouse1">The nouse1.</param>
        /// <param name="text">The text.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [InlineData("001", "", 0, "")]
        [InlineData("002", "あ", 0, "")]
        [InlineData("003", "ア", 0, "")]
        [InlineData("004", "ドア", 0, "")]
        [InlineData("005", "ポー", 0, "")]
        [InlineData("006", "ミラー", 0, "")]
        [InlineData("007", "コーヒー", 1, "コーヒー")]
        [InlineData("008", "コンピューターが壊れた。", 1, "コンピューター")]
        [InlineData("009", "コンピュータが壊れた。", 0, "")]
        [InlineData("010", "僕のコンピューターが壊れた。", 1, "コンピューター")]
        [InlineData("011", "僕のコンピュータが壊れた。", 0, "")]
        [InlineData("012", "僕のコンピューター", 1, "コンピューター")]
        [InlineData("013", "僕のコンピュータ", 0, "")]
        [InlineData("014", "コーヒー・コンピューター", 2, "コーヒー,コンピューター")]
        // KatakanaSpellcheck.txt内にリスナーが含まれているのでエラーにならない。
        [InlineData("015", "ラジオ番組はリスナーがいてこそ盛り上がります。", 0, "")]
        [InlineData("016", "メニューは必要ですか", 0, "")]
        public void BasicTest(string nouse1, string text, int errorCount, string expected)

        {
            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence(text, 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(errorCount);

            if (errors.Any())
            {
                var manager = new ErrorMessageManager();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }

        /// <summary>
        /// AllowWordを追加した場合のValidatorの動作検証。
        /// </summary>
        /// <param name="nouse1">The nouse1.</param>
        /// <param name="text">The text.</param>
        /// <param name="allowWord">The allow word.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        [InlineData("001", "濃いコーヒーは胃にわるい。", "コーヒー", 0, "")]
        public void AddAllowWordTest(string nouse1, string text, string allowWord, int errorCount, string expected)

        {
            // TODO: WordSetが単純なHashSetであるため、ValidatorとValidatorConfigurationの不変性が崩れている。
            // ImmutableHahsSetへ修正し、設定を変更する場合はValidatorとValidatorConfigurationを再生成するようにする。
            validator.Config.ExpressionSet.Add(allowWord);

            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence(text, 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(errorCount);

            if (errors.Any())
            {
                var manager = new ErrorMessageManager();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }
    }
}
