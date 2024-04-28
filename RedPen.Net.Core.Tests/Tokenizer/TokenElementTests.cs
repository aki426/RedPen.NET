using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Tokenizer
{
    public class TokenElementTests
    {
        private readonly ITestOutputHelper output;

        public TokenElementTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact(DisplayName = "TokenElementのコンストラクタ基本テスト")]
        public void TokenElementTest()
        {
            TokenElement actual = new TokenElement("word", new List<string> { "tag" }, 1, 0, "reading");
            actual.Surface.Should().Be("word");
            actual.Tags.Should().BeEquivalentTo(new List<string> { "tag" });
            actual.Offset.Should().Be(0);
            actual.Reading.Should().Be("reading");
        }

        [Fact(DisplayName = "TokenElementのImmutable性テスト")]
        public void TokenElementImmutableTest()
        {
            TokenElement actual = new TokenElement("word", new List<string> { "tag" }, 1, 0, "reading");

            // Build error.
            //actual.Invoking(i => i.Surface = "new Surface")
            //    .Should().Throw<Exception>()
            //    .WithMessage("TokenElement is immutable at Surface");
            //actual.Invoking(i => i.Offset = 42)
            //    .Should().Throw<Exception>()
            //    .WithMessage("TokenElement is immutable at Offset");
            //actual.Invoking(i => i.Reading = "new reading")
            //    .Should().Throw<Exception>()
            //    .WithMessage("TokenElement is immutable at Reading");

            // ReadOnlyCollectionなので、Add, Remove, Clearはできない。
            // Appendはできるが、元のListには影響しない。
            IEnumerable<string> enumerable = actual.Tags.Append("new Tag");

            enumerable.Count().Should().Be(2);
            enumerable.ElementAt(1).Should().Be("new Tag");

            // Tagsプロパティは都度tagsのコピーを返すので、元のtagsには影響しない。
            actual.Tags.Count.Should().Be(1);
            actual.Tags[0].Should().Be("tag");

            // actual.Tags[1].Should().Be("new Tag");

            //actual.Invoking(i => i.Tags.Append("new Tag"))
            //    .Should().Throw<Exception>()
            //    .WithMessage("TokenElement is immutable at Tags");
        }

        [Fact()]
        public void ToStringTest()
        {
            TokenElement actual = new TokenElement("surface of word", new List<string>() { "tag", "list" }, 1, 42, "reading");

            output.WriteLine(actual.ToString());
            actual.ToString().Should().Be("TokenElement { Surface = \"surface of word\", Reading = \"reading\", LineNumber = 1, Offset = 42 , Tags = [ \"tag\", \"list\" ]}");
        }

        [Theory]
        [InlineData("001", "a,b,c", "a,b,c", true)]
        [InlineData("002", "a,b,*", "a,b,c", true)]
        [InlineData("003", "a,b", "a,b,c", true)]
        [InlineData("004", "a,b,d", "a,b,c", false)]
        [InlineData("005", "a,d", "a,b,c", false)]
        [InlineData("006", "a", "a,b,c", true)]
        [InlineData("007", "", "a,b,c", true)] // taga1の方は空のTagsを生成することを期待する。
        [InlineData("008", "a,b,*,d", "a,b,c", true)]
        [InlineData("009", "a,b,*,d", "a,b,c,*", true)]
        public void MatchTagsTest(string nouse1, string tags1, string tags2, bool expected)
        {
            TokenElement token1 = new TokenElement("word1", tags1 == "" ? new List<string>() : tags1.Split(',').ToList(), 1, 0, "reading");
            TokenElement token2 = new TokenElement("word2", tags2 == "" ? new List<string>() : tags2.Split(',').ToList(), 1, 0, "reading");

            token1.MatchTags(token2).Should().Be(expected);
        }
    }
}
