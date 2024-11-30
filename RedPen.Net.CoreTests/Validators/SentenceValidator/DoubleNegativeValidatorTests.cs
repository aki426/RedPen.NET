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
using RedPen.Net.Core.Validators.Tests;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Validators.SentenceValidator.Tests
{
    public class DoubleNegativeValidatorTests
    {
        private ITestOutputHelper output;

        public DoubleNegativeValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        // no error
        [InlineData("000", "そういう話は理解できない。", 0, "")]
        [InlineData("001", "そういう話は理解できる。", 0, "")]

        // ず::ズ + に::ニ + い::イ + られ::ラレ + ない::ナイ
        // ず::ズ + に::ニ + い::イ + られ::ラレ + ませ::マセ + ん::ン
        // ず::ズ + に::ニ + *::* + い::イ + られ::ラレ + ない::ナイ
        // ず::ズ + に::ニ + *::* + い::イ + られ::ラレ + ませ::マセ + ん::ン
        [InlineData("002", "そう言わずに居られない。", 1, "ずに居られない")]
        [InlineData("003", "そう言わずに居られませんでした。", 1, "ずに居られません")]
        [InlineData("004", "そう言わずには居られない。", 1, "ずには居られない")]
        [InlineData("005", "そう言わずにも居られない。", 1, "ずにも居られない")]
        [InlineData("006", "そう言わずには居られませんでした。", 1, "ずには居られません")]

        // 無い::ナイ + *::* + 無い::ナイ
        [InlineData("007", "そんなこと無いじゃない。", 1, "無いじゃない")]
        [InlineData("008", "ないことないでしょう。", 1, "ないことない")]

        // 無い::ナイ + と::ト + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + *::* + 切れ::キレ + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + は::ハ + *::* + 無い::ナイ
        // 無い::ナイ + と::ト + *::* + も::モ + *::* + 無い::ナイ
        [InlineData("009", "ないと言えない。", 1, "ないと言えない")]
        [InlineData("010", "無いと言えないです。", 1, "無いと言えない")]
        [InlineData("011", "無いと限らないです。", 1, "無いと限らない")]
        [InlineData("012", "ないとはいえない。", 1, "ないとはいえない")]
        [InlineData("013", "無いとも言えないな。", 1, "無いとも言えない")]
        [InlineData("014", "無いとは限らないよ。", 1, "無いとは限らない")]
        [InlineData("015", "無いとも限らないでしょ。", 1, "無いとも限らない")]
        [InlineData("016", "無いと断定出来ない。", 1, "無いと断定出来ない")]
        [InlineData("017", "無いと断定は出来ない。", 1, "無いと断定は出来ない")]
        [InlineData("018", "無いと断定も出来ない。", 1, "無いと断定も出来ない")]
        [InlineData("019", "無いとは断定出来ない。", 1, "無いとは断定出来ない")]
        [InlineData("020", "無いと断定することも出来ない。", 1, "無いと断定することも出来ない")]
        [InlineData("021", "無いと断定することは出来ない。", 1, "無いと断定することは出来ない")]
        [InlineData("022", "無いと断定は出来ない。", 1, "無いと断定は出来ない")]
        [InlineData("023", "無いと言うことも出来ない。", 1, "無いと言うことも出来ない")]
        [InlineData("024", "無いと言い切れない。", 1, "無いと言い切れない")]
        [InlineData("025", "無いとも言い切れない。", 1, "無いとも言い切れない")]
        [InlineData("026", "無いと言うことは出来ない。", 1, "無いと言うことは出来ない")]

        // 無い::ナイ + 事::コト + *::* + 無い::ナイ
        [InlineData("027", "無いことも無いでしょう。", 1, "無いことも無い")]
        [InlineData("028", "ないことはない。", 1, "ないことはない")]
        [InlineData("029", "ないこともない。", 1, "ないこともない")]

        // 無い::ナイ + 訳::ワケ + で::デ + *::* + 無い::ナイ
        // 無い::ナイ + 訳::ワケ + で::デ + *::* + あり::アリ + ませ::マセ + ん::ン
        [InlineData("030", "この問題は決して解けないわけでは無い。", 1, "ないわけでは無い")]
        [InlineData("031", "この問題は決して解けないわけではありません。", 1, "ないわけではありません")]

        // 無く::ナク + は::ハ + 無い::ナイ
        // 無く::ナク + 無い::ナイ
        [InlineData("032", "無くは無いでしょう。", 1, "無くは無い")]
        [InlineData("033", "無くもない気がする。", 1, "無くもない")]
        [InlineData("034", "そうでも無くない？", 1, "無くない")]

        // 一般的な二重否定表現をリストアップ
        [InlineData("035", "反対しないとも限らない。", 1, "ないとも限らない")]
        [InlineData("036", "やれないこともないでしょう。", 1, "ないこともない")]
        [InlineData("037", "飲まずにいられない。", 1, "ずにいられない")]
        [InlineData("038", "私は、笑わずにはいられない", 1, "ずにはいられない")]
        [InlineData("039", "私は、笑わずにはいられなかった", 0, "")]
        [InlineData("040", "子供の幸せを願わない人はいない。", 1, "ない人はいない")]
        [InlineData("041", "明けない夜はない。", 1, "ない夜はない")]
        [InlineData("042", "キャンセルにならないとも限らない", 1, "ないとも限らない")]
        [InlineData("043", "伸びない人は、いない。", 1, "ない人はいない")] // 結果には「、」半角全角スペースは除去される。
        [InlineData("044", "もう恋なんてしないなんて言わないよ絶対", 1, "ないなんて言わない")]
        [InlineData("045", "彼のやり方は間違っていないとは言い切れない", 1, "ないとは言い切れない")]
        [InlineData("046", "彼の言動は誤解されないとは言い切れない", 1, "ないとは言い切れない")]
        [InlineData("047", "料理ができないわけじゃない", 1, "ないわけじゃない")]

        // 紛らわしいが問題無いケース
        [InlineData("048", "内定は無いです。", 0, "")]
        [InlineData("049", "彼がいなければ、このプロジェクトは成功していなかった。", 0, "")]
        [InlineData("050", "成長なくして成功なし", 0, "")]
        [InlineData("051", "涙なくして語れない", 0, "")]
        [InlineData("052", "彼のことは嫌いなわけじゃない。", 0, "")]
        public void JapaneseBasicTest(string nouse1, string text, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // ValidatorConfiguration
            var validatorConfiguration = new DoubleNegativeConfiguration(ValidationLevel.ERROR);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubleNegativeValidator(
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
