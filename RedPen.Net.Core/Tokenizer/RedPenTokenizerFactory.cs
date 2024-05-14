using System.Globalization;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// TokenizerのFactoryパターン。
    /// </summary>
    public static class RedPenTokenizerFactory
    {
        /// <summary>
        /// Creates the tokenizer.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>An IRedPenTokenizer.</returns>
        public static IRedPenTokenizer CreateTokenizer(CultureInfo cultureInfo)
        {
            return cultureInfo switch
            {
                // 日本語の場合は、NeologdJapaneseTokenizerを返す
                CultureInfo culture when culture.Name == "ja-JP" => new KuromojiTokenizer(),
                // その他の言語の場合は、WhiteSpaceTokenizerを返す。英語をはじめとして圧倒的に半角スペースで区切る言語が多いため。
                _ => new WhiteSpaceTokenizer()
            };
        }
    }
}
