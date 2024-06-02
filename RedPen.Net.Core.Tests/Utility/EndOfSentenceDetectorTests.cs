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
using System.Text.RegularExpressions;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Utility
{
    public class EndOfSentenceDetectorTests
    {
        private readonly ITestOutputHelper output;

        public EndOfSentenceDetectorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact()]
        public void GetSentenceEndPositionTest()
        {
            // Regex.Escape()に関する基本的なテスト。
            output.WriteLine($"escaped string: {Regex.Escape(".")}");
            Regex.Escape(".").Should().Be("\\.");

            var pattern = new Regex(Regex.Escape("."));
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // 通常のセンテンスの末尾を検出する。
            endOfSentenceDetector.GetSentenceEndPosition("", 0).Should().Be(-1);
            endOfSentenceDetector.GetSentenceEndPosition(".", 0).Should().Be(0); // '.'
            endOfSentenceDetector.GetSentenceEndPosition("A.", 0).Should().Be(1); // '.'
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen.", 0).Should().Be(13); // '.'

            // 末尾に空白があるパターン。
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen. ", 0).Should().Be(13); // 空白の直前の'.'

            // ...があり、さらに空白があるパターンと無いパターン。
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen... ", 0).Should().Be(15); // 空白の直前の'.'
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen...", 0).Should().Be(15);
        }

        [Fact()]
        public void GetSentenceEndPositionWithMultiSentenceTest()
        {
            var pattern = new Regex(Regex.Escape("."));
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // センテンスが複数ある場合にStartPositionを指定して検出する。
            endOfSentenceDetector.GetSentenceEndPosition("Right. That is not a pen.", 0).Should().Be(5);
            endOfSentenceDetector.GetSentenceEndPosition("Right. That is not a pen.", 6).Should().Be(24);

            // より実際に近いパターン。
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen. that is not pen.", 0).Should().Be(13);

            // 複数行
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen.\nthat is not pen.", 0).Should().Be(13);

            // PartialSentence（途中で切れたセンテンス）
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen. that is not", 0).Should().Be(13);

            // ...の後にさらに続くパターン。
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen... But that is a pencil.", 0).Should().Be(15);
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen...But that is a pencil.", 0).Should().Be(36);
            // 末尾に空白があるパターン。
            endOfSentenceDetector.GetSentenceEndPosition("this is a pen...But that is a pencil. ", 0).Should().Be(36);
        }

        [Fact]
        public void GetSentenceEndPositionWithMultiSymbolTest()
        {
            var pattern = new Regex("\\?|\\.");
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // 複数シンボル。
            endOfSentenceDetector.GetSentenceEndPosition("is this a pen? yes it is.", 0).Should().Be(13);
        }

        [Fact]
        public void GetSentenceEndPositionWithWhiteWordTest()
        {
            var pattern = new Regex("\\.");
            EndOfSentenceDetector endOfSentenceDetector;

            // ホワイトワード
            endOfSentenceDetector = new EndOfSentenceDetector(pattern, new List<string>() { "Mr." });
            endOfSentenceDetector.GetSentenceEndPosition("He is Mr. United States.", 0).Should().Be(23);
            // ホワイトワードを含み末尾にピリオドがないパターン。
            endOfSentenceDetector.GetSentenceEndPosition("He is Mr. United States", 0).Should().Be(-1);
            // ピリオドが無く、かつ末尾が改行のパターン。
            endOfSentenceDetector.GetSentenceEndPosition("He is Mr. United States\n", 0).Should().Be(-1);

            // 複数ホワイトワード
            endOfSentenceDetector = new EndOfSentenceDetector(pattern, new List<string>() { "Mr.", "Jun." });
            endOfSentenceDetector.GetSentenceEndPosition("This Jun. 10th, he was Mr. United States.", 0).Should().Be(40);

            // ホワイトワードにピリオドが含まれるパターン。
            endOfSentenceDetector = new EndOfSentenceDetector(pattern, new List<string>() { "a.m." });
            endOfSentenceDetector.GetSentenceEndPosition("At 10 a.m. we had a lunch.", 0).Should().Be(25);
        }

        [Fact]
        public void GetSentenceEndPositionWithQuotationMarkTest()
        {
            var pattern = new Regex("\\.\"");
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            endOfSentenceDetector.GetSentenceEndPosition("\"pen.\"", 0).Should().Be(5);
        }

        [Fact]
        public void 日本語文のセンテンス末尾テスト()
        {
            var pattern = new Regex("。");
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // 日本語のセンテンス末尾を検出する。
            endOfSentenceDetector.GetSentenceEndPosition("私はペンではない。私は人間です。", 0).Should().Be(8);

            // 空白交じり
            endOfSentenceDetector.GetSentenceEndPosition("私はペンではない。 私は人間です。", 0).Should().Be(8);

            // 文末が存在しないパターン。
            endOfSentenceDetector.GetSentenceEndPosition("私はペンでは", 0).Should().Be(-1);
        }

        [Fact]
        public void 日本語文の複数センテンス末尾テスト()
        {
            var pattern = new Regex("。");
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // 複数センテンス。
            endOfSentenceDetector.GetSentenceEndPosition("これは。。。 ペンですか。", 0).Should().Be(5);

            endOfSentenceDetector.GetSentenceEndPosition("異なる。たとえば，", 0).Should().Be(3);
            endOfSentenceDetector.GetSentenceEndPosition("異なる。\nたとえば，", 0).Should().Be(3);
        }

        [Fact]
        public void 日本語文の複数シンボルテスト()
        {
            var pattern = new Regex("。|？");
            var endOfSentenceDetector = new EndOfSentenceDetector(pattern);

            // 複数シンボル。
            endOfSentenceDetector.GetSentenceEndPosition("これは群馬ですか？いいえ埼玉です。", 0).Should().Be(8);
        }

        [Fact()]
        public void ホワイトワード位置検出のテスト()
        {
            var pattern = new Regex("\\.");
            EndOfSentenceDetector endOfSentenceDetector;

            endOfSentenceDetector = new EndOfSentenceDetector(pattern, new List<string>() { "Mr.", "Jun." });
            HashSet<int> hashSet = endOfSentenceDetector.ExtractNonEndOfSentencePositions("a Mr. white at Jun. in 2024.");

            hashSet.Should().BeEquivalentTo(new HashSet<int>() { 2, 3, 4, 15, 16, 17, 18 });
        }
    }
}
