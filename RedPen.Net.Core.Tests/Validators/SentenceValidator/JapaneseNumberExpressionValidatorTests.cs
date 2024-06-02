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
    public class JapaneseNumberExpressionValidatorTests
    {
        private ITestOutputHelper output;

        public JapaneseNumberExpressionValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // 半角数字の数字表現のみを許容するモードです。
        [InlineData("001", "半角数字表現の1つの計数表現です。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("002", "全角数字表現の１つの計数表現です。", NumberStyle.HankakuOnly, 1, "１つ")]
        [InlineData("003", "漢数字表現の一つの計数表現です。", NumberStyle.HankakuOnly, 1, "一つ")]
        [InlineData("004", "ひらなが数字表現のひとつの計数表現です。", NumberStyle.HankakuOnly, 1, "ひとつ")]
        // 全角数字の数字表現のみを許容するモードです。
        [InlineData("005", "半角数字表現の1つの計数表現です。", NumberStyle.ZenkakuOnly, 1, "1つ")]
        [InlineData("006", "全角数字表現の１つの計数表現です。", NumberStyle.ZenkakuOnly, 0, "")]
        [InlineData("007", "漢数字表現の一つの計数表現です。", NumberStyle.ZenkakuOnly, 1, "一つ")]
        [InlineData("008", "ひらなが数字表現のひとつの計数表現です。", NumberStyle.ZenkakuOnly, 1, "ひとつ")]
        // 漢数字の数字表現のみを許容するモードです。
        [InlineData("009", "半角数字表現の1つの計数表現です。", NumberStyle.KansujiOnly, 1, "1つ")]
        [InlineData("010", "全角数字表現の１つの計数表現です。", NumberStyle.KansujiOnly, 1, "１つ")]
        [InlineData("011", "漢数字表現の一つの計数表現です。", NumberStyle.KansujiOnly, 0, "")]
        [InlineData("012", "ひらなが数字表現のひとつの計数表現です。", NumberStyle.KansujiOnly, 1, "ひとつ")]
        // ひらがなの数字表現のみを許容するモードです。
        [InlineData("013", "半角数字表現の1つの計数表現です。", NumberStyle.HiraganaOnly, 1, "1つ")]
        [InlineData("014", "全角数字表現の１つの計数表現です。", NumberStyle.HiraganaOnly, 1, "１つ")]
        [InlineData("015", "漢数字表現の一つの計数表現です。", NumberStyle.HiraganaOnly, 1, "一つ")]
        [InlineData("016", "ひらなが数字表現のひとつの計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        // 半角数字の数字表現のみを許容するモードです。
        [InlineData("017", "半角数字表現の1つにあたる計数表現です。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("018", "全角数字表現の１つにあたる計数表現です。", NumberStyle.HankakuOnly, 1, "１つ")]
        [InlineData("019", "漢数字表現の一つにあたる計数表現です。", NumberStyle.HankakuOnly, 1, "一つ")]
        [InlineData("020", "ひらなが数字表現のひとつにあたる計数表現です。", NumberStyle.HankakuOnly, 1, "ひとつ")]
        // 全角数字の数字表現のみを許容するモードです。
        [InlineData("021", "半角数字表現の1つにあたる計数表現です。", NumberStyle.ZenkakuOnly, 1, "1つ")]
        [InlineData("022", "全角数字表現の１つにあたる計数表現です。", NumberStyle.ZenkakuOnly, 0, "")]
        [InlineData("023", "漢数字表現の一つにあたる計数表現です。", NumberStyle.ZenkakuOnly, 1, "一つ")]
        [InlineData("024", "ひらなが数字表現のひとつにあたる計数表現です。", NumberStyle.ZenkakuOnly, 1, "ひとつ")]
        // 漢数字の数字表現のみを許容するモードです。
        [InlineData("025", "半角数字表現の1つにあたる計数表現です。", NumberStyle.KansujiOnly, 1, "1つ")]
        [InlineData("026", "全角数字表現の１つにあたる計数表現です。", NumberStyle.KansujiOnly, 1, "１つ")]
        [InlineData("027", "漢数字表現の一つにあたる計数表現です。", NumberStyle.KansujiOnly, 0, "")]
        [InlineData("028", "ひらなが数字表現のひとつにあたる計数表現です。", NumberStyle.KansujiOnly, 1, "ひとつ")]
        // ひらがなの数字表現のみを許容するモードです。
        [InlineData("029", "半角数字表現の1つにあたる計数表現です。", NumberStyle.HiraganaOnly, 1, "1つ")]
        [InlineData("030", "全角数字表現の１つにあたる計数表現です。", NumberStyle.HiraganaOnly, 1, "１つ")]
        [InlineData("031", "漢数字表現の一つにあたる計数表現です。", NumberStyle.HiraganaOnly, 1, "一つ")]
        [InlineData("032", "ひらなが数字表現のひとつにあたる計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        // 半角数字の数字表現のみを許容するモードです。
        [InlineData("033", "半角数字表現の1、計数表現です。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("034", "全角数字表現の１、計数表現です。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("035", "漢数字表現の一、計数表現です。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("036", "ひらなが数字表現のひとつ、計数表現です。", NumberStyle.HankakuOnly, 1, "ひとつ")]
        // 全角数字の数字表現のみを許容するモードです。
        [InlineData("037", "半角数字表現の1、計数表現です。", NumberStyle.ZenkakuOnly, 0, "")]
        [InlineData("038", "全角数字表現の１、計数表現です。", NumberStyle.ZenkakuOnly, 0, "")]
        [InlineData("039", "漢数字表現の一、計数表現です。", NumberStyle.ZenkakuOnly, 0, "")]
        [InlineData("040", "ひらなが数字表現のひとつ、計数表現です。", NumberStyle.ZenkakuOnly, 1, "ひとつ")]
        // 漢数字の数字表現のみを許容するモードです。
        [InlineData("041", "半角数字表現の1、計数表現です。", NumberStyle.KansujiOnly, 0, "")]
        [InlineData("042", "全角数字表現の１、計数表現です。", NumberStyle.KansujiOnly, 0, "")]
        [InlineData("043", "漢数字表現の一、計数表現です。", NumberStyle.KansujiOnly, 0, "")]
        [InlineData("044", "ひらなが数字表現のひとつ、計数表現です。", NumberStyle.KansujiOnly, 1, "ひとつ")]
        // ひらがなの数字表現のみを許容するモードです。
        [InlineData("045", "半角数字表現の1、計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        [InlineData("046", "全角数字表現の１、計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        [InlineData("047", "漢数字表現の一、計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        [InlineData("048", "ひらなが数字表現のひとつ、計数表現です。", NumberStyle.HiraganaOnly, 0, "")]
        public void BasicTest(string nouse1, string text, NumberStyle numberStyle, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new JapaneseNumberExpressionConfiguration(ValidationLevel.ERROR, numberStyle);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new JapaneseNumberExpressionValidator(
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

        /// <summary>
        /// 複合的な計数表現に対するテストケース。
        /// </summary>
        /// <param name="nouse1">The nouse1.</param>
        /// <param name="text">The text.</param>
        /// <param name="numberStyle">The number style.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        [Theory]
        // 「～個」は許容される。
        [InlineData("001", "約５～６個の表現。", NumberStyle.HankakuOnly, 0, "")]
        [InlineData("002", "約５～６つの表現。", NumberStyle.HankakuOnly, 1, "６つ")]
        [InlineData("003", "約５～６の表現。", NumberStyle.HankakuOnly, 1, "６の")]
        // 「数」のみは許容される。
        [InlineData("004", "約５～６表現。", NumberStyle.HankakuOnly, 0, "")]
        // 正規表現によるバリデーションの限界。
        [InlineData("005", "９のつの指輪。", NumberStyle.HankakuOnly, 1, "９の")]
        public void ComplexNumberExpressionTest(string nouse1, string text, NumberStyle numberStyle, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new JapaneseNumberExpressionConfiguration(ValidationLevel.ERROR, numberStyle);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new JapaneseNumberExpressionValidator(
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
