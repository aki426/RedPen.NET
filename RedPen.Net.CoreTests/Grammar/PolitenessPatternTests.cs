//   Copyright (c) 2025 KANEDA Akihiro <taoist.aki@gmail.com>
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

using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Grammar.Tests
{
    public class PolitenessPatternTests
    {
        private readonly ITestOutputHelper output;

        public PolitenessPatternTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// PolitenessPatternのコンストラクタテスト
        /// </summary>
        [Fact()]
        public void PolitenessPatternTest()
        {
            var politenessPattern = new PolitenessPattern(
                "打ち消し",
                "::動詞:未然形 + :ナイ",
                "ない",
                "::動詞:連用形 + :マセ + :ン",
                "ません"
            );

            // NOTE: 込み入った新規ロジックを検証したいわけではないので、一旦定義を追認するだけのテストで良い。
            politenessPattern.JotaiRule.ToString().Should().Be("::動詞:未然形 + :ナイ");
            politenessPattern.KeitaiRule.ToString().Should().Be("::動詞:連用形 + :マセ + :ン"); ;
        }
    }
}
