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
using System.Collections.Immutable;
using System.Globalization;
using NLog;

namespace RedPen.Net.Core.Config
{
    public class DefaultSymbolLoader
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private static DefaultSymbolLoader instance;

        public static DefaultSymbolLoader GetInstance()
        {
            if (instance == null)
            {
                instance = new DefaultSymbolLoader();
            }

            return instance;
        }

        /// <summary>cultureInfoとvariantの設定に応じてデフォルトシンボルのDictionaryを取得する。</summary>
        public ImmutableDictionary<SymbolType, Symbol> GetSymbolDictionary(CultureInfo cultureInfo, string variant)
        {
            return GetSymbolDictionary(cultureInfo.Name, variant);
        }

        /// <summary>lang文字列とvariantの設定に応じてデフォルトシンボルのDictionaryを取得する。</summary>
        public ImmutableDictionary<SymbolType, Symbol> GetSymbolDictionary(string lang, string variant)
        {
            switch (lang)
            {
                case "ja-JP":
                    LOG.Info("\"ja-JP\" is specified.");
                    switch (variant)
                    {
                        case "hankaku":
                            LOG.Info("\"hankaku\" variant is specified");
                            return JapaneseHankakuSymbols;

                        case "zenkaku2":
                            LOG.Info("\"zenkaku2\" variant is specified");
                            return JapaneseZenkaku2Symbols;

                        default:
                            LOG.Info("\"zenkaku\" variant is specified");
                            return JapaneseSymbols;
                    }
                default:
                    LOG.Info("Default symbol settings are loaded");
                    return DefaultSymbols;
            }
        }

        /// <summary>
        /// デフォルトSymbolテーブルのReadOnlyDictionaryを生成するための関数。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>A ReadOnlyDictionary.</returns>
        private static ImmutableDictionary<SymbolType, Symbol> ConvertToSymbolDictionary(List<Symbol> list)
        {
            var builder = ImmutableDictionary.CreateBuilder<SymbolType, Symbol>();
            foreach (var symbol in list)
            {
                // すでに登録されているSymbolTypeに対しては、後勝ちで再設定する。
                if (builder.ContainsKey(symbol.Type))
                {
                    builder[symbol.Type] = symbol;
                }
                else
                {
                    builder.Add(symbol.Type, symbol);
                }
            }

            return builder.ToImmutable();
        }

        public ImmutableDictionary<SymbolType, Symbol> DefaultSymbols { get; init; }
        public ImmutableDictionary<SymbolType, Symbol> JapaneseSymbols { get; init; }
        public ImmutableDictionary<SymbolType, Symbol> JapaneseZenkaku2Symbols { get; init; }
        public ImmutableDictionary<SymbolType, Symbol> JapaneseHankakuSymbols { get; init; }

        /// <summary>
        /// Prevents a default instance of the <see cref="DefaultSymbolLoader"/> class from being created.
        /// </summary>
        private DefaultSymbolLoader()
        {
            // デフォルトシンボルテーブルをハードコード。
            DefaultSymbols = ConvertToSymbolDictionary(new List<Symbol>()
            {
                // Common symbols
                new Symbol(SymbolType.SPACE, ' ', ""),
                new Symbol(SymbolType.EXCLAMATION_MARK, '!', "！"),
                new Symbol(SymbolType.NUMBER_SIGN, '#', "＃"),
                new Symbol(SymbolType.DOLLAR_SIGN, '$', "＄"),
                new Symbol(SymbolType.PERCENT_SIGN, '%', "％"),
                new Symbol(SymbolType.QUESTION_MARK, '?', "？"),
                new Symbol(SymbolType.AMPERSAND, '&', "＆"),
                new Symbol(SymbolType.LEFT_PARENTHESIS, '(', "（", true, false),
                new Symbol(SymbolType.RIGHT_PARENTHESIS, ')', "）", false, true),
                new Symbol(SymbolType.ASTERISK, '*', "＊"),
                new Symbol(SymbolType.COMMA, ',', "，、", false, true),
                new Symbol(SymbolType.FULL_STOP, '.', "．。"),
                new Symbol(SymbolType.PLUS_SIGN, '+', "＋"),
                new Symbol(SymbolType.HYPHEN_SIGN, '-', "ー"),
                new Symbol(SymbolType.SLASH, '/', "／"),
                new Symbol(SymbolType.COLON, ':', "："),
                new Symbol(SymbolType.SEMICOLON, ';', "；"),
                new Symbol(SymbolType.LESS_THAN_SIGN, '<', "＜"),
                new Symbol(SymbolType.EQUAL_SIGN, '=', "＝"),
                new Symbol(SymbolType.GREATER_THAN_SIGN, '>', "＞"),
                new Symbol(SymbolType.AT_MARK, '@', "＠"),
                new Symbol(SymbolType.LEFT_SQUARE_BRACKET, '[', "", true, false),
                new Symbol(SymbolType.RIGHT_SQUARE_BRACKET, ']', "", false, true),
                new Symbol(SymbolType.BACKSLASH, '\\', ""),
                new Symbol(SymbolType.CIRCUMFLEX_ACCENT, '^', ""),
                new Symbol(SymbolType.LOW_LINE, '_', ""),
                new Symbol(SymbolType.LEFT_CURLY_BRACKET, '{', "｛", true, false),
                new Symbol(SymbolType.RIGHT_CURLY_BRACKET, '}', "｝", false, true),
                new Symbol(SymbolType.VERTICAL_BAR, '|', "｜"),
                new Symbol(SymbolType.TILDE, '~', "〜"),
                new Symbol(SymbolType.LEFT_SINGLE_QUOTATION_MARK, '\'', ""),
                new Symbol(SymbolType.RIGHT_SINGLE_QUOTATION_MARK, '\'', ""),
                new Symbol(SymbolType.LEFT_DOUBLE_QUOTATION_MARK, '\"', "«"),
                new Symbol(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK, '\"', "»"),

                // Digits
                new Symbol(SymbolType.DIGIT_ZERO, '0', ""),
                new Symbol(SymbolType.DIGIT_ONE, '1', ""),
                new Symbol(SymbolType.DIGIT_TWO, '2', ""),
                new Symbol(SymbolType.DIGIT_THREE, '3', ""),
                new Symbol(SymbolType.DIGIT_FOUR, '4', ""),
                new Symbol(SymbolType.DIGIT_FIVE, '5', ""),
                new Symbol(SymbolType.DIGIT_SIX, '6', ""),
                new Symbol(SymbolType.DIGIT_SEVEN, '7', ""),
                new Symbol(SymbolType.DIGIT_EIGHT, '8', ""),
                new Symbol(SymbolType.DIGIT_NINE, '9', ""),
            });

            JapaneseSymbols = ConvertToSymbolDictionary(new List<Symbol>()
            {
                // Common symbols
                new Symbol(SymbolType.SPACE, '　', ""),
                new Symbol(SymbolType.EXCLAMATION_MARK, '！', "!"),
                new Symbol(SymbolType.NUMBER_SIGN, '＃', "#"),
                new Symbol(SymbolType.DOLLAR_SIGN, '＄', "$"),
                new Symbol(SymbolType.PERCENT_SIGN, '％', ""),
                new Symbol(SymbolType.QUESTION_MARK, '？', "?"),
                new Symbol(SymbolType.AMPERSAND, '＆', ""),
                new Symbol(SymbolType.LEFT_PARENTHESIS, '（', "("),
                new Symbol(SymbolType.RIGHT_PARENTHESIS, '）', ")"),
                new Symbol(SymbolType.ASTERISK, '＊', ""), // not add "*" to invalidChars for markdown format
                new Symbol(SymbolType.COMMA, '、', ",，"),
                new Symbol(SymbolType.FULL_STOP, '。', ".．"),
                new Symbol(SymbolType.PLUS_SIGN, '＋', ""),
                new Symbol(SymbolType.HYPHEN_SIGN, 'ー', ""),
                new Symbol(SymbolType.SLASH, '／', ""),
                new Symbol(SymbolType.COLON, '：', ""),
                new Symbol(SymbolType.SEMICOLON, '；', ""),
                new Symbol(SymbolType.LESS_THAN_SIGN, '＜', ""),
                new Symbol(SymbolType.EQUAL_SIGN, '＝', ""),
                new Symbol(SymbolType.GREATER_THAN_SIGN, '＞', ""),
                new Symbol(SymbolType.AT_MARK, '＠', ""),
                new Symbol(SymbolType.LEFT_SQUARE_BRACKET, '「', ""),
                new Symbol(SymbolType.RIGHT_SQUARE_BRACKET, '」', ""),
                new Symbol(SymbolType.BACKSLASH, '¥', "\\"),
                new Symbol(SymbolType.CIRCUMFLEX_ACCENT, '＾', ""),
                new Symbol(SymbolType.LOW_LINE, '＿', ""),
                new Symbol(SymbolType.LEFT_CURLY_BRACKET, '｛', ""),
                new Symbol(SymbolType.RIGHT_CURLY_BRACKET, '｝', ""),
                new Symbol(SymbolType.VERTICAL_BAR, '｜', "|"),
                new Symbol(SymbolType.TILDE, '〜', "~"),
                new Symbol(SymbolType.LEFT_SINGLE_QUOTATION_MARK, '‘', ""),
                new Symbol(SymbolType.RIGHT_SINGLE_QUOTATION_MARK, '’', ""),
                new Symbol(SymbolType.LEFT_SINGLE_QUOTATION_MARK, '“', ""),
                new Symbol(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK, '”', ""),
                // Digits
                new Symbol(SymbolType.DIGIT_ZERO, '0', ""),
                new Symbol(SymbolType.DIGIT_ONE, '1', ""),
                new Symbol(SymbolType.DIGIT_TWO, '2', ""),
                new Symbol(SymbolType.DIGIT_THREE, '3', ""),
                new Symbol(SymbolType.DIGIT_FOUR, '4', ""),
                new Symbol(SymbolType.DIGIT_FIVE, '5', ""),
                new Symbol(SymbolType.DIGIT_SIX, '6', ""),
                new Symbol(SymbolType.DIGIT_SEVEN, '7', ""),
                new Symbol(SymbolType.DIGIT_EIGHT, '8', ""),
                new Symbol(SymbolType.DIGIT_NINE, '9', ""),
            });

            // JapaneseZenkaku2Symbolsは、JapaneseSymbolsを一部変更したもの。
            var jaZen2Builder = JapaneseSymbols.ToBuilder();
            jaZen2Builder[SymbolType.FULL_STOP] = new Symbol(SymbolType.FULL_STOP, '．', ".。");
            jaZen2Builder[SymbolType.COMMA] = new Symbol(SymbolType.COMMA, '，', ",");
            JapaneseZenkaku2Symbols = jaZen2Builder.ToImmutable();

            JapaneseHankakuSymbols = ConvertToSymbolDictionary(new List<Symbol>()
            {
                // Common symbols
                new Symbol(SymbolType.SPACE, '　', " "),
                new Symbol(SymbolType.EXCLAMATION_MARK, '!', "！"),
                new Symbol(SymbolType.NUMBER_SIGN, '#', "＃"),
                new Symbol(SymbolType.DOLLAR_SIGN, '$', "＄"),
                new Symbol(SymbolType.PERCENT_SIGN, '%', "％"),
                new Symbol(SymbolType.QUESTION_MARK, '?', "？"),
                new Symbol(SymbolType.AMPERSAND, '&', "＆"),
                new Symbol(SymbolType.LEFT_PARENTHESIS, '(', "（", true, false),
                new Symbol(SymbolType.RIGHT_PARENTHESIS, ')', "）", false, true),
                new Symbol(SymbolType.ASTERISK, '*', "＊"),
                new Symbol(SymbolType.COMMA, ',', "，、", false, true),
                new Symbol(SymbolType.FULL_STOP, '.', "．。"),
                new Symbol(SymbolType.PLUS_SIGN, '+', "＋"),
                new Symbol(SymbolType.HYPHEN_SIGN, '-', "ー"),
                new Symbol(SymbolType.SLASH, '/', "／"),
                new Symbol(SymbolType.COLON, ':', "："),
                new Symbol(SymbolType.SEMICOLON, ';', "；"),
                new Symbol(SymbolType.LESS_THAN_SIGN, '<', "＜"),
                new Symbol(SymbolType.EQUAL_SIGN, '=', "＝"),
                new Symbol(SymbolType.GREATER_THAN_SIGN, '>', "＞"),
                new Symbol(SymbolType.AT_MARK, '@', "＠"),
                new Symbol(SymbolType.LEFT_SQUARE_BRACKET, '[', "", true, false),
                new Symbol(SymbolType.RIGHT_SQUARE_BRACKET, ']', "", false, true),
                new Symbol(SymbolType.BACKSLASH, '\\', ""),
                new Symbol(SymbolType.CIRCUMFLEX_ACCENT, '^', ""),
                new Symbol(SymbolType.LOW_LINE, '_', ""),
                new Symbol(SymbolType.LEFT_CURLY_BRACKET, '{', "｛", true, false),
                new Symbol(SymbolType.RIGHT_CURLY_BRACKET, '}', "｝", false, true),
                new Symbol(SymbolType.VERTICAL_BAR, '|', "｜"),
                new Symbol(SymbolType.TILDE, '~', "〜"),
                new Symbol(SymbolType.LEFT_SINGLE_QUOTATION_MARK, '\'', ""),
                new Symbol(SymbolType.RIGHT_SINGLE_QUOTATION_MARK, '\'', ""),
                new Symbol(SymbolType.LEFT_DOUBLE_QUOTATION_MARK, '\"', ""),
                new Symbol(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK, '\"', ""),
                // Digits
                new Symbol(SymbolType.DIGIT_ZERO, '0', "０"),
                new Symbol(SymbolType.DIGIT_ONE, '1', "１"),
                new Symbol(SymbolType.DIGIT_TWO, '2', "２"),
                new Symbol(SymbolType.DIGIT_THREE, '3', "３"),
                new Symbol(SymbolType.DIGIT_FOUR, '4', "４"),
                new Symbol(SymbolType.DIGIT_FIVE, '5', "５"),
                new Symbol(SymbolType.DIGIT_SIX, '6', "６"),
                new Symbol(SymbolType.DIGIT_SEVEN, '7', "７"),
                new Symbol(SymbolType.DIGIT_EIGHT, '8', "８"),
                new Symbol(SymbolType.DIGIT_NINE, '9', "９"),
            });
        }
    }
}
