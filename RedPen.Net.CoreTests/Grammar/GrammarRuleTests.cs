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

using FluentAssertions;
using RedPen.Net.Core.Model;
using Xunit;

namespace RedPen.Net.Core.Grammar.Tests
{
    public class GrammarRuleTests
    {
        [Fact()]
        public void ReversedPatternTest()
        {
            // 基本的な検証
            GrammarRuleExtractor.Run("a + b = c").ToString()
                .Should().Be("a + b = c");
            // 文字列で検証
            new GrammarRule() { Pattern = GrammarRuleExtractor.Run("a + b = c").ReversedPattern }.ToString()
                .Should().Be("c = b + a");

            // ReversedPatternの検証
            GrammarRuleExtractor.Run("a + b = c").ReversedPattern
                .Should().BeEquivalentTo(
                    GrammarRuleExtractor.Run("c = b + a").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().BeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ + ::動詞:未然形").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // NGパターン
            GrammarRuleExtractor.Run("a + b = c").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run("c + b + a").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // InflectionFormの有無を検知する。
            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ + ::動詞:").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());

            // 隣接／非隣接を区別する。
            GrammarRuleExtractor.Run("::動詞:未然形 + :ナイ").ReversedPattern
                .Should().NotBeEquivalentTo(
                    GrammarRuleExtractor.Run(":ナイ = ::動詞:未然形").Pattern,
                    options => options.WithStrictOrdering().ComparingByMembers<TokenElement>());
        }
    }
}
