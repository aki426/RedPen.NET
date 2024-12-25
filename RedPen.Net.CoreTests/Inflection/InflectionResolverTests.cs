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
        /// 打ち消し用法の動詞活用の検証コード
        /// NOTE: 出力を1つのテストケースでしたいためactualをリスト形式で。
        /// </summary>
        [Fact()]
        public void 動詞打ち消し用法のテスト()
        {
            List<(string type, string surface, string form)> actual = new List<(string type, string surface, string form)>()
            {
                 // 5_打ち消し.csv
                 // 現在時制
                 ("カ変・クル", "こない", "未然形"),
                 ("カ変・クル", "こなかった", "未然形"),
                 ("カ変・来ル", "来ない", "未然形"),
                 ("カ変・来ル", "来なかった", "未然形"),
                 ("サ変・スル", "しない", "未然形"),
                 ("サ変・スル", "しなかった", "未然形"),
                 // TokenElement { Surface = "察", Reading = "サッ(サッ)", PoS = [ 名詞-サ変接続 ], BaseForm = , Inflection = / OffsetMap = (L1,0)}
                 //("サ変・－スル", "察しない", "未然形"),
                 //("サ変・－スル", "察しなかった", "未然形"),
                 // TokenElement { Surface = "生じ", Reading = "ショウジ(ショージ)", PoS = [ 動詞-自立 ], BaseForm = 生じる, Inflection = 一段/未然形 OffsetMap = (L1,0)-(L1,1)}
                 //("サ変・－ズル", "生じない", "未然形"),
                 //("サ変・－ズル", "生じなかった", "未然形"),
                 ("一段", "受けない", "未然形"),
                 ("一段", "受けなかった", "未然形"),
                 ("一段・クレル", "呉れない", "未然形"),
                 ("一段・クレル", "呉れなかった", "未然形"),
                 // TokenElement { Surface = "助け", Reading = "タスケ(タスケ)", PoS = [ 動詞-自立 ], BaseForm = 助ける, Inflection = 一段/未然形 OffsetMap = (L1,0)-(L1,1)}
                 //("下二・カ行", "助けない", "未然形"),
                 //("下二・カ行", "助けなかった", "未然形"),
                 // TokenElement { Surface = "見上げ", Reading = "ミアゲ(ミアゲ)", PoS = [ 動詞-自立 ], BaseForm = 見上げる, Inflection = 一段/未然形 OffsetMap = (L1,0)-(L1,1)-(L1,2)}
                 //("下二・ガ行", "見上げない", "未然形"),
                 //("下二・ガ行", "見上げなかった", "未然形"),
                 ("下二・ダ行", "いでない", "未然形"),
                 ("下二・ダ行", "いでなかった", "未然形"),
                 ("下二・ハ行", "憂へない", "未然形"),
                 ("下二・ハ行", "憂へなかった", "未然形"),
                 // TokenElement { Surface = "改め", Reading = "アラタメ(アラタメ)", PoS = [ 動詞-自立 ], BaseForm = 改める, Inflection = 一段/未然形 OffsetMap = (L1,0)-(L1,1)}
                 //("下二・マ行", "改めない", "未然形"),
                 //("下二・マ行", "改めなかった", "未然形"),
                 // TokenElement { Surface = "得", Reading = "エ(エ)", PoS = [ 動詞-自立 ], BaseForm = 得る, Inflection = 一段/未然形 OffsetMap = (L1,0)}
                 //("下二・得", "得ない", "未然形"),
                 //("下二・得", "得なかった", "未然形"),
                 ("五段・ガ行", "防がない", "未然形"),
                 ("五段・ガ行", "防がなかった", "未然形"),
                 ("五段・カ行イ音便", "履かない", "未然形"),
                 ("五段・カ行イ音便", "履かなかった", "未然形"),
                 ("五段・カ行促音便", "行かない", "未然形"),
                 ("五段・カ行促音便", "行かなかった", "未然形"),
                 ("五段・カ行促音便ユク", "過ぎ行かない", "未然形"),
                 ("五段・カ行促音便ユク", "過ぎ行かなかった", "未然形"),
                 ("五段・サ行", "冷やさない", "未然形"),
                 ("五段・サ行", "冷やさなかった", "未然形"),
                 ("五段・タ行", "打たない", "未然形"),
                 ("五段・タ行", "打たなかった", "未然形"),
                 ("五段・ナ行", "死なない", "未然形"),
                 ("五段・ナ行", "死ななかった", "未然形"),
                 ("五段・バ行", "呼ばない", "未然形"),
                 ("五段・バ行", "呼ばなかった", "未然形"),
                 ("五段・マ行", "取り込まない", "未然形"),
                 ("五段・マ行", "取り込まなかった", "未然形"),
                 ("五段・ラ行", "見送らない", "未然形"),
                 ("五段・ラ行", "見送らなかった", "未然形"),
                 ("五段・ラ行特殊", "下さらない", "未然形"),
                 ("五段・ラ行特殊", "下さらなかった", "未然形"),
                 // TokenElement { Surface = "言わ", Reading = "イワ(イワ)", PoS = [ 動詞-自立 ], BaseForm = 言う, Inflection = 五段・ワ行促音便/未然形 OffsetMap = (L1,0)-(L1,1)}
                 //("五段・ワ行ウ音便", "言わない", "未然形"),
                 //("五段・ワ行ウ音便", "言わなかった", "未然形"),
                 ("五段・ワ行促音便", "笑わない", "未然形"),
                 ("五段・ワ行促音便", "笑わなかった", "未然形"),
                 ("四段・サ行", "天てらさない", "未然形"),
                 ("四段・サ行", "天てらさなかった", "未然形"),
                 ("四段・タ行", "群だたない", "未然形"),
                 ("四段・タ行", "群だたなかった", "未然形"),
                 ("四段・ハ行", "思はない", "未然形"),
                 ("四段・ハ行", "思はなかった", "未然形"),
                 ("四段・バ行", "たばない", "未然形"),
                 ("四段・バ行", "たばなかった", "未然形"),
                 ("上二・ダ行", "恥ぢない", "未然形"),
                 ("上二・ダ行", "恥ぢなかった", "未然形"),
                 ("上二・ハ行", "憂ひない", "未然形"),
                 ("上二・ハ行", "憂ひなかった", "未然形"),
                 // 検証用追加1→一段/未然形 OffsetMap = (L1,0)}
                 ("一段", "得ない", "未然形"),
                 ("一段", "得なかった", "未然形"),
                 // TokenElement { Surface = "ない", Reading = "ナイ(ナイ)", PoS = [ 形容詞-自立 ], BaseForm = , Inflection = 形容詞・アウオ段/基本形 OffsetMap = (L1,0)-(L1,1)}
                 //("検証用追加2", "ない", "未然形"),
                 // TokenElement { Surface = "なかっ", Reading = "ナカッ(ナカッ)", PoS = [ 形容詞-自立 ], BaseForm = ない, Inflection = 形容詞・アウオ段/連用タ接続 OffsetMap = (L1,0)-(L1,1)-(L1,2)}
                 //("検証用追加2", "なかった", "未然形"),

                 // 過去時制
                 ("カ変・クル", "きません", "連用形"),
                 ("カ変・クル", "きませんでした", "連用形"),
                 ("カ変・来ル", "来ません", "連用形"),
                 ("カ変・来ル", "来ませんでした", "連用形"),
                 ("サ変・スル", "しません", "連用形"),
                 ("サ変・スル", "しませんでした", "連用形"),
                 // TokenElement { Surface = "察し", Reading = "サッシ(サッシ)", PoS = [ 動詞-自立 ], BaseForm = 察す, Inflection = 五段・サ行/連用形 OffsetMap = (L1,0)-(L1,1)}
                 //("サ変・－スル", "察しません", "連用形"),
                 //("サ変・－スル", "察しませんでした", "連用形"),
                 // TokenElement { Surface = "生じ", Reading = "ショウジ(ショージ)", PoS = [ 動詞-自立 ], BaseForm = 生じる, Inflection = 一段/連用形 OffsetMap = (L1,0)-(L1,1)}
                 //("サ変・－ズル", "生じません", "連用形"),
                 //("サ変・－ズル", "生じませんでした", "連用形"),
                 ("一段", "受けません", "連用形"),
                 ("一段", "受けませんでした", "連用形"),
                 ("一段・クレル", "呉れません", "連用形"),
                 ("一段・クレル", "呉れませんでした", "連用形"),
                 // TokenElement { Surface = "助け", Reading = "タスケ(タスケ)", PoS = [ 動詞-自立 ], BaseForm = 助ける, Inflection = 一段/連用形 OffsetMap = (L1,0)-(L1,1)}
                 //("下二・カ行", "助けません", "連用形"),
                 //("下二・カ行", "助けませんでした", "連用形"),
                 // TokenElement { Surface = "見上げ", Reading = "ミアゲ(ミアゲ)", PoS = [ 動詞-自立 ], BaseForm = 見上げる, Inflection = 一段/連用形 OffsetMap = (L1,0)-(L1,1)-(L1,2)}
                 //("下二・ガ行", "見上げません", "連用形"),
                 //("下二・ガ行", "見上げませんでした", "連用形"),
                 ("下二・ダ行", "いでません", "連用形"),
                 ("下二・ダ行", "いでませんでした", "連用形"),
                 ("下二・ハ行", "憂へません", "連用形"),
                 ("下二・ハ行", "憂へませんでした", "連用形"),
                 // TokenElement { Surface = "改め", Reading = "アラタメ(アラタメ)", PoS = [ 動詞-自立 ], BaseForm = 改める, Inflection = 一段/連用形 OffsetMap = (L1,0)-(L1,1)}
                 //("下二・マ行", "改めません", "連用形"),
                 //("下二・マ行", "改めませんでした", "連用形"),
                 // TokenElement { Surface = "得", Reading = "エ(エ)", PoS = [ 動詞-自立 ], BaseForm = 得る, Inflection = 一段/連用形 OffsetMap = (L1,0)}
                 //("下二・得", "得ません", "連用形"),
                 //("下二・得", "得ませんでした", "連用形"),
                 ("五段・ガ行", "防ぎません", "連用形"),
                 ("五段・ガ行", "防ぎませんでした", "連用形"),
                 ("五段・カ行イ音便", "履きません", "連用形"),
                 ("五段・カ行イ音便", "履きませんでした", "連用形"),
                 ("五段・カ行促音便", "行きません", "連用形"),
                 ("五段・カ行促音便", "行きませんでした", "連用形"),
                 ("五段・カ行促音便ユク", "過ぎ行きません", "連用形"),
                 ("五段・カ行促音便ユク", "過ぎ行きませんでした", "連用形"),
                 ("五段・サ行", "冷やしません", "連用形"),
                 ("五段・サ行", "冷やしませんでした", "連用形"),
                 ("五段・タ行", "打ちません", "連用形"),
                 ("五段・タ行", "打ちませんでした", "連用形"),
                 ("五段・ナ行", "死にません", "連用形"),
                 ("五段・ナ行", "死にませんでした", "連用形"),
                 ("五段・バ行", "呼びません", "連用形"),
                 ("五段・バ行", "呼びませんでした", "連用形"),
                 ("五段・マ行", "取り込みません", "連用形"),
                 ("五段・マ行", "取り込みませんでした", "連用形"),
                 ("五段・ラ行", "見送りません", "連用形"),
                 ("五段・ラ行", "見送りませんでした", "連用形"),
                 ("五段・ラ行特殊", "下さいません", "連用形"),
                 ("五段・ラ行特殊", "下さいませんでした", "連用形"),
                 // TokenElement { Surface = "言い", Reading = "イイ(イイ)", PoS = [ 動詞-自立 ], BaseForm = 言う, Inflection = 五段・ワ行促音便/連用形 OffsetMap = (L1,0)-(L1,1)}
                 //("五段・ワ行ウ音便", "言いません", "連用形"),
                 //("五段・ワ行ウ音便", "言いませんでした", "連用形"),
                 ("五段・ワ行促音便", "笑いません", "連用形"),
                 ("五段・ワ行促音便", "笑いませんでした", "連用形"),
                 ("四段・サ行", "天てらしません", "連用形"),
                 ("四段・サ行", "天てらしませんでした", "連用形"),
                 ("四段・タ行", "群だちません", "連用形"),
                 ("四段・タ行", "群だちませんでした", "連用形"),
                 ("四段・ハ行", "思ひません", "連用形"),
                 ("四段・ハ行", "思ひませんでした", "連用形"),
                 ("四段・バ行", "たびません", "連用形"),
                 ("四段・バ行", "たびませんでした", "連用形"),
                 ("上二・ダ行", "恥ぢません", "連用形"),
                 ("上二・ダ行", "恥ぢませんでした", "連用形"),
                 ("上二・ハ行", "憂ひません", "連用形"),
                 ("上二・ハ行", "憂ひませんでした", "連用形"),
                 // 検証用追加1→一段
                 ("一段", "得ません", "連用形"),
                 ("一段", "得ませんでした", "連用形"),
                 // 検証用追加2→五段・ラ行
                 ("五段・ラ行", "ありません", "連用形"),
                 ("五段・ラ行", "ありませんでした", "連用形"),
            };

            foreach (var item in actual)
            {
                var tokenElements = KuromojiController.Tokenize(new Sentence(item.surface, 1));

                output.WriteLine($"{tokenElements[0].ToString()}");

                tokenElements[0].InflectionType.Should().Be(item.type);
                tokenElements[0].InflectionForm.Should().Be(item.form);
            }
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
