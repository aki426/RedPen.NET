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
    public class マニュアル用テストケース
    {
        private ITestOutputHelper output;

        public マニュアル用テストケース(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "猫", 1, "猫")]
        [InlineData("002", "猫:ネコ:*-一般", 1, "猫")]
        [InlineData("003", ":ネコ", 1, "猫")]
        [InlineData("004", "猫:イヌ", 0, "")]
        [InlineData("005", "吾輩 + は", 1, "吾輩|は")]
        [InlineData("006", "*::名詞 + ::助詞", 2, "吾輩|は|犬|で")]
        [InlineData("007", ":ワガハイ + は = :アル", 1, "吾輩|は|猫|だ|が|犬|で|も|ある")]
        [InlineData("007", "::名詞 = ::名詞 = ::名詞", 1, "吾輩|は|猫|だ|が|犬")]
        public void 文法ルールのパターンマッチ具体例(string nouse1, string rule, int matchCount, string expected)
        {
            GrammarRuleTestUtility.TestMatchExtend(rule, "吾輩は猫だが犬でもある。", matchCount, expected, output);
        }
    }
}
