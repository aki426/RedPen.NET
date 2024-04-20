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

However, pen is not oranges.
We need to be peisient.

Happy life. Happy home. Tama Home.
";
            Document doc = GenerateDocument(sampleText, "en-US");
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
            // "This is a pen."
            paragraphs[0].Sentences[0].LineNumber.Should().Be(1);
            paragraphs[0].Sentences[0].StartPositionOffset.Should().Be(0);
            // "That is a orange."
            paragraphs[0].Sentences[1].LineNumber.Should().Be(2);
            paragraphs[0].Sentences[1].StartPositionOffset.Should().Be(0);

            paragraphs[1].Sentences.Count.Should().Be(2);
            paragraphs[1].Sentences[0].LineNumber.Should().Be(4);
            paragraphs[1].Sentences[1].LineNumber.Should().Be(5);

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
野を越え山越え、十里はなれた此このシラクスの市にやって来た。";

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

            paragraphs[2].Sentences.Count.Should().Be(1);
            // きょう未明メロスは村を出発し、
            // 野を越え山越え、十里はなれた此このシラクスの市にやって来た。"
            paragraphs[2].Sentences[0].LineNumber.Should().Be(7);
            paragraphs[2].Sentences[0].StartPositionOffset.Should().Be(0);

            _output.WriteLine(paragraphs[2].Sentences[0].Content);

            var outtext = string.Join(
                "",
                paragraphs[2].Sentences[0].Content.ToArray().Select(c =>
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

            _output.WriteLine(outtext);

            foreach (var item in paragraphs[2].Sentences[0].OffsetMap)
            {
                _output.WriteLine(item.ConvertToText());
            }

            foreach (var item in paragraphs[2].Sentences[0].Tokens)
            {
                _output.WriteLine($"{item.Surface} :  {item.Reading} , {item.LineNumber}:{item.Offset}");
            }
        }

        // 残りのテストケースも同様に変換
    }
}
