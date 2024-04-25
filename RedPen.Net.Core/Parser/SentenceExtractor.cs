﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Parser
{
    /// <summary>テキストから文を抽出するクラスです。</summary>
    public class SentenceExtractor
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>Full stop = 句点を表すパターン。</summary>
        private Regex fullStopPattern;

        /// <summary>Full stop = 句点を表す文字</summary>
        private char[] fullStopList;

        /// <summary>Right quotation = 右引用符を表す文字</summary>
        private char[] rightQuotationList;

        // TODO: make white words configurable.

        /// <summary>ピリオドが含まれていても無視するホワイトワード</summary>
        private static readonly List<string> WHITE_WORDS = new List<string>(){
            "Mr.", "Mrs.", "Miss.", "Dr.",
            "genn.ai", "Co., Ltd.",
            "a.m.", "p.m.",
            "U.S.A.",
            "Jan.", "Feb.", "Mar.", "Apr.",
            "May.", "Jun.", "Jul.", "Aug.",
            "Sep.", "Oct.", "Nov.", "Dec.", "Feb.",
            "B.C", "A.D." };

        /// <summary>EndOfSentenceDetector</summary>
        private EndOfSentenceDetector endOfSentenceDetector;

        /// <summary>SymbolTable</summary>
        private SymbolTable? symbolTable = null;

        // MEMO: 未使用につきコメントアウト。

        ///// <summary>
        ///// Prevents a default instance of the <see cref="SentenceExtractor"/> class from being created.
        ///// </summary>
        ///// <param name="fullStopList">fullStopList set of end of sentence characters</param>
        //private SentenceExtractor(char[] fullStopList)
        //    : this(fullStopList, extractRightQuotations(Configuration.Builder().Build().SymbolTable))
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceExtractor"/> class.
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        public SentenceExtractor(SymbolTable symbolTable)
            : this(extractPeriods(symbolTable), extractRightQuotations(symbolTable))
        {
            this.symbolTable = symbolTable;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SentenceExtractor"/> class from being created.
        /// </summary>
        /// <param name="fullStopList">The full stop list.</param>
        /// <param name="rightQuotationList">The right quotation list.</param>
        private SentenceExtractor(char[] fullStopList, char[] rightQuotationList)
        {
            this.fullStopList = fullStopList;
            this.rightQuotationList = rightQuotationList;

            this.fullStopPattern = this.constructEndSentencePattern();
            this.endOfSentenceDetector = new EndOfSentenceDetector(this.fullStopPattern, WHITE_WORDS);
        }

        /// <summary>
        /// SymbolTableの定義からピリオドに類する文字を取得する関数。
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        /// <returns>An array of char.</returns>
        private static char[] extractPeriods(SymbolTable symbolTable)
        {
            char[] periods = new char[]{
                // MEMO: 日本語では「。」「？」「！」の3種類が該当。
                symbolTable.GetValueOrFallbackToDefault(SymbolType.FULL_STOP),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.QUESTION_MARK),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.EXCLAMATION_MARK)
            };
            LOG.Info("\"" + periods.ToString() + "\" are added as a end of sentence characters");
            return periods;
        }

        /// <summary>
        /// SymbolTableの定義から右クォーテーションマークに類する文字を取得する関数。
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        /// <returns>An array of char.</returns>
        private static char[] extractRightQuotations(SymbolTable symbolTable)
        {
            char[] rightQuotations = new char[]{
                // MEMO: 日本語では「’」と「”」が該当。ただしこの記号で文を区切る意識がある日本語話者は少ないはず。
                symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_SINGLE_QUOTATION_MARK),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK)
            };
            LOG.Info("\"" + rightQuotations.ToString() + "\" are added as a right quotation characters");
            return rightQuotations;
        }

        // MEMO: .NETでは正規表現のエスケープはRegex.Escape()で行う。
        //private static string handleSpecialCharacter(char endChar)
        //{
        //    if (endChar == '.')
        //    {
        //        return "\\.";
        //    }
        //    else if (endChar == '?')
        //    {
        //        return "\\?";
        //    }
        //    else if (endChar == '!')
        //    {
        //        return "\\!";
        //    }
        //    else
        //    {
        //        return endChar.ToString();
        //    }
        //}

        private void generateSimplePattern(char[] endCharacters, StringBuilder patternString)
        {
            patternString.Append("[");
            foreach (char endChar in endCharacters)
            {
                //patternString.Append(handleSpecialCharacter(endChar));
                patternString.Append(Regex.Escape(endChar.ToString()));
            }
            patternString.Append("]");
        }

        /// <summary>
        /// fullStopListとrightQuotationListから、文末を検出するための正規表現を構築します。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private Regex constructEndSentencePattern()
        {
            if (this.fullStopList == null || this.fullStopList.Length == 0)
            {
                throw new ArgumentNullException("No end character is specified");
            }

            StringBuilder patternString = new StringBuilder();
            generateSimplePattern(this.fullStopList, patternString);
            patternString.Append("[");
            foreach (char rightQuotation in rightQuotationList)
            {
                patternString.Append(rightQuotation);
            }
            patternString.Append("]?");

            return new Regex(patternString.ToString()); ;
        }

        //    /**
        //     * Get Sentence lists.
        //     *
        //     * @param line              Input line which can contain more than one sentences
        //     * @param sentencePositions List of extracted sentences
        //     * @return remaining line
        //     */
        //public int Extract(string line, List<(int first, int second)> sentencePositions)
        //{
        //    int startPosition = 0;
        //    // センテンスのちょうど終わりの文字の位置を返す。センテンスはその文字位置を含む。
        //    int periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, 0);

        //    // センテンスの終了位置が不正でなければ結果に詰め込んでいく。
        //    while (0 <= periodPosition)
        //    {
        //        // 結果には、センテンスの開始位置と終了位置+1を詰め込む。つまり左閉右開区間となる。
        //        sentencePositions.Add(new(startPosition, periodPosition + 1));

        //        // iteration.
        //        startPosition = periodPosition + 1;
        //        periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, startPosition);
        //    }

        //    return startPosition;
        //}

        public List<(int first, int second)> Extract(string line)
        {
            List<(int first, int second)> sentencePositions = new List<(int first, int second)>();

            int startPosition = 0;
            // センテンスのちょうど終わりの文字の位置を返す。センテンスはその文字位置を含む。
            int periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, 0);

            // センテンスの終了位置が不正でなければ結果に詰め込んでいく。
            while (0 <= periodPosition)
            {
                // 結果には、センテンスの開始位置と終了位置+1を詰め込む。つまり左閉右開区間となる。
                sentencePositions.Add(new(startPosition, periodPosition + 1));

                // iteration.
                startPosition = periodPosition + 1;
                periodPosition = endOfSentenceDetector.GetSentenceEndPosition(line, startPosition);
            }

            return sentencePositions;
        }

        //    /**
        //     * Given string, return sentence end position.
        //     *
        //     * @param str input string
        //     * @return position of full stop when there is a full stop, -1 otherwise
        //     */
        public int GetSentenceEndPosition(string str)
        {
            return this.endOfSentenceDetector.GetSentenceEndPosition(str, 0);
        }

        /// <summary>
        /// 改行で分割されたテキストを再結合するための文字列を返します。
        /// １センテンスであるべき文字列が改行で分割された場合にそれを連結することを想定しています。
        /// 日本語の場合：空文字列
        /// 英語などそれ以外の言語の場合：半角空白
        /// TODO: The specification of this string should be moved to part of the configuration
        /// </summary>
        /// <returns>A string.</returns>
        public string getBrokenLineSeparator()
        {
            // 日本語設定のみのハードコーディングだが、日本語は空文字列で
            // それ以外の言語は半角空白をBlokenLineSeparatorとする、という2択で良いのか疑問が残る。
            // TODO: 多言語対応の際には、このメソッドの実装を見直す。
            return (symbolTable != null) && (symbolTable.Lang == "ja-JP") ? "" : " ";
        }
    }
}
