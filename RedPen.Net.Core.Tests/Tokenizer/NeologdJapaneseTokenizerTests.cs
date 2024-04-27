﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        // 助動詞意外の「ます」
        [InlineData(11, 1, "それで済ますことはない。", "ない")]
        public void GetJodoshiTest(int nouse1, int nouser2, string text, string jodoshi)
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
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

        /// <summary>
        /// 体言止めの検出ロジックを検証するテスト。
        /// </summary>
        /// <param name="nouse1">The nouse1.</param>
        /// <param name="nouser2">The nouser2.</param>
        /// <param name="text">The text.</param>
        /// <param name="taigendomeFlag">If true, taigendome flag.</param>
        [Theory]
        [InlineData(1, 1, "吾輩は猫だ。", false)]
        [InlineData(1, 2, "吾輩は猫。", true)]
        [InlineData(1, 3, "吾輩は猫", true)]
        [InlineData(1, 4, "吾輩は猫、", false)]
        [InlineData(1, 5, "吾輩は猫？", false)]
        [InlineData(1, 6, "吾輩は猫 ", false)]
        public void TaigendomeTest(int nouse1, int nouser2, string text, bool taigendomeFlag)
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            var lastone = tokens[tokens.Count() - 1];
            var lasttwo = tokens[tokens.Count() - 2];
            bool isTaigendome = false;

            if (lastone.Tags[0] == "名詞")
            {
                isTaigendome = true;
            }
            else if (lastone.Tags[0] == "記号" && lastone.Tags[1] == "句点" && lasttwo.Tags[0] == "名詞")
            {
                isTaigendome = true;
            }
            else
            {
                isTaigendome = false;
            }

            isTaigendome.Should().Be(taigendomeFlag);
        }

        [Theory(DisplayName = "Tokenize結果を目視確認するだけのテスト")]
        // デフォルト辞書はMeCab-IPADIC辞書なので「りんごジュース」が「りん|ご|ジュース」に分割される。
        [InlineData("001", "りんごジュースを飲んだ。", "りん|ご|ジュース|を|飲ん|だ|。")]
        [InlineData("002", "体言止めのりんご。", "体言|止め|の|りんご|。")]
        [InlineData("003", "きゃりーぱみゅぱみゅもゲスの極み乙女。もモーニング娘。も問題なく分割できます。",
            "きゃ|り|ー|ぱみゅぱみゅも|ゲス|の|極み|乙女|。|も|モーニング|娘|。|も|問題|なく|分割|でき|ます|。")]
        [InlineData("004", "それは不謹慎。", "それ|は|不謹慎|。")]
        [InlineData("005", "それは不謹慎だ。", "それ|は|不謹慎|だ|。")]
        [InlineData("006", "体言止め。", "体言|止め|。")]
        public void DisplayTokens(string nouse1, string text, string expected)
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            tokens.Select(t => t.Surface).Should().BeEquivalentTo(expected.Split('|'));
        }

        [Fact(Skip = "動作速度の確認のためのものなので回帰テストには含めない。")]
        public void Tokenize吾輩は猫であるTest()
        {
            // MEMO: このテストケースはLucene.NETのKuromoji.NETの速度検証のためのテストケースである。
            // 他のTokenizerとの比較値は『2019年末版 形態素解析器の比較』
            // https://qiita.com/hi-asano/items/aaf406db875f1c81530e
            // を参照のこと。条件を合わせるために『言語処理100本ノック』
            // https://www.cl.ecei.tohoku.ac.jp/nlp100/
            // からDLできる夏目漱石『吾輩は猫である』のテキストデータで、空行を除くと9210行、約31万8千字の分量である。
            // 著作権切れのデータではあるが、リポジトリにはPUSHしていないので動作確認前にDLすること。
            // このテストをCPU : Ryzen 5 8600G, MEM : 32GB環境で実行したところ、処理時間は毎回約700ms程度であった。
            // これは動作環境に依存すると思われるが、参考値として記載しておく。
            // 元のQiita記事の実行環境がどのようなものであったかは不明であるが、
            // MeCabには劣るがJUMANやSudachiより1オーダー高速でありエンドユーザの実行環境でもストレス無く利用できるお思われる。

            string currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Tokenizer", "DATA", "neko.txt");
            var fileContents = File.ReadAllText(filePath);

            var sw = new Stopwatch();
            sw.Start();

            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(fileContents, 1));

            sw.Stop();

            // Token目視。
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            output.WriteLine(sw.Elapsed.Milliseconds.ToString());
        }
    }
}
