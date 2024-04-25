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
                // MEMO: OffsetMap生成のために開始位置オフセットをコンストラクタ引数で渡しておく。
                output.Add(new Sentence(line.Substring(first, second - first), 0, first));
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
            var input = "this is a pen.";

            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(1);
            outputSentences[0].Content.Should().Be(input);

            // 最後の位置は入力した文字列の末尾位置に一致する。
            outputPositions.Last().second.Should().Be(input.Length);
            outputPositions.Last().second.Should().Be(14);
        }

        [Fact]
        public void TestMultipleSentences()
        {
            var input = "this is a pen. that is a paper.";

            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(2);

            outputSentences[0].Content.Should().Be("this is a pen.");
            outputSentences[1].Content.Should().Be(" that is a paper.");

            outputPositions.Last().second.Should().Be(input.Length);
            outputPositions.Last().second.Should().Be(31);
        }

        [Fact]
        public void TestTwoSentencesWithDifferentStopCharacters()
        {
            var input = "is this a pen? that is a paper.";

            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Should().HaveCount(2);

            outputSentences[0].Content.Should().Be("is this a pen?");
            outputSentences[1].Content.Should().Be(" that is a paper.");

            outputPositions.Last().second.Should().Be(input.Length);
            outputPositions.Last().second.Should().Be(31);
        }

        //        @Test
        //    void testMultipleSentencesWithoutPeriodInTheEnd()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a pen. that is a paper";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a pen.", outputSentences.get(0).getContent());
        //            assertEquals(14, lastPosition); // NOTE: second sentence start with white space.
        //        }

        //        @Test
        //    void testEndWithDoubleQuotation()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            final String input = "this is a \"pen.\"";
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a \"pen.\"", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testEndWithSingleQuotation()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a \'pen.\'";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a \'pen.\'", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testEndWithDoubleQuotationEnglishVersion()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a \"pen\".";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a \"pen\".", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testEndWithSingleQuotationEnglishVersion()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a \'pen\'.";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a \'pen\'.", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testMultipleSentencesOneOfThemIsEndWithDoubleQuotation()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a \"pen.\" Another one is not a pen.";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("this is a \"pen.\"", outputSentences.get(0).getContent());
        //            assertEquals(" Another one is not a pen.", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testMultipleSentencesWithPartialSplit()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a pen. Another\n" + "one is not a pen.";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("this is a pen.", outputSentences.get(0).getContent());
        //            assertEquals(" Another\none is not a pen.", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testMultipleSentencesWithSplitInEndOfSentence()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a pen.\nAnother one is not a pen.";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("this is a pen.", outputSentences.get(0).getContent());
        //            assertEquals("\nAnother one is not a pen.", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testMultipleSentencesWithPartialSentence()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "this is a pen. Another\n";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("this is a pen.", outputSentences.get(0).getContent());
        //            assertEquals(14, lastPosition);
        //        }

        //        @Test
        //    void testJapaneseSimple()
        //        {
        //            char[] stopChars = { '。', '？' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars);
        //            final String input = "これは埼玉ですか？いいえ群馬です。";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("これは埼玉ですか？", outputSentences.get(0).getContent());
        //            assertEquals("いいえ群馬です。", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testJapaneseSimpleWithSpace()
        //        {
        //            char[] stopChars = { '。', '？' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars);
        //            final String input = "これは埼玉ですか？ いいえ群馬です。";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("これは埼玉ですか？", outputSentences.get(0).getContent());
        //            assertEquals(" いいえ群馬です。", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testJapaneseSimpleWithEndQuotations()
        //        {
        //            char[] stopChars = { '。', '？' };
        //            char[] rightQuotations = { '’', '”' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars, rightQuotations);
        //            final String input = "これは“群馬。”";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("これは“群馬。”", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testJapaneseMultipleSentencesWithEndQuotations()
        //        {
        //            char[] stopChars = { '。', '？' };
        //            char[] rightQuotations = { '’', '”' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars, rightQuotations);
        //            final String input = "これは“群馬。”あれは群馬ではない。";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("これは“群馬。”", outputSentences.get(0).getContent());
        //            assertEquals("あれは群馬ではない。", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testJapaneseMultipleSentencesWithPartialSplit()
        //        {
        //            char[] stopChars = { '．', '？' };
        //            char[] rightQuotations = { '”', '’' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars, rightQuotations);
        //            final String input = "それは異なる．たとえば，\n" +
        //                    "以下のとおりである．";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("それは異なる．", outputSentences.get(0).getContent());
        //            assertEquals("たとえば，\n以下のとおりである．", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testJapanesSentenceWithEndWithNonFullStop()
        //        {
        //            char[] stopChars = { '．' };
        //            char[] rightQuotations = { '’', '”' };
        //            SentenceExtractor extractor = new SentenceExtractor(stopChars, rightQuotations);
        //            final String input = "それは異なる．たとえば，";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("それは異なる．", outputSentences.get(0).getContent());
        //            assertEquals(7, lastPosition);
        //        }

        //        @Test
        //    void testSentenceWithWhiteWordPosition()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "He is a Dr. candidate.";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals("He is a Dr. candidate.", outputSentences.get(0).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testMultipleSentencesWithWhiteWordPosition()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor
        //                    (Configuration.builder().build().getSymbolTable());
        //            final String input = "Is he a Dr. candidate? Yes, he is.";  // NOTE: white word list contains "Dr."
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(2, outputSentences.size());
        //            assertEquals("Is he a Dr. candidate?", outputSentences.get(0).getContent());
        //            assertEquals(" Yes, he is.", outputSentences.get(1).getContent());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testVoidLine()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = "";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(0, outputSentences.size());
        //            assertEquals(0, lastPosition); // NOTE: second sentence start with white space.
        //        }

        //        @Test
        //    void testJustPeriodLine()
        //        {
        //            SentenceExtractor extractor = new SentenceExtractor(
        //                    Configuration.builder().build().getSymbolTable());
        //            final String input = ".";
        //            List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //            int lastPosition = extractor.extract(input, outputPositions);
        //            List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //            assertEquals(1, outputSentences.size());
        //            assertEquals(input.length(), lastPosition);
        //        }

        //        @Test
        //    void testConstructPatternStringWithoutEscape()
        //        {
        //            char[] endCharacters = { '.', '?', '!' };
        //            SentenceExtractor extractor = new SentenceExtractor(endCharacters);
        //            assertEquals("[\\.\\?\\!][\'\"]?", extractor.constructEndSentencePattern().pattern());
        //        }

        //        @Test
        //    void testThrowExceptionGivenVoidList()
        //        {
        //            assertThrows(IllegalArgumentException.class, () -> {
        //            char[] endCharacters = { };
        //        SentenceExtractor extractor = new SentenceExtractor(endCharacters);
        //        extractor.constructEndSentencePattern();
        //        });
        //    }

        //@Test
        //    void testThrowExceptionGivenNull() {
        //        SentenceExtractor extractor = new SentenceExtractor(Configuration.builder().build().getSymbolTable());
        //extractor.constructEndSentencePattern(); // not a throw exception
        //    }
    }
}
