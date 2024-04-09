using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedPen.Net.Core.Config
{
    public partial class SymbolTable
    {
        /// <summary>
        /// デフォルトSymbolテーブルのReadOnlyDictionaryを生成するための関数。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>A ReadOnlyDictionary.</returns>
        private static ReadOnlyDictionary<SymbolType, Symbol> InitSymbolDictionary(List<Symbol> list)
        {
            var temp = new Dictionary<SymbolType, Symbol>();
            foreach (var symbol in list)
            {
                // Java版はTryAddがあったが、C#10.0は無いので、ContainsKeyでチェックしてからAddする。
                if (!temp.ContainsKey(symbol.Type))
                {
                    temp.Add(symbol.Type, symbol);
                }
                else
                {
                    temp[symbol.Type] = symbol;
                }
            }
            return new ReadOnlyDictionary<SymbolType, Symbol>(temp);
        }

        private static readonly ReadOnlyDictionary<SymbolType, Symbol> DEFAULT_SYMBOLS =
            InitSymbolDictionary(new List<Symbol>()
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

        //private static readonly ReadOnlyDictionary<SymbolType, Symbol> RUSSIAN_SYMBOLS =
        //    new ReadOnlyDictionary<SymbolType, Symbol>(DEFAULT_SYMBOLS);

        private static readonly ReadOnlyDictionary<SymbolType, Symbol> JAPANESE_SYMBOLS =
            InitSymbolDictionary(new List<Symbol>()
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
                new Symbol(SymbolType.FULL_STOP, '。', "．"),
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

        private static ReadOnlyDictionary<SymbolType, Symbol> InitJaZenkaku2()
        {
            // new Dictionary<SymbolType, Symbol>
            Dictionary<SymbolType, Symbol> tempDict = new Dictionary<SymbolType, Symbol>(JAPANESE_SYMBOLS);
            tempDict[SymbolType.FULL_STOP] = new Symbol(SymbolType.FULL_STOP, '．', "。");
            tempDict[SymbolType.COMMA] = new Symbol(SymbolType.COMMA, '，', ",");

            return new ReadOnlyDictionary<SymbolType, Symbol>(tempDict);
        }

        private static readonly ReadOnlyDictionary<SymbolType, Symbol> JAPANESE_ZENKAKU2_SYMBOLS =
            InitJaZenkaku2();

        private static readonly ReadOnlyDictionary<SymbolType, Symbol> JAPANESE_HANKAKU_SYMBOLS =
            InitSymbolDictionary(new List<Symbol>()
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
