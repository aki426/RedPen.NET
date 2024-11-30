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

using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Model;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tokenizer.Tests
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
