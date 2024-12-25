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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
        }

        /// <summary>
        /// 活用変化を目視確認するためのデータセットテストケース。
        /// </summary>
        [Fact]
        public void WatchTokenizedString()
        {
            string text = @"
笑わない
笑おう
笑う
笑いだ
笑いである
笑うとき
笑えば
笑え
笑えよ
笑われる
笑える
笑わせる
笑わさせる
笑わぬ
笑わん
笑おう
笑うまい
笑いたい
笑いたがる
笑いそうだ
笑うようだ
笑うらしい
笑っている
笑わなかった
笑った
笑いだった
笑いであった
笑ったとき
笑われた
笑えた
笑わせた
笑わさせた
笑いたかった
笑いたがった
笑いそうだった
笑うようだった
笑うらしかった
笑っていた
笑いません
笑いましょう
笑います
笑いです
笑いです
ｘ
ｘ
笑いなさい
笑いなさいよ
笑われます
笑えます
笑わせます
笑わさせます
笑いませぬ
笑いません
笑いましょう
笑いますまい
笑いたいです
笑いたがります
笑いそうです
笑うようです
笑うらしいです
笑っています
笑いませんでした
笑いました
笑いでした
笑いでした
ｘ
笑われました
笑えました
笑わせました
笑わさせました
笑いたかったです
笑いたがりました
笑いそうでした
笑うようでした
笑うらしかったです
笑っていました
";

            var sentences = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            ).Select((item, index) => new Sentence(item, index));
            List<List<TokenElement>> listlist = KuromojiController.Tokenize(sentences);

            foreach (var tokens in listlist)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Join("", tokens.Select(i => i.Surface)));
                sb.Append('\t');

                List<string> tokenString = new List<string>();
                foreach (var token in tokens)
                {
                    tokenString.Add($"{token.Surface}({token.Reading})/{token.BaseForm}[{token.PartOfSpeech[0]}]{token.InflectionType}/{token.InflectionForm}");
                }

                sb.Append(string.Join(" - ", tokenString));
                sb.Append('\t');

                sb.Append(string.Join(" - ", tokens.Select(i => i.Surface)));
                sb.Append('\t');

                sb.Append(string.Join(" - ", tokens.Select(i => i.Surface).Reverse()));

                output.WriteLine(sb.ToString());
            }
        }
    }
}
