using System;
using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Tokenizer
{
    public class WhiteSpaceTokenizerTests
    {
        private ITestOutputHelper output;

        public WhiteSpaceTokenizerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "this is a\u00A0pen", "this|is|a|pen")]
        [InlineData("002", "distributed(cluster) systems are good", "distributed|(|cluster|)|systems|are|good")]
        [InlineData("003", "I am an engineer.", "I|am|an|engineer|.")]
        [InlineData("004", "I'm an engineer", "I'm|an|engineer")]
        [InlineData("005", "Amount is $123,456,789.45", "Amount|is|$123|,|456|,|789|.|45")]
        [InlineData("006", "123", "123")]
        public void BasicTest(string nouse1, string text, string expected)
        {
            IRedPenTokenizer tokenizer = new WhiteSpaceTokenizer();
            List<TokenElement> results = tokenizer.Tokenize(new Sentence(text, 1));

            foreach (var item in results)
            {
                output.WriteLine(item.ToString());
            }

            string.Join("|", results.ConvertAll(x => x.Surface)).Should().Be(expected);
        }

        //
        //[InlineData("006", )]
    }
}
