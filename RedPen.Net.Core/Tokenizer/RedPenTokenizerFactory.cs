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
