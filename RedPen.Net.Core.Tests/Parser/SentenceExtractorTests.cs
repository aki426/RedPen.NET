using System;
using System.Collections.Generic;
using FluentAssertions;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using Xunit;

namespace RedPen.Net.Core.Tests.Parser
{
    public class SentenceExtractorTests
    {
        private List<Sentence> CreateSentences(List<(int, int)> outputPositions, int lastPosition, string line)
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
            //var extractor = new SentenceExtractor(Configuration.Builder().Build().SymbolTable);
            //var outputPositions = new List<(int, int)>();
            //var input = "this is a pen.";
            //var lastPosition = extractor.Extract(input, outputPositions);
            //var outputSentences = CreateSentences(outputPositions, lastPosition, input);
            //outputSentences.Should().HaveCount(1);
            //outputSentences[0].Content.Should().Be(input);
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
