using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using redpen_core.tokenizer;
using Xunit;

namespace redpen_core.tokenizer.Tests
{
    public class WhiteSpaceTokenizerTests
    {
        [Fact()]
        public void TokenizeTest()
        {
            IRedPenTokenizer tokenizer = new WhiteSpaceTokenizer();
            List<TokenElement> results = tokenizer.Tokenize("this is a\u00A0pen");

            results.Count.Should().Be(4);

            results[0].Surface.Should().Be("this");
            results[0].Tags.Count.Should().Be(0);

            results[1].Surface.Should().Be("is");
            results[1].Tags.Count.Should().Be(0);

            results[2].Surface.Should().Be("a");
            results[2].Tags.Count.Should().Be(0);

            results[3].Surface.Should().Be("pen");
            results[3].Tags.Count.Should().Be(0);
        }

        [Fact]
        public void TokenizeSentenceWithNoSpaceBracketTest()
        {
            IRedPenTokenizer tokenizer = new WhiteSpaceTokenizer();
            List<TokenElement> results = tokenizer.Tokenize("distributed(cluster) systems are good");

            results.Count.Should().Be(7);

            results[0].Surface.Should().Be("distributed");
            results[0].Tags.Count.Should().Be(0);

            results[1].Surface.Should().Be("(");
            results[1].Tags.Count.Should().Be(0);

            results[2].Surface.Should().Be("cluster");
            results[2].Tags.Count.Should().Be(0);

            results[3].Surface.Should().Be(")");
            results[3].Tags.Count.Should().Be(0);

            results[4].Surface.Should().Be("systems");
            results[4].Tags.Count.Should().Be(0);

            results[5].Surface.Should().Be("are");
            results[5].Tags.Count.Should().Be(0);

            results[6].Surface.Should().Be("good");
            results[6].Tags.Count.Should().Be(0);
        }

        [Fact]
        public void TokenizeSentenceEndsWithPeriodTest()
        {
            IRedPenTokenizer tokenizer = new WhiteSpaceTokenizer();
            List<TokenElement> results = tokenizer.Tokenize("I am an engineer.");

            results.Count.Should().Be(5);

            results[0].Surface.Should().Be("I");
            results[0].Tags.Count.Should().Be(0);

            results[1].Surface.Should().Be("am");
            results[1].Tags.Count.Should().Be(0);

            results[2].Surface.Should().Be("an");
            results[2].Tags.Count.Should().Be(0);

            results[3].Surface.Should().Be("engineer");
            results[3].Tags.Count.Should().Be(0);

            results[4].Surface.Should().Be(".");
            results[4].Tags.Count.Should().Be(0);
        }

        [Fact]
        public void TokenizeSentenceWithContractionTest()
        {
            IRedPenTokenizer tokenizer = new WhiteSpaceTokenizer();
            List<TokenElement> results = tokenizer.Tokenize("I'm an engineer");

            results.Count.Should().Be(3);
            results[0].Surface.Should().Be("I'm");
            results[0].Tags.Count.Should().Be(0);
            results[1].Surface.Should().Be("an");
            results[1].Tags.Count.Should().Be(0);
            results[2].Surface.Should().Be("engineer");
            results[2].Tags.Count.Should().Be(0);
        }
    }
}
