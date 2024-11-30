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
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Parser.Tests
{
    public class PlainTextParserTests
    {
        private ITestOutputHelper _output;

        public PlainTextParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly IDocumentParser _parser = new PlainTextParser(); //DocumentParser.PLAIN;

        public Document GenerateDocument(string sampleText, string lang)
        {
            Document doc = null;
            // Lang設定以外はデフォルト。
            //Configuration configuration = Configuration.Builder()
            //    .SetLang(lang)
            //    .Build();
            Configuration configuration = new Configuration()
            {
                DocumentLang = lang,
                Variant = "",
                ValidatorConfigurations = new List<ValidatorConfiguration>(),
                Symbols = new List<Symbol>(),
            };

            try
            {
                doc = _parser.Parse(
                    sampleText,
                    new SentenceExtractor(configuration.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(configuration.DocumentCultureInfo));
            }
            catch (Exception e)
            {
                Assert.True(false, "Exception not expected.");
            }

            return doc;
        }

        [Fact]
        public void EnglishBasicTest()
        {
            string sampleText = @"This is a pen.
That is a orange.

However,
pen is not oranges.

Happy life. Happy home. Tama Home.
";
            Document doc = GenerateDocument(sampleText, "en-US");
            if (!doc.Sections.Any())
            {
                Assert.False(true);
            }

            Section section = doc.Sections.Last();

            // 2行改行があるので、6センテンスが3セクションに分割される。
            section.Paragraphs.Count.Should().Be(3);
            section.Paragraphs.Sum(p => p.Sentences.Count).Should().Be(6);

            var paragraphs = section.Paragraphs;

            paragraphs[0].Sentences.Count.Should().Be(2);
            // "This is a pen."
            paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            paragraphs[0].Sentences[0].StartPositionOffset.Should().Be(0);
            // "That is a orange."
            paragraphs[0].Sentences[1].LineNumber.Should().Be(2);
            paragraphs[0].Sentences[1].StartPositionOffset.Should().Be(0);

            // MEMO: 複数行に分かれたセンテンスは1つのSentenceオブジェクトにまとめられる。
            paragraphs[1].Sentences.Count.Should().Be(1);
            paragraphs[1].Sentences[0].LineNumber.Should().Be(4);
            paragraphs[1].Sentences[0].StartPositionOffset.Should().Be(0);

            // MEMO: 英語の場合、センテンス内の改行は半角スペースに変換されて連結される。
            // 目視確認のため制御文字を考慮して出力。
            _output.WriteLine(ConvertToControlCharVisible(paragraphs[1].Sentences[0].Content));

            paragraphs[2].Sentences.Count.Should().Be(3);
            // "Happy life. Happy home. Tama Home."
            paragraphs[2].Sentences[0].LineNumber.Should().Be(7);
            paragraphs[2].Sentences[0].StartPositionOffset.Should().Be(0);
            paragraphs[2].Sentences[1].LineNumber.Should().Be(7);
            paragraphs[2].Sentences[1].StartPositionOffset.Should().Be(11);
            paragraphs[2].Sentences[2].LineNumber.Should().Be(7);
            paragraphs[2].Sentences[2].StartPositionOffset.Should().Be(23);

            // MEMO: Sentenceは1行内に複数存在することがあるので、LineNumberのみでSentenceの位置を特定することはできない。
            // そのため、StartPositionOffsetも利用する。
        }

        [Fact]
        public void JapaneseBasicTest()
        {
            string sampleText = @"メロスは激怒した。
必ず、かの邪智暴虐の王を除かなければならぬと決意した。

メロスには政治がわからぬ。メロスは、村の牧人である。笛を吹き、羊と遊んで暮して来た。
けれども邪悪に対しては、人一倍に敏感であった。

きょう未明メロスは村を出発し、
野を越え山越え、十里はなれた此のシラクスの市にやって来た。";

            Document doc = GenerateDocument(sampleText, "ja-JP");
            Section section = doc.GetLastSection();
            if (section == null)
            {
                Assert.False(true);
            }

            // 2行改行があるので、7センテンスが3セクションに分割される。
            section.Paragraphs.Count.Should().Be(3);
            section.Paragraphs.Sum(p => p.Sentences.Count).Should().Be(7);

            var paragraphs = section.Paragraphs;

            paragraphs[0].Sentences.Count.Should().Be(2);
            // "メロスは激怒した。"
            paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            paragraphs[0].Sentences[0].StartPositionOffset.Should().Be(0);
            // "必ず、かの邪智暴虐の王を除かなければならぬと決意した。"
            paragraphs[0].Sentences[1].LineNumber.Should().Be(2);
            paragraphs[0].Sentences[1].StartPositionOffset.Should().Be(0);

            paragraphs[1].Sentences.Count.Should().Be(4);
            // "メロスには政治がわからぬ。"
            paragraphs[1].Sentences[0].LineNumber.Should().Be(4);
            paragraphs[1].Sentences[0].StartPositionOffset.Should().Be(0);
            // "メロスは、村の牧人である。"
            paragraphs[1].Sentences[1].LineNumber.Should().Be(4);
            paragraphs[1].Sentences[1].StartPositionOffset.Should().Be(13);
            // "笛を吹き、羊と遊んで暮して来た。"
            paragraphs[1].Sentences[2].LineNumber.Should().Be(4);
            paragraphs[1].Sentences[2].StartPositionOffset.Should().Be(26);
            // "けれども邪悪に対しては、人一倍に敏感であった。"
            paragraphs[1].Sentences[3].LineNumber.Should().Be(5);
            paragraphs[1].Sentences[3].StartPositionOffset.Should().Be(0);

            // 改行無しでセンテンスが続いた場合のOffsetMapのOffsetは原文テキスト通りになっている。
            _output.WriteLine($"=> {paragraphs[1].Sentences[1].Content}");
            foreach (var item in paragraphs[1].Sentences[1].OffsetMap)
            {
                _output.WriteLine(item.ToString());
            }
            _output.WriteLine("");

            // MEMO: 複数行に分かれたセンテンスは1つのSentenceオブジェクトにまとめられる。
            // その際、本来のテキストの位置情報はOffsetMapに格納される。
            paragraphs[2].Sentences.Count.Should().Be(1);
            // きょう未明メロスは村を出発し、
            // 野を越え山越え、十里はなれた此のシラクスの市にやって来た。"
            paragraphs[2].Sentences[0].LineNumber.Should().Be(7);
            paragraphs[2].Sentences[0].StartPositionOffset.Should().Be(0);

            int contentLength = paragraphs[2].Sentences[0].Content.Count();
            int sumOfTokenSurface = paragraphs[2].Sentences[0].Tokens.Sum(t => t.Surface.Length);
            int lineOffsetIndexLength = paragraphs[2].Sentences[0].OffsetMap.Count;

            contentLength.Should().Be(44);
            sumOfTokenSurface.Should().Be(44);
            lineOffsetIndexLength.Should().Be(44);

            // MEMO: 日本語の場合、センテンス内の改行は無視されてただ連結される。
            // 目視確認のため制御文字を考慮して出力。
            _output.WriteLine(ConvertToControlCharVisible(paragraphs[2].Sentences[0].Content));

            // MEMO: TokenElementにもOffsetMapを持たせているので、それを利用して元のテキストの位置情報を取得できる。
            // 改行されていることをOffsetMapで確認できる。
            foreach (LineOffset item in paragraphs[2].Sentences[0].OffsetMap)
            {
                _output.WriteLine(item.ConvertToShortText());
            }
            // Tokenの位置を正確にOffsetMapで保持しているので、元のテキストの位置情報を取得できる。
            foreach (var item in paragraphs[2].Sentences[0].Tokens)
            {
                _output.WriteLine(item.ToString());
            }
        }

        private string ConvertToControlCharVisible(string str)
        {
            return string.Join(
                "",
                str.ToArray().Select(c =>
                {
                    if (c == '\r')
                    {
                        return "[CR]";
                    }
                    else if (c == '\n')
                    {
                        return "[LF]";
                    }
                    else if (c == ' ')
                    {
                        return "[SP]";
                    }
                    else
                    {
                        return c.ToString();
                    }
                }));
        }

        [Fact]
        public void EnglishShortMultiLineTest()
        {
            string sampleText = @"This is
a very
verylong pen.
";
            Document doc = GenerateDocument(sampleText, "en-US");
            Section section = doc.GetLastSection();
            if (section == null)
            {
                Assert.False(true);
            }

            // 3行に分割された1センテンス。
            section.Paragraphs.Count.Should().Be(1);
            section.Paragraphs.Sum(p => p.Sentences.Count).Should().Be(1);

            var sentence = section.Paragraphs[0].Sentences[0];

            // 改行を連結する際にスペースが入るので"This is a very verylong pen."となり28文字。
            sentence.Content.Length.Should().Be(28);

            sentence.Tokens[3].Surface.Should().Be("very");
            sentence.Tokens[3].OffsetMap[0].Should().Be(new LineOffset(2, 2));

            sentence.Tokens[5].Surface.Should().Be("pen");
            sentence.Tokens[5].OffsetMap[0].Should().Be(new LineOffset(3, 9));

            sentence.Tokens[6].Surface.Should().Be(".");
            sentence.Tokens[6].OffsetMap[0].Should().Be(new LineOffset(3, 12));

            // OffsetMapを目視確認。
            foreach (var item in sentence.OffsetMap)
            {
                _output.WriteLine(item.ToString());
            }

            foreach (var item in sentence.Tokens)
            {
                _output.WriteLine(item.ToString());
            }
        }

        [Fact]
        public void JapaneseShortMultiLineTest()
        {
            string sampleText = @"サン
プル。";

            Document doc = GenerateDocument(sampleText, "ja-JP");
            Section section = doc.GetLastSection();
            if (section == null)
            {
                Assert.False(true);
            }

            // 改行が連結されて1センテンスになる。
            section.Paragraphs.Count.Should().Be(1);
            section.Paragraphs.Sum(p => p.Sentences.Count).Should().Be(1);

            // "サンプル。"
            var sentence = section.Paragraphs[0].Sentences[0];

            sentence.Content.Should().Be("サンプル。");
            sentence.OffsetMap.Count.Should().Be(5);
            sentence.Tokens.Count().Should().Be(2);

            sentence.Tokens[0].Surface.Should().Be("サンプル");
            sentence.Tokens[0].OffsetMap[0].Should().Be(new LineOffset(1, 0));
            sentence.Tokens[0].OffsetMap[1].Should().Be(new LineOffset(1, 1));
            sentence.Tokens[0].OffsetMap[2].Should().Be(new LineOffset(2, 0));
            sentence.Tokens[0].OffsetMap[3].Should().Be(new LineOffset(2, 1));

            sentence.Tokens[1].Surface.Should().Be("。");
            sentence.Tokens[1].OffsetMap[0].Should().Be(new LineOffset(2, 2));

            foreach (var item in sentence.OffsetMap)
            {
                _output.WriteLine(item.ToString());
            }

            // Sentenceはそれ以前の情報までは持っていないので0より小さい場合はExceptionを投げる。
            Action act = () => sentence.ConvertToLineOffset(-1);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid index: -1 in sentence : サンプル。"); // Exception

            // SentenceのContentの1文字ずつが元のテキストのどこに位置しているか。
            sentence.ConvertToLineOffset(0).Should().Be(new LineOffset(1, 0));
            sentence.ConvertToLineOffset(1).Should().Be(new LineOffset(1, 1));
            sentence.ConvertToLineOffset(2).Should().Be(new LineOffset(2, 0));
            sentence.ConvertToLineOffset(3).Should().Be(new LineOffset(2, 1));
            sentence.ConvertToLineOffset(4).Should().Be(new LineOffset(2, 2));

            // Sentence.Contentの範囲を超えた場合はその情報を持っていないのでExceptionを投げる。
            act = () => sentence.ConvertToLineOffset(5);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Invalid index: 5 in sentence : サンプル。"); // Exception
        }

        [Fact]
        public void MultiParagraphTest()
        {
            // "1パラグラフ目
            //
            // 2パラグラフ目。
            //
            //
            // 4パラグラフ目。
            // "
            string sampleText = "1パラグラフ目\r\n\r\n2パラグラフ目。\r\n\r\n\r\n4パラグラフ目。\r\n";

            Document doc = GenerateDocument(sampleText, "ja-JP");
            doc.Sections[0].Paragraphs.Count().Should().Be(4);

            doc.Sections[0].Paragraphs[0].Sentences.Count().Should().Be(1);
            doc.Sections[0].Paragraphs[0].Sentences[0].Content.Should().Be("1パラグラフ目");
            doc.Sections[0].Paragraphs[1].Sentences.Count().Should().Be(1);
            doc.Sections[0].Paragraphs[1].Sentences[0].Content.Should().Be("2パラグラフ目。");
            // MEMO: 2パラグラフ目と4パラグラフ目の間の空行2行で空のパラグラフが1つ作成される。
            doc.Sections[0].Paragraphs[2].Sentences.Count().Should().Be(0);
            doc.Sections[0].Paragraphs[3].Sentences.Count().Should().Be(1);
            doc.Sections[0].Paragraphs[3].Sentences[0].Content.Should().Be("4パラグラフ目。");
        }

        // 残りのテストケースも同様に変換
    }
}
