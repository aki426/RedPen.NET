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
        /// 動詞の活用形Resolveのテスト。
        /// </summary>
        [Fact()]
        public void ResolveVerbTest()
        {
            var tokenElements = KuromojiController.Tokenize(new Sentence("笑うらしい。", 1));
            output.WriteLine("★Token...");
            foreach (var item in tokenElements)
            {
                output.WriteLine(item.ToString());
            }
            output.WriteLine("");

            // TokenElement { S= "笑う",	R= "ワラウ(ワラウ)",	P= [動詞-自立],	I= 基本形/五段・ワ行促音便,	B= ,	O= (L1,0)-(L1,1)}
            var t = tokenElements[0];
            InflectionResolver.Resolve(t, "基本形").surface.Should().Be("笑う");
            InflectionResolver.Resolve(t, "未然形").surface.Should().Be("笑わ");
            InflectionResolver.Resolve(t, "未然ウ接続").surface.Should().Be("笑お");
            InflectionResolver.Resolve(t, "連用形").surface.Should().Be("笑い");
            InflectionResolver.Resolve(t, "連用タ接続").surface.Should().Be("笑っ");
            InflectionResolver.Resolve(t, "仮定形").surface.Should().Be("笑え");
            InflectionResolver.Resolve(t, "命令ｅ").surface.Should().Be("笑え");

            (bool success, string surface) result = InflectionResolver.Resolve(t, "存在しない活用形");
            result.success.Should().BeFalse();
        }

        /// <summary>
        /// 形容詞の活用形Resolveのテスト。
        /// </summary>
        [Fact()]
        public void ResolveAdjTest()
        {
            var tokenElements = KuromojiController.Tokenize(new Sentence("四角い。", 1));
            output.WriteLine("★Token...");
            foreach (var item in tokenElements)
            {
                output.WriteLine(item.ToString());
            }
            output.WriteLine("");

            var t = tokenElements[0];
            InflectionResolver.Resolve(t, "基本形").surface.Should().Be("四角い");
            InflectionResolver.Resolve(t, "未然ヌ接続").surface.Should().Be("四角から");
            InflectionResolver.Resolve(t, "未然ウ接続").surface.Should().Be("四角かろ");
            InflectionResolver.Resolve(t, "連用タ接続").surface.Should().Be("四角かっ");
            InflectionResolver.Resolve(t, "連用テ接続").surface.Should().Be("四角くっ");
            InflectionResolver.Resolve(t, "連用ゴザイ接続").surface.Should().Be("四角ぅ");
            InflectionResolver.Resolve(t, "体言接続").surface.Should().Be("四角き");
            InflectionResolver.Resolve(t, "仮定形").surface.Should().Be("四角けれ");
            InflectionResolver.Resolve(t, "仮定縮約１").surface.Should().Be("四角けりゃ");
            InflectionResolver.Resolve(t, "仮定縮約２").surface.Should().Be("四角きゃ");
            InflectionResolver.Resolve(t, "命令ｅ").surface.Should().Be("四角かれ");
            InflectionResolver.Resolve(t, "ガル接続").surface.Should().Be("四角");
            InflectionResolver.Resolve(t, "文語基本形").surface.Should().Be("四角し");

            (bool success, string surface) result = InflectionResolver.Resolve(t, "存在しない活用形");
            result.success.Should().BeFalse();

            tokenElements = KuromojiController.Tokenize(new Sentence("美しかった。", 1));
            output.WriteLine("★Token...");
            foreach (var item in tokenElements)
            {
                output.WriteLine(item.ToString());
            }
            output.WriteLine("");

            t = tokenElements[0];
            InflectionResolver.Resolve(t, "基本形").surface.Should().Be("美しい");
            InflectionResolver.Resolve(t, "未然ヌ接続").surface.Should().Be("美しから");
            InflectionResolver.Resolve(t, "未然ウ接続").surface.Should().Be("美しかろ");
            InflectionResolver.Resolve(t, "連用タ接続").surface.Should().Be("美しかっ");
            InflectionResolver.Resolve(t, "連用テ接続").surface.Should().Be("美しくっ");
            InflectionResolver.Resolve(t, "連用ゴザイ接続").surface.Should().Be("美しゅぅ");
            InflectionResolver.Resolve(t, "体言接続").surface.Should().Be("美しき");
            InflectionResolver.Resolve(t, "仮定形").surface.Should().Be("美しけれ");
            InflectionResolver.Resolve(t, "仮定縮約１").surface.Should().Be("美しけりゃ");
            InflectionResolver.Resolve(t, "仮定縮約２").surface.Should().Be("美しきゃ");
            InflectionResolver.Resolve(t, "命令ｅ").surface.Should().Be("美しかれ");
            InflectionResolver.Resolve(t, "ガル接続").surface.Should().Be("美し");
            InflectionResolver.Resolve(t, "文語基本形").surface.Should().Be("美し");
        }
    }
}
