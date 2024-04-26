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
    public class SentenceExtractorTests
    {
        private ITestOutputHelper output;

        public SentenceExtractorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        //private List<Sentence> CreateSentences(List<(int, int)> outputPositions, int lastPosition, string line)
        private List<Sentence> CreateSentences(List<(int, int)> outputPositions, string line)
        {
            var output = new List<Sentence>();
            foreach (var (first, second) in outputPositions)
            {
                // MEMO: OffsetMap生成のために開始位置オフセットをコンストラクタ引数で渡しておく。
                output.Add(new Sentence(line.Substring(first, second - first), 1, first));
            }
            return output;
        }

        [Fact]
        public void JapaneseSplitedSentenceTest()
        {
            // setup

            // 日本語の改行されたテキスト表現を考える。
            string input = @"サン
プル。";

            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
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
            outputSentences[0].OffsetMap[4].Should().Be(new LineOffset(1, 4)); // プ
            outputSentences[0].OffsetMap[5].Should().Be(new LineOffset(1, 5)); // ル
            outputSentences[0].OffsetMap[6].Should().Be(new LineOffset(1, 6)); // 。

            outputSentences[0].Content[2].Should().Be('\r');
            outputSentences[0].Content[3].Should().Be('\n');
            outputSentences[0].Content[6].Should().Be('。');
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
        [InlineData("005", "this is a \"pen\".", 1, "this is a \"pen\".", "")]
        // 1行でシングルクォーテーションで囲まれた単語のテキスト表現。
        [InlineData("006", "this is a \'pen\'.", 1, "this is a \'pen\'.", "")]
        // 1行でダブルクォーテーションで終わる複数テキストの表現。
        [InlineData("007", "\"this is a pen.\" Another one is not a pen.", 2, "\"this is a pen.\"", " Another one is not a pen.")]
        // 1行で部分的に分割された複数テキストの表現。
        [InlineData("008", "this is a pen. Another\n" + "one is not a pen.", 2, "this is a pen.", " Another\none is not a pen.")]
        // センテンスの終わりで改行された複数テキストの表現を考える。
        [InlineData("009", "this is a pen.\n" + "Another one is not a pen.", 2, "this is a pen.", "\nAnother one is not a pen.")]
        // センテンスの途中で改行された不十分なテキストの表現を考える。
        [InlineData("010", "this is a pen. Another\n", 1, "this is a pen.", "")]
        public void EnglishSentencesTest(string nouse1, string input, int sentenceCount, string extractedFirst, string extractedSecond)
        {
            // カスタムシンボル無しのen-USデフォルトのSymbolTableをロード。
            var extractor = new SentenceExtractor(new SymbolTable("en-US", "", new List<Symbol>()));

            // センテンスの分割位置を取得し、センテンスオブジェクトを生成する。
            List<(int first, int second)> outputPositions = extractor.Extract(input);
            List<Sentence> outputSentences = CreateSentences(outputPositions, input);

            // Assert
            outputSentences.Count.Should().Be(sentenceCount);

            // 1つしかセンテンスが抽出されない場合と2つ抽出される場合で分ける。このテストは2センテンスまでなのでそれ以外はエラー。
            if (sentenceCount == 1)
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

        //[Fact]
        //public void testJapaneseSimple()
        //{
        //    char[] stopChars = { '。', '？' };
        //    SentenceExtractor extractor = new SentenceExtractor(stopChars);
        //    final String input = "これは埼玉ですか？いいえ群馬です。";
        //    List<Pair<Integer, Integer>> outputPositions = new ArrayList<>();
        //    int lastPosition = extractor.extract(input, outputPositions);
        //    List<Sentence> outputSentences = createSentences(outputPositions, lastPosition, input);
        //    assertEquals(2, outputSentences.size());
        //    assertEquals("これは埼玉ですか？", outputSentences.get(0).getContent());
        //    assertEquals("いいえ群馬です。", outputSentences.get(1).getContent());
        //    assertEquals(input.length(), lastPosition);
        //}

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
