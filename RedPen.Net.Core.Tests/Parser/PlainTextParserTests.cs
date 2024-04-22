using System;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Parser
{
    public class PlainTextParserTests
    {
        private ITestOutputHelper _output;

        public PlainTextParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly IDocumentParser _parser = new PlainTextParser(); //DocumentParser.PLAIN;

        private Document GenerateDocument(string sampleText, string lang)
        {
            Document doc = null;
            // Lang設定以外はデフォルト。
            Configuration configuration = Configuration.Builder()
                .SetLang(lang)
                .Build();

            try
            {
                doc = _parser.Parse(
                    sampleText,
                    new SentenceExtractor(configuration.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(configuration.CultureInfo));
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
            Section section = doc.GetLastSection();
            if (section == null)
            {
                Assert.False(true);
            }

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
                _output.WriteLine(item.ConvertToText());
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

            // 2行改行があるので、7センテンスが3セクションに分割される。
            section.Paragraphs.Count.Should().Be(1);
            section.Paragraphs.Sum(p => p.Sentences.Count).Should().Be(1);

            var paragraphs = section.Paragraphs;

            paragraphs[0].Sentences.Count.Should().Be(1);
            // "サンプル。"
            var sentence = paragraphs[0].Sentences[0];
            sentence.Content.Should().Be("サンプル。");
            sentence.LineNumber.Should().Be(1);
            sentence.StartPositionOffset.Should().Be(0);

            sentence.OffsetMap.Count.Should().Be(5);

            foreach (var item in sentence.OffsetMap)
            {
                _output.WriteLine(item.ToString());
            }

            // Sentenceはそれ以前の情報までは持っていないので0より小さい場合はnullを返す。
            sentence.GetOffset(-1).Should().BeNull();
            // SentenceのContentの1文字ずつが元のテキストのどこに位置しているか。
            sentence.GetOffset(0).Should().Be(new LineOffset(1, 0));
            sentence.GetOffset(1).Should().Be(new LineOffset(1, 1));
            sentence.GetOffset(2).Should().Be(new LineOffset(2, 0));
            sentence.GetOffset(3).Should().Be(new LineOffset(2, 1));
            sentence.GetOffset(4).Should().Be(new LineOffset(2, 2));
            // Sentence.Contentの範囲を超えた場合は、推定位置として最後の文字と同じ行の相当する文字位置を返す。
            // つまりこの場合LineOffset(2, 5)にはならない。
            sentence.GetOffset(5).Should().Be(new LineOffset(2, 3));
        }

        // 残りのテストケースも同様に変換
    }
}
