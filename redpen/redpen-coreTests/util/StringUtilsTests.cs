using System.Text.Unicode;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace redpen_core.util.Tests
{
    public class StringUtilsTests
    {
        private readonly ITestOutputHelper output;

        public StringUtilsTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <summary>
        /// UnicodeRangeの挙動を確認するための一時的なテスト。
        /// </summary>
        [Fact(DisplayName = "UnicodeRangeの基本動作確認", Skip = "UnicodeRange動作確認のためのTempTest")]
        public void UnicodeRangeテスト()
        {
            // 個々のUnicodeRangeはプロパティとしてアクセスできる。
            UnicodeRange hiragana = UnicodeRanges.Hiragana;
            int EndCodePoint = hiragana.FirstCodePoint + hiragana.Length - 1;
            output.WriteLine($"UnicodeRanges.Hiragana, FirstCodePoint:{hiragana.FirstCodePoint}, EndCodePoint:{EndCodePoint}, Length:{hiragana.Length}");

            // ひらがなのユニコードブロックは「ぁ」で始まり、「ゟ」で終わる。
            output.WriteLine($"ぁ, Code:{(int)'ぁ'}");
            output.WriteLine($"ゟ, Code:{(int)'ゟ'}");

            (hiragana.FirstCodePoint <= (int)'ぁ' && (int)'ぁ' <= EndCodePoint).Should().BeTrue();
            (hiragana.FirstCodePoint <= (int)'ゟ' && (int)'ゟ' <= EndCodePoint).Should().BeTrue();
            (hiragana.FirstCodePoint <= (int)'a' && (int)'a' <= EndCodePoint).Should().BeFalse();
            (hiragana.FirstCodePoint <= (int)'亜' && (int)'亜' <= EndCodePoint).Should().BeFalse();

            output.WriteLine($"---- 一覧出力 ----");

            // 定義済みのUnicodeRangeの一覧を取得する。
            IEnumerable<(string Name, UnicodeRange UnicodeRange)> EnumerateUnicodeRanges()
                => typeof(UnicodeRanges).GetProperties()
                    .Where(propInfo => propInfo.GetValue(null) is UnicodeRange)
                    .Select(propInfo => (propInfo.Name, (propInfo.GetValue(null) as UnicodeRange)!));

            // UnicodeRangeの一覧が取得できる。
            foreach (var (name, unicodeRange) in EnumerateUnicodeRanges())
            {
                output.WriteLine($"Name:{name}, Length:{unicodeRange.Length}");
            }
        }

        [Fact()]
        public void IsHiraganaTest()
        {
            StringUtils.IsHiragana('あ').Should().BeTrue();
            StringUtils.IsHiragana('ア').Should().BeFalse();
            StringUtils.IsHiragana('亜').Should().BeFalse();
            StringUtils.IsHiragana('a').Should().BeFalse();
        }

        [Fact()]
        public void IsKatakanaTest()
        {
            StringUtils.IsKatakana('あ').Should().BeFalse();
            StringUtils.IsKatakana('ア').Should().BeTrue();
            StringUtils.IsKatakana('亜').Should().BeFalse();
            StringUtils.IsKatakana('a').Should().BeFalse();
        }

        [Fact()]
        public void IsCJKTest()
        {
            StringUtils.IsCJK('あ').Should().BeFalse();
            StringUtils.IsCJK('ア').Should().BeFalse();
            StringUtils.IsCJK('亜').Should().BeTrue();
            StringUtils.IsCJK('a').Should().BeFalse();
        }

        [Fact()]
        public void IsProbablyJapaneseTest()
        {
            StringUtils.IsProbablyJapanese('あ').Should().BeTrue();
            StringUtils.IsProbablyJapanese('ア').Should().BeTrue();
            StringUtils.IsProbablyJapanese('亜').Should().BeTrue();
            StringUtils.IsProbablyJapanese('a').Should().BeFalse();
        }

        [Fact()]
        public void IsBasicLatinTest()
        {
            StringUtils.IsBasicLatin('あ').Should().BeFalse();
            StringUtils.IsBasicLatin('ア').Should().BeFalse();
            StringUtils.IsBasicLatin('亜').Should().BeFalse();
            StringUtils.IsBasicLatin('a').Should().BeTrue();
        }
    }
}
