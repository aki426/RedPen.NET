using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Utility
{
    public class RuleExtractorTests
    {
        private ITestOutputHelper output;

        public RuleExtractorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Splits the test.
        /// </summary>
        [Fact]
        public void SplitTest()
        {
            string[] segments = RuleExtractor.Split("This:n + is:v");
            segments.Length.Should().Be(2);
            segments[0].Should().Be("This:n");
            segments[1].Should().Be("is:v");
        }

        /// <summary>
        /// Splits the without spaces test.
        /// </summary>
        [Fact]
        public void SplitWithoutSpacesTest()

        {
            string[] segments = RuleExtractor.Split("This:n+is:v");
            segments.Length.Should().Be(2);
            segments[0].Should().Be("This:n");
            segments[1].Should().Be("is:v");
        }

        /// <summary>
        /// Splits the without before spaces test.
        /// </summary>
        [Fact]
        public void SplitWithoutBeforeSpacesTest()
        {
            string[] segments = RuleExtractor.Split("This:n +is:v");
            segments.Length.Should().Be(2);
            segments[0].Should().Be("This:n");
            segments[1].Should().Be("is:v");
        }

        /// <summary>
        /// ルール表現文字列から抽出したルールのマッチングテスト。
        /// </summary>
        [Fact]
        public void RunTest()
        {
            ExpressionRule expressionRule = RuleExtractor.Run("This:n + is:v");
            expressionRule.ToSurface().Should().Be("Thisis");

            foreach (TokenElement token in expressionRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            List<TokenElement> tokens =
                "He said , This is a pen .".Split(' ')
                    .Select(s => new TokenElement(s, new List<string> { "tag" }, 1, 0, "reading")).ToList();

            foreach (TokenElement token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            expressionRule.MatchSurface(tokens).Should().BeTrue();
        }
    }
}
