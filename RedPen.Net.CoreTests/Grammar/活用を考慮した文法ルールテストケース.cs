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

using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.CoreTests.Grammar
{
    public class 活用を考慮した文法ルールテストケース
    {
        private ITestOutputHelper output;

        public 活用を考慮した文法ルールテストケース(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "笑わない",
            "笑わ+ない", 1, "笑わ|ない")]
        [InlineData("002", "笑わない",
            "::動詞 + :ナイ:助動詞:基本形:特殊・ナイ", 1, "笑わ|ない")]
        [InlineData("002", "笑わない",
            "笑わ:ワラワ:動詞:未然形:五段・ワ行促音便:笑う + ない:ナイ:助動詞:基本形:特殊・ナイ", 1, "笑わ|ない")]
        public void 活用型や活用形を持つRuleの実装ケース(string nouse1, string text, string rule, int matchCount, string expected)
        {
            GrammarRuleTestUtility.TestMatchExtend(rule, text, matchCount, expected, output);
        }

        [Theory]
        [InlineData("001", "::動詞 + ::助動詞:基本形:特殊・ナイ", "防がない", 1, "防が|ない")]
        [InlineData("002", "::動詞 + ::助動詞:基本形:特殊・ナイ", "履かない", 1, "履か|ない")]
        [InlineData("003", "::動詞 + ::助動詞:基本形:特殊・ナイ", "行かない", 1, "行か|ない")]
        [InlineData("004", "::動詞 + ::助動詞:基本形:特殊・ナイ", "過ぎ行かない", 1, "過ぎ行か|ない")]
        [InlineData("005", "::動詞 + ::助動詞:基本形:特殊・ナイ", "冷やさない", 1, "冷やさ|ない")]
        [InlineData("006", "::動詞 + ::助動詞:基本形:特殊・ナイ", "打たない", 1, "打た|ない")]
        [InlineData("007", "::動詞 + ::助動詞:基本形:特殊・ナイ", "死なない", 1, "死な|ない")]
        [InlineData("008", "::動詞 + ::助動詞:基本形:特殊・ナイ", "呼ばない", 1, "呼ば|ない")]
        [InlineData("009", "::動詞 + ::助動詞:基本形:特殊・ナイ", "取り込まない", 1, "取り込ま|ない")]
        [InlineData("010", "::動詞 + ::助動詞:基本形:特殊・ナイ", "見送らない", 1, "見送ら|ない")]
        [InlineData("011", "::動詞 + ::助動詞:基本形:特殊・ナイ", "下さらない", 1, "下さら|ない")]
        [InlineData("012", "::動詞 + ::助動詞:基本形:特殊・ナイ", "言わない", 1, "言わ|ない")]
        [InlineData("013", "::動詞 + ::助動詞:基本形:特殊・ナイ", "笑わない", 1, "笑わ|ない")]
        public void 打ち消し用法(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRuleTestUtility.TestMatchExtend(rule, text, matchCount, expected, output);
        }

        [Theory]
        [InlineData("001", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "防がなかった", 1, "防が|なかっ|た")]
        [InlineData("002", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "履かなかった", 1, "履か|なかっ|た")]
        [InlineData("003", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "行かなかった", 1, "行か|なかっ|た")]
        [InlineData("004", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "過ぎ行かなかった", 1, "過ぎ行か|なかっ|た")]
        [InlineData("005", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "冷やさなかった", 1, "冷やさ|なかっ|た")]
        [InlineData("006", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "打たなかった", 1, "打た|なかっ|た")]
        [InlineData("007", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "死ななかった", 1, "死な|なかっ|た")]
        [InlineData("008", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "呼ばなかった", 1, "呼ば|なかっ|た")]
        [InlineData("009", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "取り込まなかった", 1, "取り込ま|なかっ|た")]
        [InlineData("010", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "見送らなかった", 1, "見送ら|なかっ|た")]
        [InlineData("011", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "下さらなかった", 1, "下さら|なかっ|た")]
        [InlineData("012", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "言わなかった", 1, "言わ|なかっ|た")]
        [InlineData("013", "::動詞:未然形 + :ナカッ:助動詞:連用タ接続:特殊・ナイ + :タ:助動詞:基本形:特殊・タ",
            "笑わなかった", 1, "笑わ|なかっ|た")]
        public void 打ち消し過去用法(string nouse1, string rule, string text, int matchCount, string expected)
        {
            GrammarRuleTestUtility.TestMatchExtend(rule, text, matchCount, expected, output);
        }
    }
}
