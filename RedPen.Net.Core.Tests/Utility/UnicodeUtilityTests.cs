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
using System.Linq;
using System.Text.Unicode;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Utility
{
    public class UnicodeUtilityTests
    {
        private readonly ITestOutputHelper output;

        public UnicodeUtilityTests(ITestOutputHelper output)
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
                    .Select(propInfo => (propInfo.Name, (propInfo.GetValue(null) as UnicodeRange)));

            // UnicodeRangeの一覧が取得できる。
            foreach (var (name, unicodeRange) in EnumerateUnicodeRanges())
            {
                output.WriteLine($"Name:{name}, Length:{unicodeRange.Length}");
            }
        }

        [Fact()]
        public void IsHiraganaTest()
        {
            UnicodeUtility.IsHiragana('あ').Should().BeTrue();
            UnicodeUtility.IsHiragana('ア').Should().BeFalse();
            UnicodeUtility.IsHiragana('亜').Should().BeFalse();
            UnicodeUtility.IsHiragana('a').Should().BeFalse();
        }

        [Fact()]
        public void IsKatakanaTest()
        {
            UnicodeUtility.IsKatakana('あ').Should().BeFalse();
            UnicodeUtility.IsKatakana('ア').Should().BeTrue();
            UnicodeUtility.IsKatakana('亜').Should().BeFalse();
            UnicodeUtility.IsKatakana('a').Should().BeFalse();
        }

        [Fact()]
        public void IsCJKTest()
        {
            UnicodeUtility.IsCJK('あ').Should().BeFalse();
            UnicodeUtility.IsCJK('ア').Should().BeFalse();
            UnicodeUtility.IsCJK('亜').Should().BeTrue();
            UnicodeUtility.IsCJK('a').Should().BeFalse();
        }

        [Fact()]
        public void IsProbablyJapaneseTest()
        {
            UnicodeUtility.IsProbablyJapanese('あ').Should().BeTrue();
            UnicodeUtility.IsProbablyJapanese('ア').Should().BeTrue();
            UnicodeUtility.IsProbablyJapanese('亜').Should().BeTrue();
            UnicodeUtility.IsProbablyJapanese('a').Should().BeFalse();
        }

        [Fact()]
        public void IsBasicLatinTest()
        {
            UnicodeUtility.IsBasicLatin('あ').Should().BeFalse();
            UnicodeUtility.IsBasicLatin('ア').Should().BeFalse();
            UnicodeUtility.IsBasicLatin('亜').Should().BeFalse();
            UnicodeUtility.IsBasicLatin('a').Should().BeTrue();
        }
    }
}
