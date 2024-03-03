using FluentAssertions;
using Xunit;

namespace redpen_core.tokenizer.Tests
{
    public class NeologdJapaneseTokenizerTests
    {
        /// <summary>
        /// Tokenizes the test.
        /// </summary>
        [Fact(DisplayName = "Kuromoji＋Neologdの基本的なTokenizeテスト")]
        public void TokenizeTest()
        {
            NeologdJapaneseTokenizer tokenizer = new NeologdJapaneseTokenizer();
            List<TokenElement> tokens = tokenizer.Tokenize("今日も晴天だ。");

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
    }
}