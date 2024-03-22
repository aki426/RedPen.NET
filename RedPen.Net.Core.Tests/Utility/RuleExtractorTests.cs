using System;
using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validator;
using Xunit;

namespace RedPen.Net.Core.Tests.Utility
{
    public class RuleExtractorTests
    {
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

            // MEMO: TokenElementの仕様は無視してSurfaceのみで比較している。
            // TODO: TokenElementの実態に即した使い方に変更する。
            expressionRule.Match(new List<TokenElement>() {
                new TokenElement("He", new List<string> { "tag" }, 0, "reading"),
                new TokenElement("said", new List<string> { "tag" }, 0, "reading"),
                new TokenElement(",", new List<string> { "tag" }, 0, "reading"),
                new TokenElement("This", new List<string> { "tag" }, 0, "reading"),
                new TokenElement("is", new List<string> { "tag" }, 0, "reading"),
                new TokenElement("a", new List<string> { "tag" }, 0, "reading"),
                new TokenElement("pen", new List<string> { "tag" }, 0, "reading"),
            }).Should().BeTrue();
        }
    }
}
