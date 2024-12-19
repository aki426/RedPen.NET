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
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tokenizer.Tests
{
    public class KuromojiTokenizerTests
    {
        private ITestOutputHelper output;

        public KuromojiTokenizerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tokenizes the test.
        /// </summary>
        [Fact(DisplayName = "Lucene.Net標準のKuromoji＋IPAdicの基本的なTokenizeテスト")]
        public void TokenizeTest()
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();

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
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
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
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
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
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
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
        [InlineData("007", "総額は$123,456,789.45", "総額|は|$|123|,|456|,|789|.|45")]
        [InlineData("008", "123", "123")]
        [InlineData("009", "ジュースはジューサーで作りますが、パン・アメリカ総合研究所はアメリカパンを作る会社です。",
            "ジュース|は|ジューサー|で|作り|ます|が|、|パン|・|アメリカ|総合|研究所|は|アメリカ|パン|を|作る|会社|です|。")]
        // MEMO: Kuromoji + IPADIC辞書の場合はカタカナ表現をカタカナのみで分割するが、SudachiやNeologdの場合分割単位が異なる点に注意。
        // MEMO: 別の日本語Tokenizerを用いる場合は、KatakanaSpellCheckなどカタカナ表現のみでTokenを形成することを前提とするValidatorのロジックも再検討する必要がある。
        [InlineData("010", "カタカナ表現はプラスチック樹脂やシス卿やジュース製造機のように1単語であっても分割します。",
            "カタカナ|表現|は|プラスチック|樹脂|や|シス|卿|や|ジュース|製造|機|の|よう|に|1|単語|で|あっ|て|も|分割|し|ます|。")]
        public void DisplayTokens(string nouse1, string text, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }

            tokens.Select(t => t.Surface).Should().BeEquivalentTo(expected.Split('|'));
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-ア")]
        [InlineData("001", "俺は歩く。", "*:名詞,代名詞:*", "俺")]
        [InlineData("002", "俺たちは歩く。", "*:名詞,代名詞:*", "俺")]
        [InlineData("003", "俺達は歩く。", "*:名詞,代名詞:*", "俺")]
        [InlineData("004", "俺らは歩く。", "*:名詞,代名詞:*", "俺")]
        [InlineData("005", "俺等は歩く。", "*:名詞,代名詞:*", "俺")]
        [InlineData("006", "おれは歩く。", "*:名詞,代名詞:*", "おれ")]
        [InlineData("007", "おれたちは歩く。", "*:名詞,代名詞:*", "おれ")]
        [InlineData("008", "おれらは歩く。", "*:名詞,代名詞:*", "おれ")]
        [InlineData("009", "彼は歩く。", "*:名詞,代名詞:*", "彼")]
        [InlineData("010", "彼らは歩く。", "*:名詞,代名詞:*", "彼ら")]
        [InlineData("011", "彼等は歩く。", "*:名詞,代名詞:*", "彼等")]
        [InlineData("012", "かれは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("013", "かれらは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("014", "彼女は歩く。", "*:名詞,代名詞:*", "彼女")]
        [InlineData("015", "彼女らは歩く。", "*:名詞,代名詞:*", "彼女ら")]
        [InlineData("016", "彼女等は歩く。", "*:名詞,代名詞:*", "彼女")]
        [InlineData("017", "かのじょは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("018", "かのじょらは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("019", "誰が歩く。", "*:名詞,代名詞:*", "誰")]
        [InlineData("020", "だれが歩く。", "*:名詞,代名詞:*", "だれ")]
        [InlineData("021", "何が走る。", "*:名詞,代名詞:*", "何")]
        [InlineData("022", "なにが走る。", "*:名詞,代名詞:*", "なに")]
        [InlineData("023", "僕は歩く。", "*:名詞,代名詞:*", "僕")]
        [InlineData("024", "僕たちは歩く。", "*:名詞,代名詞:*", "僕")]
        [InlineData("025", "僕達は歩く。", "*:名詞,代名詞:*", "僕達")]
        [InlineData("026", "僕らは歩く。", "*:名詞,代名詞:*", "僕ら")]
        [InlineData("027", "僕等は歩く。", "*:名詞,代名詞:*", "僕")]
        [InlineData("028", "ぼくは歩く。", "*:名詞,代名詞:*", "ぼく")]
        [InlineData("029", "ぼくたちは歩く。", "*:名詞,代名詞:*", "ぼく")]
        [InlineData("030", "ぼくらは歩く。", "*:名詞,代名詞:*", "ぼく")]
        [InlineData("031", "私は歩く。", "*:名詞,代名詞:*", "私")]
        [InlineData("032", "私たちは歩く。", "*:名詞,代名詞:*", "私")]
        [InlineData("033", "私達は歩く。", "*:名詞,代名詞:*", "私")]
        [InlineData("034", "私らは歩く。", "*:名詞,代名詞:*", "私")]
        [InlineData("035", "私等は歩く。", "*:名詞,代名詞:*", "私")]
        [InlineData("036", "わたしは歩く。", "*:名詞,代名詞:*", "わたし")]
        [InlineData("037", "わたしたちは歩く。", "*:名詞,代名詞:*", "わたし")]
        [InlineData("038", "わたしらは歩く。", "*:名詞,代名詞:*", "わたし")]
        [InlineData("039", "貴方は歩く。", "*:名詞,代名詞:*", "貴方")]
        [InlineData("040", "貴方たちは歩く。", "*:名詞,代名詞:*", "貴方")]
        [InlineData("041", "貴方達は歩く。", "*:名詞,代名詞:*", "貴方")]
        [InlineData("042", "貴方がたは歩く。", "*:名詞,代名詞:*", "貴方")]
        [InlineData("043", "貴方方は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("044", "貴女は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("045", "貴女たちは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("046", "貴女達は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("047", "貴女がたは歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("048", "貴女方は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("049", "あなたは歩く。", "*:名詞,代名詞:*", "あなた")]
        [InlineData("050", "あなたたちは歩く。", "*:名詞,代名詞:*", "あなた")]
        [InlineData("051", "あなたがたは歩く。", "*:名詞,代名詞:*", "あなた")]
        [InlineData("052", "あんたは歩く。", "*:名詞,代名詞:*", "あんた")]
        [InlineData("053", "あんたたちは歩く。", "*:名詞,代名詞:*", "あんた")]
        [InlineData("054", "あんたがたは歩く。", "*:名詞,代名詞:*", "あんた")]
        [InlineData("055", "我々は歩く。", "*:名詞,代名詞:*", "我々")]
        [InlineData("056", "われわれは歩く。", "*:名詞,代名詞:*", "われわれ")]
        [InlineData("057", "我は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("058", "我らは歩く。", "*:名詞,代名詞:*", "我ら")]
        [InlineData("059", "我等は歩く。", "*:名詞,代名詞:*", "")]
        [InlineData("060", "われは歩く。", "*:名詞,代名詞:*", "われ")]
        [InlineData("061", "われらは歩く。", "*:名詞,代名詞:*", "われ")]
        public void DaimeishiMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★代名詞 {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-イ")]
        [InlineData("001", "彼は主張する余り、", "*:副詞:*", "")]
        [InlineData("002", "彼は主張するあまり、", "*:副詞:*", "")]
        [InlineData("003", "彼は至って真剣に、", "*:副詞:*", "至って")]
        [InlineData("004", "彼はいたって真剣に、", "*:副詞:*", "")]
        [InlineData("005", "彼は大いに憤慨して、", "*:副詞:*", "大いに")]
        [InlineData("006", "彼はおおいに憤慨して、", "*:副詞:*", "おおいに")]
        [InlineData("007", "彼は恐らく行った。", "*:副詞:*", "恐らく")]
        [InlineData("008", "彼はおそらく行った。", "*:副詞:*", "おそらく")]
        [InlineData("009", "彼は概して慎重で、", "*:副詞:*", "概して")]
        [InlineData("010", "彼はがいして慎重で、", "*:副詞:*", "がいして")]
        [InlineData("011", "彼は必ず言った。", "*:副詞:*", "必ず")]
        [InlineData("012", "彼はかならず言った。", "*:副詞:*", "かならず")]
        [InlineData("013", "彼は必ずしも慎重ではなく、", "*:副詞:*", "必ずしも")]
        [InlineData("014", "彼はかならずしも慎重ではなく、", "*:副詞:*", "かならずしも")]
        [InlineData("015", "彼は辛うじて慎重に、", "*:副詞:*", "辛うじて")]
        [InlineData("016", "彼はかろうじて慎重に、", "*:副詞:*", "かろうじて")]
        [InlineData("017", "彼は極めて慎重に、", "*:副詞:*", "極めて")]
        [InlineData("018", "彼はきわめて慎重に、", "*:副詞:*", "きわめて")]
        [InlineData("019", "彼は殊に慎重に、", "*:副詞:*", "殊に")]
        [InlineData("020", "彼はことに慎重に、", "*:副詞:*", "ことに")]
        [InlineData("021", "彼は更に慎重に、", "*:副詞:*", "更に")]
        [InlineData("022", "彼はさらに慎重に、", "*:副詞:*", "さらに")]
        [InlineData("023", "彼は実に慎重に、", "*:副詞:*", "実に")]
        [InlineData("024", "彼はじつに慎重に、", "*:副詞:*", "じつに")]
        [InlineData("025", "彼は少なくとも行った。", "*:副詞:*", "少なくとも")]
        [InlineData("026", "彼はすくなくとも行った。", "*:副詞:*", "すくなくとも")]
        [InlineData("027", "彼は少し慎重に、", "*:副詞:*", "少し")]
        [InlineData("028", "彼はすこし慎重に、", "*:副詞:*", "すこし")]
        [InlineData("029", "彼は既に行った。", "*:副詞:*", "既に")]
        [InlineData("030", "彼はすでに行った。", "*:副詞:*", "すでに")]
        [InlineData("031", "彼は全て行った。", "*:副詞:*", "")]
        [InlineData("032", "彼はすべて行った。", "*:副詞:*", "")]
        [InlineData("033", "彼は切に願った。", "*:副詞:*", "切に")]
        [InlineData("034", "彼はせつに願った。", "*:副詞:*", "せつに")]
        [InlineData("035", "彼は大して慎重ではなく、", "*:副詞:*", "大して")]
        [InlineData("036", "彼はたいして慎重ではなく、", "*:副詞:*", "たいして")]
        [InlineData("037", "彼は絶えず慎重に、", "*:副詞:*", "絶えず")]
        [InlineData("038", "彼はたえず慎重に、", "*:副詞:*", "たえず")]
        [InlineData("039", "彼らは互いに主張し、", "*:副詞:*", "互いに")]
        [InlineData("040", "彼らはたがいに主張し、", "*:副詞:*", "")]
        [InlineData("041", "彼は直ちに行った。", "*:副詞:*", "直ちに")]
        [InlineData("042", "彼はただちに行った。", "*:副詞:*", "ただちに")]
        [InlineData("043", "彼は例えば慎重に、", "*:副詞:*", "")]
        [InlineData("044", "彼はたとえば慎重に、", "*:副詞:*", "")]
        [InlineData("045", "彼に次いで慎重に、", "*:副詞:*", "")]
        [InlineData("046", "彼についで慎重に、", "*:副詞:*", "ついで")]
        [InlineData("047", "彼は努めて明るく、", "*:副詞:*", "")]
        [InlineData("048", "彼はつとめて明るく、", "*:副詞:*", "つとめて")]
        [InlineData("049", "彼は常に慎重に、", "*:副詞:*", "常に")]
        [InlineData("050", "彼はつねに慎重に、", "*:副詞:*", "つねに")]
        [InlineData("051", "彼は特に慎重に、", "*:副詞:*", "特に")]
        [InlineData("052", "彼はとくに慎重に、", "*:副詞:*", "とくに")]
        [InlineData("053", "彼は突然言った。", "*:副詞:*", "突然")]
        [InlineData("054", "彼はとつぜん言った。", "*:副詞:*", "とつぜん")]
        [InlineData("055", "彼は初めて言った。", "*:副詞:*", "初めて")]
        [InlineData("056", "彼ははじめて言った。", "*:副詞:*", "はじめて")]
        [InlineData("057", "彼は果たして慎重か、", "*:副詞:*", "果たして")]
        [InlineData("058", "彼ははたして慎重か、", "*:副詞:*", "はたして")]
        [InlineData("059", "彼は甚だ慎重に、", "*:副詞:*", "甚だ")]
        [InlineData("060", "彼ははなはだ慎重に、", "*:副詞:*", "はなはだ")]
        [InlineData("061", "彼は再び言った。", "*:副詞:*", "再び")]
        [InlineData("062", "彼はふたたび言った。", "*:副詞:*", "ふたたび")]
        [InlineData("063", "彼は全く慎重に、", "*:副詞:*", "全く")]
        [InlineData("064", "彼はまったく慎重に、", "*:副詞:*", "まったく")]
        [InlineData("065", "彼は無論慎重に、", "*:副詞:*", "無論")]
        [InlineData("066", "彼はむろん慎重に、", "*:副詞:*", "むろん")]
        [InlineData("067", "彼は最も慎重に、", "*:副詞:*", "最も")]
        [InlineData("068", "彼はもっとも慎重に、", "*:副詞:*", "もっとも")]
        [InlineData("069", "彼は専ら慎重に、", "*:副詞:*", "専ら")]
        [InlineData("070", "彼はもっぱら慎重に、", "*:副詞:*", "もっぱら")]
        [InlineData("071", "彼は僅かの差で、", "*:副詞:*", "僅か")]
        [InlineData("072", "彼はわずかの差で、", "*:副詞:*", "わずか")]
        [InlineData("073", "彼は体の割に慎重な方で、", "*:副詞:*", "")]
        [InlineData("074", "彼は体のわりに慎重な方で、", "*:副詞:*", "")]
        [InlineData("075", "明くる日の朝、", "*:連体詞:*", "")]
        [InlineData("076", "あくる日の朝、", "*:連体詞:*", "あくる")]
        [InlineData("077", "大きなカブが、", "*:連体詞:*", "大きな")]
        [InlineData("078", "おおきなカブが、", "*:連体詞:*", "おおきな")]
        [InlineData("079", "来る日曜日に、", "*:連体詞:*", "来る")]
        [InlineData("080", "きたる日曜日に、", "*:連体詞:*", "きたる")]
        [InlineData("081", "去る8月からずっと、", "*:連体詞:*", "去る")]
        [InlineData("082", "さる8月からずっと、", "*:連体詞:*", "さる")]
        [InlineData("083", "小さなカブが、", "*:連体詞:*", "小さな")]
        [InlineData("084", "ちいさなカブが、", "*:連体詞:*", "ちいさな")]
        [InlineData("085", "我が国において、", "*:連体詞:*", "")]
        [InlineData("086", "わが国において、", "*:連体詞:*", "")]
        [InlineData("087", "可也大きなカブが、", "*:副詞:*", "可也")]
        [InlineData("088", "かなり大きなカブが、", "*:副詞:*", "かなり")]
        [InlineData("089", "不図思い立って、", "*:副詞:*", "")]
        [InlineData("090", "ふと思い立って、", "*:副詞:*", "ふと")]
        [InlineData("091", "矢張り困難が、", "*:副詞:*", "")]
        [InlineData("092", "やはり困難が、", "*:副詞:*", "やはり")]
        [InlineData("093", "余程堪えたと見えて、", "*:副詞:*", "余程")]
        [InlineData("094", "よほど堪えたと見えて、", "*:副詞:*", "よほど")]
        public void FukushiMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-ウ")]
        [InlineData("001", "御案内", "*:接頭詞:*", "御")]
        [InlineData("002", "御あんない", "*:接頭詞:*", "御")]
        [InlineData("003", "ご案内", "*:接頭詞:*", "ご")]
        [InlineData("004", "ごあんない", "*:接頭詞:*", "ご")]
        [InlineData("005", "御挨拶", "*:接頭詞:*", "御")]
        [InlineData("006", "御あいさつ", "*:接頭詞:*", "御")]
        [InlineData("007", "ご挨拶", "*:接頭詞:*", "ご")]
        [InlineData("008", "ごあいさつ", "*:接頭詞:*", "ご")]
        [InlineData("009", "御尤も", "*:接頭詞:*", "御")]
        [InlineData("010", "御もっとも", "*:接頭詞:*", "御")]
        [InlineData("011", "ご尤も", "*:接頭詞:*", "ご")]
        [InlineData("012", "ごもっとも", "*:接頭詞:*", "ご")]
        public void PrefixGoMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-エ")]
        [InlineData("001", "惜しげもなく、", "*:名詞,接尾:*", "")]
        [InlineData("002", "惜し気もなく、", "*:名詞,接尾:*", "")]
        [InlineData("003", "悔しげな表情で、", "*:名詞,接尾:*", "げ")]
        [InlineData("004", "悔し気な表情で、", "*:名詞,接尾:*", "")]
        [InlineData("005", "私どもも、", "*:名詞,接尾:*", "ども")]
        [InlineData("006", "私共も、", "*:名詞,接尾:*", "共")]
        [InlineData("007", "子どもも、", "*:名詞,接尾:*", "")]
        [InlineData("008", "子供も、", "*:名詞,接尾:*", "")]
        [InlineData("009", "偉ぶる様子も、", "*:名詞,接尾:*", "")]
        [InlineData("010", "偉振る様子も、", "*:名詞,接尾:*", "")]
        [InlineData("011", "弱みに付け込んで、", "*:名詞,接尾:*", "")]
        [InlineData("012", "弱味に付け込んで、", "*:名詞,接尾:*", "")]
        [InlineData("013", "強みを活かして、", "*:名詞,接尾:*", "")]
        [InlineData("014", "強味を活かして、", "*:名詞,接尾:*", "")]
        [InlineData("015", "少なめに言った。", "*:名詞,接尾:*", "")]
        [InlineData("016", "少な目に言った。", "*:名詞,接尾:*", "")]
        [InlineData("017", "強めの火力で、", "*:名詞,接尾:*", "")]
        [InlineData("018", "強目の火力で、", "*:名詞,接尾:*", "目")]
        public void SuffixMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-オ")]
        [InlineData("001", "おって連絡します", "*:接続詞:*", "")]
        [InlineData("002", "おって、連絡します", "*:接続詞:*", "")]
        [InlineData("003", "追って連絡します", "*:接続詞:*", "追って")]
        [InlineData("004", "追って、連絡します", "*:接続詞:*", "追って")]
        [InlineData("005", "鳥かつ哺乳類です。", "*:接続詞:*", "かつ")]
        [InlineData("006", "鳥、かつ哺乳類です。", "*:接続詞:*", "かつ")]
        [InlineData("007", "鳥且つ哺乳類です。", "*:接続詞:*", "")]
        [InlineData("008", "鳥、且つ哺乳類です。", "*:接続詞:*", "")]
        [InlineData("009", "したがって賛成です。", "*:接続詞:*", "したがって")]
        [InlineData("010", "したがって、賛成です。", "*:接続詞:*", "したがって")]
        [InlineData("011", "従って賛成です。", "*:接続詞:*", "従って")]
        [InlineData("012", "従って、賛成です。", "*:接続詞:*", "従って")]
        [InlineData("013", "ただし条件があります。", "*:接続詞:*", "ただし")]
        [InlineData("014", "ただし、条件があります。", "*:接続詞:*", "ただし")]
        [InlineData("015", "但し条件があります。", "*:接続詞:*", "但し")]
        [InlineData("016", "但し、条件があります。", "*:接続詞:*", "但し")]
        [InlineData("017", "ついては提案があります。", "*:接続詞:*", "ついては")]
        [InlineData("018", "ついては、提案があります。", "*:接続詞:*", "ついては")]
        [InlineData("019", "付いては提案があります。", "*:接続詞:*", "")]
        [InlineData("020", "付いては、提案があります。", "*:接続詞:*", "")]
        [InlineData("021", "ところが反対されました。", "*:接続詞:*", "ところが")]
        [InlineData("022", "ところが、反対されました。", "*:接続詞:*", "ところが")]
        [InlineData("023", "所が反対されました。", "*:接続詞:*", "")]
        [InlineData("024", "所が、反対されました。", "*:接続詞:*", "")]
        [InlineData("025", "ところでどうですか。", "*:接続詞:*", "ところで")]
        [InlineData("026", "ところで、どうですか。", "*:接続詞:*", "ところで")]
        [InlineData("027", "所でどうですか。", "*:接続詞:*", "")]
        [InlineData("028", "所で、どうですか。", "*:接続詞:*", "")]
        [InlineData("029", "また反対意見については、", "*:接続詞:*", "また")]
        [InlineData("030", "また、反対意見については、", "*:接続詞:*", "また")]
        [InlineData("031", "又反対意見については、", "*:接続詞:*", "又")]
        [InlineData("032", "又、反対意見については、", "*:接続詞:*", "又")]
        [InlineData("033", "復反対意見については、", "*:接続詞:*", "")]
        [InlineData("034", "復、反対意見については、", "*:接続詞:*", "")]
        [InlineData("035", "ゆえに賛成です。", "*:接続詞:*", "ゆえに")]
        [InlineData("036", "ゆえに、賛成です。", "*:接続詞:*", "ゆえに")]
        [InlineData("037", "故に賛成です。", "*:接続詞:*", "故に")]
        [InlineData("038", "故に、賛成です。", "*:接続詞:*", "故に")]
        [InlineData("039", "賛成および反対意見は、", "*:接続詞:*", "および")]
        [InlineData("040", "賛成及び反対意見は、", "*:接続詞:*", "及び")]
        [InlineData("041", "賛成ならびに反対意見は、", "*:接続詞:*", "ならびに")]
        [InlineData("042", "賛成並びに反対意見は、", "*:接続詞:*", "並びに")]
        [InlineData("043", "賛成または反対の方は、", "*:接続詞:*", "または")]
        [InlineData("044", "賛成又は反対の方は、", "*:接続詞:*", "又は")]
        [InlineData("045", "賛成復は反対の方は、", "*:接続詞:*", "")]
        [InlineData("046", "賛成もしくは反対の方は、", "*:接続詞:*", "もしくは")]
        [InlineData("047", "賛成若しくは反対の方は、", "*:接続詞:*", "若しくは")]
        public void ConjunctionMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-カ")]
        [InlineData("001", "現地には，行かない。", "*:助動詞,特殊・ナイ:ナイ", "ない")]
        [InlineData("002", "現地には，行か無い。", "*:助動詞,形容詞・イ段:ナイ", "無い")]
        [InlineData("003", "それ以外に方法がないようだ。", "*:名詞,非自立,助動詞語幹:ヨウ", "よう")]
        [InlineData("004", "それ以外に方法がない様だ。", "*:名詞,非自立,助動詞語幹:ヨウ", "様")]
        [InlineData("005", "二十歳ぐらいの人", "*:助詞,副助詞:グライ", "ぐらい")]
        [InlineData("006", "二十歳位の人", "*:名詞,接尾:イ", "位")]
        [InlineData("007", "調査しただけである。", "*:助詞,副助詞:ダケ", "だけ")]
        [InlineData("008", "三日ほど経過した。", "*:名詞,接尾,助数詞:ニチ + *:助詞,副助詞:ホド", "日")]
        [InlineData("009", "三日程経過した。", "*:名詞,一般:ニッテイ", "日程")]
        public void JodoshiJoshiMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        [Theory(DisplayName = "公用文における漢字使用等について1-(2)-キ")]
        [InlineData("001", "その点に問題がある。", "*:動詞,自立:アル", "ある")]
        [InlineData("002", "その点に問題が有る。", "*:動詞,自立:アル", "有る")]
        [InlineData("003", "その点に問題が在る。", "*:動詞,自立:アル", "在る")]
        [InlineData("004", "ここに関係者がいる。", "*:動詞,自立:イル", "いる")]
        [InlineData("005", "ここに関係者が居る。", "*:動詞,自立:イル", "居る")]
        [InlineData("006", "許可しないことがある。", "*:動詞,自立:アル", "ある")]
        [InlineData("007", "許可しないことが有る。", "*:動詞,自立:アル", "有る")]
        [InlineData("008", "だれでも利用ができる。", "*:動詞,自立:デキル", "できる")]
        [InlineData("009", "だれでも利用が出来る。", "*:動詞,自立:デキル", "出来る")]
        [InlineData("010", "次のとおりである。", "*:名詞:トオリ", "とおり")]
        [InlineData("011", "次の通りである。", "*:名詞:トオリ", "通り")]
        [InlineData("012", "事故のときは連絡する。", "*:名詞,非自立:トキ", "とき")]
        [InlineData("013", "事故の時は連絡する。", "*:名詞,非自立:トキ", "時")]
        [InlineData("014", "現在のところ差し支えない。", "*:名詞,非自立:トコロ", "ところ")]
        [InlineData("015", "現在の所差し支えない。", "*:名詞,非自立:トコロ", "所")]
        [InlineData("016", "説明するとともに意見を聞く。", "*:助詞,格助詞:トトモニ", "とともに")]
        [InlineData("017", "説明すると共に意見を聞く。", "*:助詞,格助詞:トトモニ", "と共に")]
        [InlineData("018", "欠点がない。", "*:形容詞,自立:ナイ", "ない")]
        [InlineData("019", "欠点が無い。", "*:形容詞,自立:ナイ", "無い")]
        [InlineData("020", "合計すると１万円になる。", "*:動詞,自立:ナル", "なる")]
        [InlineData("021", "合計すると１万円に成る。", "*:動詞,自立:ナル", "成る")]
        [InlineData("022", "そのほか、特別の場合を除くほか、除かない。", "*:助動詞,特殊・ナイ:ナイ", "ない")]
        [InlineData("023", "そのほか、特別の場合を除くほか、除か無い。", "*:助動詞,形容詞・イ段:ナイ", "無い")]
        [InlineData("024", "正しいものと認める。", "*:名詞,非自立:モノ", "もの")]
        [InlineData("025", "正しい物と認める。", "*:名詞,非自立:モノ", "物")]
        [InlineData("026", "一部の反対のゆえにはかどらない。", "*:名詞,非自立:ユエ", "ゆえ")]
        [InlineData("027", "一部の反対の故にはかどらない。", "*:名詞,非自立:ユエ", "故")]
        [InlineData("028", "賛成するわけにはいかない。", "*:名詞,非自立:ワケ", "わけ")]
        [InlineData("029", "賛成する訳にはいかない。", "*:名詞,非自立:ワケ", "訳")]
        [InlineData("030", "間違いかもしれない。", "*:動詞,自立:シレ", "しれ")]
        [InlineData("031", "間違いかも知れない。", "*:動詞,自立:シレ", "知れ")]
        [InlineData("032", "図書を貸してあげる。", "*:動詞,非自立:アゲル", "あげる")]
        [InlineData("033", "図書を貸して上げる。", "*:動詞,非自立:アゲル", "上げる")]
        [InlineData("034", "負担が増えていく。", "*:動詞,非自立:イク", "いく")]
        [InlineData("035", "負担が増えて行く。", "*:動詞,非自立:イク", "行く")]
        [InlineData("036", "報告していただく。", "*:動詞,自立:イタダク", "いただく")]
        [InlineData("037", "報告して頂く。", "*:動詞,自立:イタダク", "頂く")]
        [InlineData("038", "報告していただきます。", "*:動詞,非自立:イタダキ", "いただき")]
        [InlineData("039", "報告して頂きます。", "*:動詞,非自立:イタダキ", "頂き")]
        [InlineData("040", "通知しておく。", "*:動詞,非自立:オク", "おく")]
        [InlineData("041", "通知して置く。", "*:動詞,自立:オク", "置く")]
        [InlineData("042", "問題点を話してください。", "*:動詞,非自立:クダサイ", "ください")]
        [InlineData("043", "問題点を話して下さい。", "*:動詞,非自立:クダサイ", "下さい")]
        [InlineData("044", "寒くなってくる。", "*:動詞,非自立:クル", "くる")]
        [InlineData("045", "寒くなって来る。", "*:動詞,非自立:クル", "来る")]
        [InlineData("046", "書いてしまう。", "*:動詞,非自立:シマウ", "しまう")]
        [InlineData("047", "書いて仕舞う。", "*:動詞,自立:シマウ", "仕舞う")]
        [InlineData("048", "見てみる。", "*:動詞,非自立:ミル", "みる")]
        [InlineData("049", "見て見る。", "*:動詞,自立:ミル", "見る")]
        [InlineData("050", "連絡してよい。", "*:形容詞,非自立:ヨイ", "よい")]
        [InlineData("051", "連絡して良い。", "*:形容詞,非自立:ヨイ", "良い")]
        [InlineData("052", "調査だけにすぎない。", "*:動詞,自立:スギ", "すぎ")]
        [InlineData("053", "調査だけに過ぎない。", "*:動詞,自立:スギ", "過ぎ")]
        [InlineData("054", "これについて考慮する。", "*:助詞,格助詞:ニツイテ", "について")]
        [InlineData("055", "これに付いて考慮する。", "*:動詞,自立:ツイ", "付い")]
        public void OkuriganaMatchGrammarRuleTest(string nouse1, string text, string rule, string expected)
        {
            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(text, 1));

            // Token目視。
            tokens.ForEach(t => output.WriteLine(t.ConvertToGrammarRuleText()));

            var (isMatch, tokens1) =
                GrammarRuleExtractor.Run(rule).MatchesConsecutive(tokens); //.isMatch.Should().BeTrue();

            if (isMatch)
            {
                var matched = string.Join("|", tokens1.Select(lis => lis.First()).Select(t => t.Surface));
                output.WriteLine("");
                output.WriteLine($"★ {matched} にマッチしました。");
                matched.Should().Be(expected);
            }
            else if (!isMatch && expected == "")
            {
                // マッチせず、マッチが想定されていない。
                output.WriteLine("何もマッチしませんでした。想定内です。");
            }
            else
            {
                output.WriteLine("想定外です。マッチしませんでした。");
                true.Should().BeFalse();
            }
        }

        //[Fact(Skip = "動作速度の確認のためのものなので回帰テストには含めない。")]
        [Fact]
        public void Tokenize吾輩は猫であるTest()
        {
            // MEMO: このテストケースはLucene.NETのKuromoji.NETの速度検証のためのテストケースである。
            // 他のTokenizerとの比較値は『2019年末版 形態素解析器の比較』
            // https://qiita.com/hi-asano/items/aaf406db875f1c81530e
            // を参照のこと。条件を合わせるために『言語処理100本ノック』
            // https://www.cl.ecei.tohoku.ac.jp/nlp100/
            // からDLできる夏目漱石『吾輩は猫である』のテキストデータで、空行を除くと9210行、約31万8千字の分量である。
            // 著作権切れのデータではあるが、リポジトリにはPUSHしていないので動作確認前にDLすること。
            // このテストをCPU : Ryzen 5 8600G, MEM : 32GB環境で実行したところ、処理時間は毎回約1050~1100ms程度であった。
            // これは動作環境に依存すると思われるが、参考値として記載しておく。
            // 元のQiita記事の実行環境がどのようなものであったかは不明であるが、
            // MeCabには劣るがJUMANやSudachiより1オーダー高速でありエンドユーザの実行環境でもストレス無く利用できると思われる。

            // Arrange
            string currentDirectory = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(currentDirectory, "Tokenizer", "DATA", "neko.txt");

            // ファイルの中身が空でないことも確認する場合
            FileInfo file = new FileInfo(filePath);
            file.Exists.Should().BeTrue("テストデータ「neko.txt」が存在しません。Download-WagahaiHaNekoDearu.ps1を実行してください。");
            file.Length.Should().NotBe(0, "ファイルが空です");

            // Act
            var fileContents = File.ReadAllText(filePath);

            var sw = new Stopwatch();
            sw.Start();

            KuromojiTokenizer tokenizer = new KuromojiTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize(new Sentence(fileContents, 1));

            sw.Stop();

            // Assert
            // 実行環境によるが30万字1500ms以下をスレショルドとした。
            sw.ElapsedMilliseconds.Should().BeLessThan(1500);

            // Token目視。
            output.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds}ms");

            // NOTE: 以下、全Tokenの表示。非常に長い時間がかかるので回帰テストには入れない方が良いがデバッグ出力としてコメントアウトして残す。。
            foreach (var token in tokens)
            {
                output.WriteLine(token.ToString());
            }
        }
    }
}
