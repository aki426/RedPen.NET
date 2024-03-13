using System.Text.Unicode;

namespace RedPen.Net.Core.Utility
{
    /// <summary>Unicode文字種判定ユーティリティクラス。</summary>
    public static class StringUtils
    {
        /// <summary>JAVAのUnicodeBlockを模擬したユーティリティクラス。</summary>
        private class UnicodeBlock
        {
            public string Name { get; init; }
            public UnicodeRange UniCodeRange { get; init; }
            public int FirstCodePoint => this.UniCodeRange.FirstCodePoint;
            public int LastCodePoint => this.UniCodeRange.FirstCodePoint + this.UniCodeRange.Length - 1;

            public bool Of(char c)
            {
                return this.FirstCodePoint <= c && c <= this.LastCodePoint;
            }
        }

        private static readonly UnicodeBlock HIRAGANA = new UnicodeBlock { Name = "HIRAGANA", UniCodeRange = UnicodeRanges.Hiragana };
        private static readonly UnicodeBlock KATAKANA = new UnicodeBlock { Name = "KATAKANA", UniCodeRange = UnicodeRanges.Katakana };
        private static readonly UnicodeBlock CJK_UNIFIED_IDEOGRAPHS = new UnicodeBlock { Name = "CJK_UNIFIED_IDEOGRAPHS", UniCodeRange = UnicodeRanges.CjkUnifiedIdeographs };
        private static readonly UnicodeBlock HANGUL_SYLLABLES = new UnicodeBlock { Name = "HANGUL_SYLLABLES", UniCodeRange = UnicodeRanges.HangulSyllables };
        private static readonly UnicodeBlock HANGUL_JAMO = new UnicodeBlock { Name = "HANGUL_JAMO", UniCodeRange = UnicodeRanges.HangulJamo };
        private static readonly UnicodeBlock BASIC_LATIN = new UnicodeBlock { Name = "BASIC_LATIN", UniCodeRange = UnicodeRanges.BasicLatin };
        private static readonly UnicodeBlock CYRILLIC = new UnicodeBlock { Name = "CYRILLIC", UniCodeRange = UnicodeRanges.Cyrillic };

        /// <summary>
        /// 文字がひらがなのUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHiragana(char c) => HIRAGANA.Of(c);

        /// <summary>
        /// 文字がカタカナのUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsKatakana(char c) => KATAKANA.Of(c);

        /// <summary>
        /// 文字がCJK統合漢字のUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCJK(char c) => CJK_UNIFIED_IDEOGRAPHS.Of(c);

        /// <summary>
        /// 文字が日本語の文字のUnicodeブロックに含まれるかどうかを判定する。
        /// MEMO: 漢字は日本語の常用漢字とは限らない点に注意。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsProbablyJapanese(char c)
        {
            return HIRAGANA.Of(c) || KATAKANA.Of(c) || CJK_UNIFIED_IDEOGRAPHS.Of(c);
        }

        /// <summary>
        /// 文字が基本ラテン文字のUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBasicLatin(char c) => BASIC_LATIN.Of(c);

        /// <summary>
        /// 文字がキリル文字のUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsCyrillic(char c) => CYRILLIC.Of(c);

        /// <summary>
        /// 文字が韓国語の文字のUnicodeブロックに含まれるかどうかを判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsKorean(char c)
        {
            return HANGUL_SYLLABLES.Of(c) || HANGUL_JAMO.Of(c);
        }
    }
}