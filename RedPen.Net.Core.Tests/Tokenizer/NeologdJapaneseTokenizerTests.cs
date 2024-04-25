using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Tokenizer
{
    public class NeologdJapaneseTokenizerTests
    {
        private ITestOutputHelper output;

        public NeologdJapaneseTokenizerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tokenizes the test.
        /// </summary>
        [Fact(DisplayName = "Kuromoji＋Neologdの基本的なTokenizeテスト")]
        public void TokenizeTest()
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();

            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence("今日も晴天だ。", 1));

            tokens.Count.Should().Be(5);

            tokens[0].Surface.Should().Be("今日");
            tokens[0].Tags[0].Should().Be("名詞");

            tokens[1].Surface.Should().Be("も");
            tokens[1].Tags[0].Should().Be("助詞");

            tokens[2].Surface.Should().Be("晴天");
            tokens[2].Tags[0].Should().Be("名詞");

            tokens[3].Surface.Should().Be("だ");
            tokens[3].Tags[0].Should().Be("助動詞");

            tokens[4].Surface.Should().Be("。");
            tokens[4].Tags[0].Should().Be("記号");
        }

        [Fact]
        public void TokenizeVoidTest()
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence("", 1));
            tokens.Count.Should().Be(0);
        }

        [Theory]
        // 断定
        [InlineData(1, 1, "吾輩は猫だ。", "だ")]
        [InlineData(1, 2, "吾輩は猫である。", "である")]
        [InlineData(1, 3, "吾輩は猫です。", "です")]
        [InlineData(1, 4, "吾輩は猫ですね。", "です")]
        [InlineData(1, 5, "吾輩は猫ですよ。", "です")]
        // 仮定
        [InlineData(1, 6, "名前はまだ無い。", "")]
        [InlineData(1, 6, "名前はまだ無いだろう。", "だろう")]
        [InlineData(1, 6, "名前はまだ無いはずだ。", "だ")]
        [InlineData(1, 6, "名前はまだ無いでしょう。", "でしょう")]
        [InlineData(1, 6, "名前はまだ無いはずです。", "です")]
        // が
        [InlineData(2, 1, "猫なのだが、子猫であった。", "な,だ,であった")]
        [InlineData(2, 2, "猫だが、子猫だった。", "だ,だった")]
        [InlineData(2, 3, "猫なのですが、子猫でした。", "な,です,でした")]
        [InlineData(3, 1, "子猫であるが、捨て猫だった。", "である,だった")]
        [InlineData(3, 2, "子猫ですが、捨て猫でした。", "です,でした")]
        // 過去形
        [InlineData(4, 1, "ごはんがあった。", "た")]
        [InlineData(4, 2, "ごはんがありました。", "ました")]
        // 過去形
        [InlineData(5, 1, "名前だ。", "だ")]
        [InlineData(5, 2, "名前だった。", "だった")]
        [InlineData(5, 3, "名前です。", "です")]
        [InlineData(5, 4, "名前でした。", "でした")]
        // 過去進行
        [InlineData(6, 1, "ニャーと鳴いていた。", "た")]
        [InlineData(6, 2, "ニャーと鳴いてた。", "た")]
        [InlineData(6, 3, "ニャーと鳴いてました。", "ました")]
        [InlineData(6, 4, "ニャーと鳴いていました。", "ました")]
        // 接続
        [InlineData(6, 5, "子猫だったが、捨て猫だったので、空腹なので、心配だが、食事だ。", "だった,だった,な,だ,だ")]
        [InlineData(6, 6, "子猫でしたが、捨て猫でしたので、空腹ですので、心配ですが、食事です。", "でした,でした,です,です,です")]
        // ～と
        [InlineData(7, 1, "人間だと見当をつける。", "だ")]
        [InlineData(7, 1, "人間だろうと見当をつける。", "だろう")]
        [InlineData(7, 2, "人間だと見当をつけます。", "だ,ます")]
        // 否定
        [InlineData(8, 1, "わからぬ。", "ぬ")]
        [InlineData(8, 2, "わからない。", "ない")]
        [InlineData(8, 3, "わからないはず。", "ない")]
        [InlineData(8, 4, "わからないはずだ。", "ない,だ")]
        [InlineData(8, 5, "わかりません。", "ません")]
        [InlineData(8, 6, "わからないです。", "ないです")]
        [InlineData(8, 7, "わからないでしょう。", "ないでしょう")]
        // 検出無し
        [InlineData(9, 1, "食事する。", "")]
        [InlineData(9, 1, "食事します。", "ます")]
        // 体言止め
        [InlineData(10, 1, "吾輩。", "")]
        public void GetJodoshiTest(int nouse1, int nouser2, string text, string jodoshi)
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();

            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            // 連続する助動詞を連結し、まとめて区切る。
            List<string> jodoshiChunk = new List<string>();
            string jodoshiBuffer = "";
            foreach (var token in tokens)
            {
                if (token.Tags[0] == "助動詞")
                {
                    jodoshiBuffer += token.Surface;
                }
                else
                {
                    if (jodoshiBuffer != "")
                    {
                        jodoshiChunk.Add(jodoshiBuffer);
                        jodoshiBuffer = "";
                    }
                }
            }
            if (jodoshiBuffer != "")
            {
                jodoshiChunk.Add(jodoshiBuffer);
            }

            output.WriteLine(string.Join(",", jodoshiChunk));

            string.Join(",", jodoshiChunk).Should().Be(jodoshi);
        }
    }
}
