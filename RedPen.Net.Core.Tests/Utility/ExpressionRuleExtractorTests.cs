using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;
using RedPen.Net.Core.Validators;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Utility
{
    public class ExpressionRuleExtractorTests
    {
        private ITestOutputHelper output;

        public ExpressionRuleExtractorTests(ITestOutputHelper output)
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
            string[] segments = ExpressionRuleExtractor.SplitToRuleElements(text);
            segments.Length.Should().Be(count);

            string.Join("|", segments).Should().Be(expected);
        }

        /// <summary>ルール表現文字列からルールを抽出するテスト。</summary>
        [Fact]
        public void RunTest()
        {
            ExpressionRule expressionRule = ExpressionRuleExtractor.Run("This: n,noun + is: v");
            // MEMO: ルール文字列はマッチングのためすべて小文字に変換される。
            expressionRule.ToSurface().Should().Be("thisis");

            foreach (TokenElement token in expressionRule.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            expressionRule.Tokens[0].Surface.Should().Be("this");
            expressionRule.Tokens[0].Tags.Count.Should().Be(2);
            expressionRule.Tokens[0].Tags[0].Should().Be("n");
            expressionRule.Tokens[0].Tags[1].Should().Be("noun");
            expressionRule.Tokens[1].Surface.Should().Be("is");
            expressionRule.Tokens[1].Tags.Count.Should().Be(1);
            expressionRule.Tokens[1].Tags[0].Should().Be("v");
        }
    }
}
