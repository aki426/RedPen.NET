﻿using System.Collections.Generic;
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
            TokenElement actual = new TokenElement("word", new List<string> { "tag" }, 0, "reading");
            actual.Surface.Should().Be("word");
            actual.Tags.Should().BeEquivalentTo(new List<string> { "tag" });
            actual.Offset.Should().Be(0);
            actual.Reading.Should().Be("reading");
        }

        [Fact(DisplayName = "TokenElementのImmutable性テスト")]
        public void TokenElementImmutableTest()
        {
            TokenElement actual = new TokenElement("word", new List<string> { "tag" }, 0, "reading");

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
            TokenElement actual = new TokenElement("surface of word", new List<string> { "tag", "list" }, 42, "reading");
            output.WriteLine(actual.ToString());
        }
    }
}
