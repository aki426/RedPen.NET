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

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Inflection.Tests
{
    public class InflectionResolverTests
    {
        private readonly ITestOutputHelper output;

        public InflectionResolverTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// 活用形Resolveのテスト。
        /// </summary>
        [Fact()]
        public void ResolveTest()
        {
            TokenElement token = new TokenElement(
                "笑わ",
                "ワラワ",
                "ワラワ",
                ImmutableList.Create("動詞", "自立"),
                "笑う",
                "五段・ワ行促音便",
                "未然形",
                ImmutableList.Create(new LineOffset(1, 0))
            );

            InflectionResolver.Resolve(token, "基本形").surface.Should().Be("笑う");
            InflectionResolver.Resolve(token, "未然形").surface.Should().Be("笑わ");
            InflectionResolver.Resolve(token, "未然ウ接続").surface.Should().Be("笑お");
            InflectionResolver.Resolve(token, "連用形").surface.Should().Be("笑い");
            InflectionResolver.Resolve(token, "連用タ接続").surface.Should().Be("笑っ");
            InflectionResolver.Resolve(token, "仮定形").surface.Should().Be("笑え");
            InflectionResolver.Resolve(token, "命令ｅ").surface.Should().Be("笑え");

            (bool success, string surface) result = InflectionResolver.Resolve(token, "存在しない活用形");
            result.success.Should().BeFalse();

            // TokenElement { S= "笑う",	R= "ワラウ(ワラウ)",	P= [動詞-自立],	I= 基本形/五段・ワ行促音便,	B= ,	O= (L1,0)-(L1,1)}
            var token2 = KuromojiController.Tokenize(new Sentence("笑うらしい", 1)).FirstOrDefault();
            InflectionResolver.Resolve(token2, "基本形").surface.Should().Be("笑う");
            InflectionResolver.Resolve(token2, "未然形").surface.Should().Be("笑わ");
            InflectionResolver.Resolve(token2, "未然ウ接続").surface.Should().Be("笑お");
            InflectionResolver.Resolve(token2, "連用形").surface.Should().Be("笑い");
            InflectionResolver.Resolve(token2, "連用タ接続").surface.Should().Be("笑っ");
            InflectionResolver.Resolve(token2, "仮定形").surface.Should().Be("笑え");
            InflectionResolver.Resolve(token2, "命令ｅ").surface.Should().Be("笑え");
        }
    }
}
