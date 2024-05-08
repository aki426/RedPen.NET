using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using FluentAssertions;
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
        [InlineData("010", "a,b,,d", "a,b,c,d", true)]
        [InlineData("011", "a,b,,d", "a,b,c", true)] // 空文字のタグは「*」として扱う
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

        [Fact]
        public void ParseKatakanaTokenTest()
        {
            var jaTokenizer = RedPenTokenizerFactory.CreateTokenizer(CultureInfo.GetCultureInfo("ja-JP"));
            List<TokenElement> tokens = jaTokenizer.Tokenize(new Sentence("ここに仮面ライダー参上。", 1));

            output.WriteLine("★Original:");
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            // 元々のTokenは固有名詞「仮面ライダー」で分割されている。
            string.Join("|", tokens.Select(t => t.Surface)).Should().Be("ここ|に|仮面ライダー|参上|。");

            // Tokenをカタカナ文字かそれ以外でさらに分割する。
            var katakanaParsedTokens = TokenElement.ParseKatakanaToken(tokens);

            output.WriteLine("★Parse Katakana:");
            foreach (var token in katakanaParsedTokens)
            {
                output.WriteLine(token.ToString());
            }
            output.WriteLine("");

            // 「仮面」と「ライダー」で分割されている。
            string.Join("|", katakanaParsedTokens.Select(t => t.Surface)).Should().Be("ここ|に|仮面|ライダー|参上|。");

            // OffsetMapは元のTokenのOffsetMapを引き継ぐ。
            tokens.SelectMany(i => i.OffsetMap).SequenceEqual(katakanaParsedTokens.SelectMany(i => i.OffsetMap)).Should().BeTrue();
            tokens[2].OffsetMap.SequenceEqual(new List<LineOffset> {
                katakanaParsedTokens[2].OffsetMap[0],
                katakanaParsedTokens[2].OffsetMap[1],
                katakanaParsedTokens[3].OffsetMap[0],
                katakanaParsedTokens[3].OffsetMap[1],
                katakanaParsedTokens[3].OffsetMap[2],
                katakanaParsedTokens[3].OffsetMap[3]
            }).Should().BeTrue();

            string.Join("-", katakanaParsedTokens[2].OffsetMap.Select(i => i.ConvertToShortText())).Should().Be("(L1,3)-(L1,4)");
            string.Join("-", katakanaParsedTokens[3].OffsetMap.Select(i => i.ConvertToShortText())).Should().Be("(L1,5)-(L1,6)-(L1,7)-(L1,8)");

            var complexToken = new TokenElement("解離性ミリオンアーサー第２シーズン", new List<string> { "tag" }.ToImmutableList(), "reading",
                ImmutableList.Create(
                    new LineOffset(1, 0), // 解
                    new LineOffset(1, 1),
                    new LineOffset(1, 2),
                    new LineOffset(1, 3), // ミ
                    new LineOffset(1, 4),
                    new LineOffset(1, 5),
                    new LineOffset(1, 6),
                    new LineOffset(1, 7), // ア
                    new LineOffset(1, 8),
                    new LineOffset(1, 9),
                    new LineOffset(1, 10),
                    new LineOffset(1, 11), // 第
                    new LineOffset(1, 12),
                    new LineOffset(1, 13), // シ
                    new LineOffset(1, 14),
                    new LineOffset(1, 15),
                    new LineOffset(1, 16)
                ));

            var complexTokenList = TokenElement.ParseKatakanaToken(new List<TokenElement>() { complexToken });

            output.WriteLine("★Parse Katakana:");
            foreach (var token in complexTokenList)
            {
                output.WriteLine(token.ToString());
            }

            complexTokenList.Count.Should().Be(4);
            string.Join("|", complexTokenList.Select(t => t.Surface)).Should().Be("解離性|ミリオンアーサー|第２|シーズン");
        }
    }
}
