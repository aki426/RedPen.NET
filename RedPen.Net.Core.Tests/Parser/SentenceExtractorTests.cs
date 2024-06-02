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
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Parser
{
    /// <summary>
    /// The sentence extractor tests.
    /// </summary>
    public class SentenceExtractorTests
    {
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceExtractorTests"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public SentenceExtractorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// Creates the sentences.
        /// </summary>
        /// <param name="outputPositions">The output positions.</param>
        /// <param name="line">The line.</param>
        /// <returns>A list of Sentences.</returns>
        private List<Sentence> CreateSentences(List<(int, int)> outputPositions, string line)
        {
            var output = new List<Sentence>();

            int lineNum = 1;
            int startOffset = 0;

            foreach (var (first, second) in outputPositions)
            {
                var sentence = new Sentence(line.Substring(first, second - first), lineNum, startOffset);
                // MEMO: OffsetMap生成のために開始位置オフセットをコンストラクタ引数で渡しておく。
                output.Add(sentence);

                // 改行が含まれていた場合Sentence生成時にOffsetMapが生成されるので、その最後の位置を取得して次のSentenceの開始位置とする。
                lineNum = sentence.OffsetMap.Last().LineNum;
                startOffset = sentence.OffsetMap.Last().Offset + 1;
            }
            return output;
        }

        [Theory]
        // 最もシンプルなケース。1行で1センテンス。
        [InlineData("001", "this is a pen.", 1, "this is a pen.", "")]
        // 1行でピリオド終わりのテキスト複数の表現。
        [InlineData("002", "this is a pen. that is a paper.", 2, "this is a pen.", " that is a paper.")]
        // 1行でピリオドと疑問符で終わるテキスト複数の表現。
        [InlineData("003", "is this a pen? that is a paper.", 2, "is this a pen?", " that is a paper.")]
        // 1行でピリオド終わりが無い複数テキストの表現。
        [InlineData("004", "this is a pen. that is a paper", 1, "this is a pen.", "")]
        // 1行でダブルクォーテーション終わりのテキスト表現。
        [InlineData("004", "this is a \"pen.\"", 1, "this is a \"pen.\"", "")]
        // 1行でシングルクォーテーション終わりのテキスト表現。
        [InlineData("005", "this is a \'pen.\'", 1, "this is a \'pen.\'", "")]
        // 1行でダブルクォーテーションで囲まれた単語のテキスト表現。
        [InlineData("006", "this is a \"pen\".", 1, "this is a \"pen\".", "")]
        // 1行でシングルクォーテーションで囲まれた単語のテキスト表現。
        [InlineData("007", "this is a \'pen\'.", 1, "this is a \'pen\'.", "")]
        // 1行でダブルクォーテーションで終わる複数テキストの表現。
        [InlineData("008", "\"this is a pen.\" Another one is not a pen.", 2, "\"this is a pen.\"", " Another one is not a pen.")]
        // 1行で部分的に分割された複数テキストの表現。
        [InlineData("009", "this is a pen. Another\n" + "one is not a pen.", 2, "this is a pen.", " Another\none is not a pen.")]
        // センテンスの終わりで改行された複数テキストの表現を考える。
        [InlineData("010", "this is a pen.\n" + "Another one is not a pen.", 2, "this is a pen.", "\nAnother one is not a pen.")]
        // センテンスの途中で改行された不十分なテキストの表現を考える。
        [InlineData("011", "this is a pen. Another\n", 1, "this is a pen.", "")]
        // センテンス区切りとしては扱わないピリオド。
        [InlineData("012", "He is a Dr. candidate.", 1, "He is a Dr. candidate.", "")]
        // センテンス区切りとしては扱わないピリオド。
        [InlineData("013", "Is he a Dr. candidate? Yes, he is.", 2, "Is he a Dr. candidate?", " Yes, he is.")]
        // void
        [InlineData("014", "", 0, "", "")]
        // ピリオドのみ
        [InlineData("015", ".", 1, ".", "")]
        public void EnglishSentencesTest(string nouse1, string input, int sentenceCount, string extractedFirst, string extractedSecond)
        {
            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(sentenceCount);

            if (sentenceCount == 0)
            {
                output.WriteLine($"outputSentences has no element.");
            }
            // 1つしかセンテンスが抽出されない場合と2つ抽出される場合で分ける。このテストは2センテンスまでなのでそれ以外はエラー。
            else if (sentenceCount == 1)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                outputSentences[0].Content.Should().Be(extractedFirst);
            }
            else if (sentenceCount == 2)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                output.WriteLine($"outputSentences[1].Content : {outputSentences[1].Content}");

                // 2センテンス抽出できた場合は2つ目のセンテンスの終了位置が入力文字列の長さと一致する前提。
                // MEMO: これが崩れるパターンを検証したい場合は要注意。
                outputPositions.Last().second.Should().Be(input.Length);

                outputSentences[0].Content.Should().Be(extractedFirst);
                outputSentences[1].Content.Should().Be(extractedSecond);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Theory]
        //
        [InlineData("001", "これは埼玉ですか？いいえ群馬です。", 2, "これは埼玉ですか？", "いいえ群馬です。")]
        [InlineData("002", "これは埼玉ですか？ いいえ群馬です。", 2, "これは埼玉ですか？", " いいえ群馬です。")]
        [InlineData("003", "これは“群馬。”", 1, "これは“群馬。”", "")]
        [InlineData("004", "これは“群馬。”あれは群馬ではない。", 2, "これは“群馬。”", "あれは群馬ではない。")]
        public void JapaneseSentenceTest(string nouse1, string input, int sentenceCount, string extractedFirst, string extractedSecond)
        {
            // デフォルトのSymbolTableをロード。ja-JPかつzenkakuの設定がロードされる。
            // これによりセンテンス区切り文字は「。」「？」「！」の3種類が、右クォーテーションとして「’」と「”」が適用される。
            SentenceExtractor extractor = new SentenceExtractor(new SymbolTable("ja-JP", "", new List<Symbol>()));
            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(sentenceCount);

            if (sentenceCount == 0)
            {
                output.WriteLine($"outputSentences has no element.");
            }
            // 1つしかセンテンスが抽出されない場合と2つ抽出される場合で分ける。このテストは2センテンスまでなのでそれ以外はエラー。
            else if (sentenceCount == 1)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                outputSentences[0].Content.Should().Be(extractedFirst);
            }
            else if (sentenceCount == 2)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                output.WriteLine($"outputSentences[1].Content : {outputSentences[1].Content}");

                // 2センテンス抽出できた場合は2つ目のセンテンスの終了位置が入力文字列の長さと一致する前提。
                // MEMO: これが崩れるパターンを検証したい場合は要注意。
                outputPositions.Last().second.Should().Be(input.Length);

                outputSentences[0].Content.Should().Be(extractedFirst);
                outputSentences[1].Content.Should().Be(extractedSecond);
            }
            else
            {
                Assert.True(false);
            }
        }

        [Theory]
        [InlineData("001", "それは異なる．たとえば，\n" + "以下のとおりである．", 2, "それは異なる．", "たとえば，\n以下のとおりである．")]
        [InlineData("002", "それは異なる．たとえば，", 1, "それは異なる．", "")]
        public void JapaneseZenkaku2SentenceTest(string nouse1, string input, int sentenceCount, string extractedFirst, string extractedSecond)
        {
            // デフォルトのSymbolTableをロード。ja-JPかつzenkaku2の設定がロードされる。
            // zenkaku2の場合、センテンス区切り文字は「．」「？」「！」の3種類が、右クォーテーションはzenkakuと同じ「’」と「”」が適用される。
            SentenceExtractor extractor = new SentenceExtractor(new SymbolTable("ja-JP", "zenkaku2", new List<Symbol>()));
            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(sentenceCount);

            if (sentenceCount == 0)
            {
                output.WriteLine($"outputSentences has no element.");
            }
            // 1つしかセンテンスが抽出されない場合と2つ抽出される場合で分ける。このテストは2センテンスまでなのでそれ以外はエラー。
            else if (sentenceCount == 1)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                outputSentences[0].Content.Should().Be(extractedFirst);
            }
            else if (sentenceCount == 2)
            {
                output.WriteLine($"outputSentences[0].Content : {outputSentences[0].Content}");
                output.WriteLine($"outputSentences[1].Content : {outputSentences[1].Content}");

                // 2センテンス抽出できた場合は2つ目のセンテンスの終了位置が入力文字列の長さと一致する前提。
                // MEMO: これが崩れるパターンを検証したい場合は要注意。
                outputPositions.Last().second.Should().Be(input.Length);

                outputSentences[0].Content.Should().Be(extractedFirst);
                outputSentences[1].Content.Should().Be(extractedSecond);
            }
            else
            {
                Assert.True(false);
            }
        }

        /// <summary>SentenceExtractorでは改行を考慮してセンテンスを分割することができないことの検証テスト。</summary>
        [Fact]
        public void JapaneseSplitedSentenceTest()
        {
            // setup

            // 日本語の改行されたテキスト表現を考える。
            string input = @"サン
プル。";

            // カスタムシンボル無しのデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("ja-JP", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(1);
            outputSentences[0].Content.Should().Be(input);

            // 最後の位置は入力した文字列の末尾位置に一致する。
            outputPositions.Last().second.Should().Be(input.Length);
            outputPositions.Last().second.Should().Be(7);

            // オフセットマップは改行を考慮して計算して渡してやらないと、Sentenceコンストラクタのデフォルトでは
            // ただ単に開始行と開始位置から連続するオフセットを生成して格納するだけなので、以下のように改行は反映されない。
            outputSentences[0].OffsetMap.Count.Should().Be(7);
            outputSentences[0].OffsetMap[0].Should().Be(new LineOffset(1, 0)); // サ
            outputSentences[0].OffsetMap[1].Should().Be(new LineOffset(1, 1)); // ン
            outputSentences[0].OffsetMap[2].Should().Be(new LineOffset(1, 2)); // \r ←PlainTextParserなどでは\r\nは\nに変換される。
            outputSentences[0].OffsetMap[3].Should().Be(new LineOffset(1, 3)); // \n
            // LineOffset.MakeOffsetListでも\nを改行とみなすので、LineOffsetMapに改行のあることが反映される。
            // が、それはSentenceの正しい分割を意味しない。
            outputSentences[0].OffsetMap[4].Should().Be(new LineOffset(2, 0)); // プ
            outputSentences[0].OffsetMap[5].Should().Be(new LineOffset(2, 1)); // ル
            outputSentences[0].OffsetMap[6].Should().Be(new LineOffset(2, 2)); // 。

            outputSentences[0].Content[2].Should().Be('\r');
            outputSentences[0].Content[3].Should().Be('\n');
            outputSentences[0].Content[6].Should().Be('。');
        }
    }
}
