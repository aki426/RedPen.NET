using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Lucene.Net.Util;
using RedPen.Net.Core.Model;
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
            actual.ToString().Should().Be("TokenElement { Surface = \"surface of word\", Reading = \"reading\", Tags = [ \"tag\", \"list\" ], OffsetMap = (L1,42)-(L1,43)-(L1,44)-(L1,45)-(L1,46)-(L1,47)-(L1,48)-(L1,49)-(L1,50)-(L1,51)-(L1,52)-(L1,53)-(L1,54)-(L1,55)-(L1,56)}");
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

        [Theory]
        [InlineData("001", "弊社の経営方針の説明を受けた。", "弊社|の|経営方針|の|説明|を|受け|た|。")]
        [InlineData("002", "長い言葉恐怖症", "長い|言葉恐怖症")]
        public void ConcatNounsTest(string nouse1, string text, string expected)
        {
            var jaTokenizer = RedPenTokenizerFactory.CreateTokenizer(CultureInfo.GetCultureInfo("ja-JP"));
            List<TokenElement> tokens = jaTokenizer.Tokenize(new Sentence(text, 1));

            output.WriteLine("★Original:");
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            var concatedNouns = TokenElement.ConvertToConcatedNouns(tokens);

            output.WriteLine("★Original:");
            foreach (var token in concatedNouns)
            {
                output.WriteLine(token.ToString());
            }

            string.Join("|", concatedNouns.Select(t => t.Surface)).Should().Be(expected);
        }
    }
}
