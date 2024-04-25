using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using Xunit;

namespace RedPen.Net.Core.Tests.Parser
{
    public class SentenceExtractorTests
    {
        //private List<Sentence> CreateSentences(List<(int, int)> outputPositions, int lastPosition, string line)
        private List<Sentence> CreateSentences(List<(int, int)> outputPositions, string line)
        {
            var output = new List<Sentence>();
            foreach (var (first, second) in outputPositions)
            {
                output.Add(new Sentence(line.Substring(first, second - first), 0));
            }
            return output;
        }

        [Fact]
        public void GetOffsetTest()
        {
            // setup

            // 日本語の改行されたテキスト表現を考える。
            string origin = @"サン
プル。";

            // 日本語テキストの場合改行は単に連結される。
            var sentence = new Sentence("サンプル。", 1, 0);
            sentence.OffsetMap.Add(new LineOffset(1, 0)); // サ
            sentence.OffsetMap.Add(new LineOffset(1, 1)); // ン
            sentence.OffsetMap.Add(new LineOffset(2, 0)); // プ
            sentence.OffsetMap.Add(new LineOffset(2, 1)); // ル
            sentence.OffsetMap.Add(new LineOffset(2, 2)); // 。
        }

        [Fact]
        public void TestSimple()
        {
            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));
            //var outputPositions = new List<(int, int)>();
            var input = "this is a pen.";
            //int lastPosition = extractor.Extract(input, outputPositions);
            List<(int first, int second)> outputPositions = extractor.Extract(input);

            List<Sentence> outputsentences = CreateSentences(outputPositions, input);

            outputsentences.Count.Should().Be(1);
            outputsentences[0].Content.Should().Be(input);

            outputPositions.Last().second.Should().Be(input.Length);
            outputPositions.Last().second.Should().Be(14);
            //lastPosition.Should().Be(14);
        }

        [Fact]
        public void TestMultipleSentences()
        {
            //var extractor = new SentenceExtractor(Configuration.Builder().Build().SymbolTable);
            //var input = "this is a pen. that is a paper.";
            //var outputPositions = new List<(int, int)>();
            //var lastPosition = extractor.Extract(input, outputPositions);
            //var outputSentences = CreateSentences(outputPositions, lastPosition, input);
            //outputSentences.Should().HaveCount(2);
            //outputSentences[0].Content.Should().Be("this is a pen.");
            //outputSentences[1].Content.Should().Be(" that is a paper.");
            //lastPosition.Should().Be(31);
        }

        [Fact]
        public void TestTwoSentencesWithDifferentStopCharacters()
        {
            //var extractor = new SentenceExtractor(Configuration.Builder().Build().SymbolTable);
            //var input = "is this a pen? that is a paper.";
            //var outputPositions = new List<(int, int)>();
            //var lastPosition = extractor.Extract(input, outputPositions);
            //var outputSentences = CreateSentences(outputPositions, lastPosition, input);
            //outputSentences.Should().HaveCount(2);
            //outputSentences[0].Content.Should().Be("is this a pen?");
            //outputSentences[1].Content.Should().Be(" that is a paper.");
            //lastPosition.Should().Be(31);
        }
    }
}
