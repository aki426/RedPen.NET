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
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Utility
{
    public class GrammarRuleExtractorTests
    {
        private ITestOutputHelper output;

        public GrammarRuleExtractorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "This:n + is:v", 2, "This:n|is:v")]
        [InlineData("002", "This:n+is:v", 2, "This:n|is:v")]
        [InlineData("003", "This:n +is:v", 2, "This:n|is:v")]
        [InlineData("004", "This:n+ is:v", 2, "This:n|is:v")]
        [InlineData("005", "This:n +  is:v", 2, "This:n|is:v")]
        public void SplitTest(string nouse1, string text, int count, string expected)
        {
            string[] segments = GrammarRuleExtractor.SplitToRuleElements(text);
            segments.Length.Should().Be(count);

            string.Join("|", segments).Should().Be(expected);
        }

        /// <summary>ルール表現文字列からルールを抽出するテスト。</summary>
        [Fact]
        public void RunTest()
        {
            GrammarRule rule = GrammarRuleExtractor.Run("This: n,noun + is: v");
            // MEMO: ルール文字列はマッチングのためすべて小文字に変換される。
            rule.ToSurface().Should().Be("thisis");

            foreach (TokenElement token in rule.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            rule.Tokens[0].Surface.Should().Be("this");
            rule.Tokens[0].Tags.Count.Should().Be(2);
            rule.Tokens[0].Tags[0].Should().Be("n");
            rule.Tokens[0].Tags[1].Should().Be("noun");
            rule.Tokens[1].Surface.Should().Be("is");
            rule.Tokens[1].Tags.Count.Should().Be(1);
            rule.Tokens[1].Tags[0].Should().Be("v");
        }
    }
}
