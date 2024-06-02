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

using System;
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
        private Regex _fullStopPattern;

        /// <summary>Full stop = 句点を表す文字</summary>
        private char[] _fullStopList;

        /// <summary>Right quotation = 右引用符を表す文字</summary>
        private char[] _rightQuotationList;

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
        private EndOfSentenceDetector _endOfSentenceDetector;

        /// <summary>SymbolTable</summary>
        private SymbolTable? _symbolTable = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceExtractor"/> class.
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        public SentenceExtractor(SymbolTable symbolTable)
            : this(GetPeriodsFrom(symbolTable), GetRightQuotationsFrom(symbolTable))
        {
            this._symbolTable = symbolTable;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SentenceExtractor"/> class from being created.
        /// </summary>
        /// <param name="fullStopList">The full stop list.</param>
        /// <param name="rightQuotationList">The right quotation list.</param>
        private SentenceExtractor(char[] fullStopList, char[] rightQuotationList)
        {
            this._fullStopList = fullStopList;
            this._rightQuotationList = rightQuotationList;

            this._fullStopPattern = this.ConstructEndSentencePattern();
            this._endOfSentenceDetector = new EndOfSentenceDetector(this._fullStopPattern, WHITE_WORDS);
        }

        /// <summary>
        /// SymbolTableの定義からピリオドに類する文字を取得する関数。
        /// </summary>
        /// <param name="symbolTable">The symbol table.</param>
        /// <returns>An array of char.</returns>
        private static char[] GetPeriodsFrom(SymbolTable symbolTable)
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
        private static char[] GetRightQuotationsFrom(SymbolTable symbolTable)
        {
            char[] rightQuotations = new char[]{
                // MEMO: 日本語では「’」と「”」が該当。ただしこの記号で文を区切る意識がある日本語話者は少ないはず。
                symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_SINGLE_QUOTATION_MARK),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.RIGHT_DOUBLE_QUOTATION_MARK)
            };
            LOG.Info("\"" + rightQuotations.ToString() + "\" are added as a right quotation characters");
            return rightQuotations;
        }

        /// <summary>
        /// エスケープされた文字のグループパターンを生成します。
        /// </summary>
        /// <param name="endCharacters">The end characters.</param>
        /// <returns>A string.</returns>
        private static string ConvertToEscapedCharGroupPattern(char[] endCharacters)
        {
            StringBuilder patternString = new StringBuilder();

            patternString.Append("[");
            foreach (char endChar in endCharacters)
            {
                patternString.Append(Regex.Escape(endChar.ToString()));
            }
            patternString.Append("]");

            return patternString.ToString();
        }

        /// <summary>
        /// fullStopListとrightQuotationListから、文末を検出するための正規表現を構築します。
        /// NOTE: 正規表現としては「[終端文字][右クォーテーション]?」となります。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private Regex ConstructEndSentencePattern()
        {
            if (this._fullStopList == null || this._fullStopList.Length == 0)
            {
                throw new ArgumentNullException("No end character is specified");
            }

            StringBuilder patternString = new StringBuilder();

            patternString.Append(ConvertToEscapedCharGroupPattern(this._fullStopList));
            patternString.Append(ConvertToEscapedCharGroupPattern(this._rightQuotationList));
            patternString.Append("?");

            return new Regex(patternString.ToString()); ;
        }

        // TODO: Extractメソッドの戻り値のsecondは、終了文字のオフセット+1を返している。
        // テストケースでしか使われていないが、文字位置指定の一貫性を保つためにsecondを終了文字のオフセットそのものに見直す検討をする。

        /// <summary>
        /// Sentenceのリストを返します。
        /// 結果のリストはセンテンスの開始位置と終了位置+1のタプルのリストです。
        /// </summary>
        /// <param name="line">Input line which can contain more than one sentences.</param>
        /// <returns>List of extracted sentences</returns>
        public List<(int first, int second)> Extract(string line)
        {
            List<(int first, int second)> sentencePositions = new List<(int first, int second)>();

            int startPosition = 0;
            // センテンスのちょうど終わりの文字の位置を返す。センテンスはその文字（ピリオドなど）位置を含む。
            int periodPosition = _endOfSentenceDetector.GetSentenceEndPosition(line, 0);

            // センテンスの終了位置が不正でなければ結果に詰め込んでいく。
            while (0 <= periodPosition)
            {
                // 結果には、センテンスの開始位置と終了位置+1を詰め込む。つまり左閉右開区間となる。
                // TODO: 現在はfirstはそのセンテンスの開始位置だが、secondeは次のセンテンスの開始位置おなっておりこれで良いかどうか検討が必要。
                sentencePositions.Add(new(startPosition, periodPosition + 1));

                // iteration.
                startPosition = periodPosition + 1;
                periodPosition = _endOfSentenceDetector.GetSentenceEndPosition(line, startPosition);
            }

            return sentencePositions;
        }

        /// <summary>
        /// Given string, return sentence end position.
        /// </summary>
        /// <param name="str">input string</param>
        /// <returns>position of full stop when there is a full stop, -1 otherwise</returns>
        public int GetSentenceEndPosition(string str)
        {
            return this._endOfSentenceDetector.GetSentenceEndPosition(str, 0);
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
            return (_symbolTable != null) && (_symbolTable.Lang == "ja-JP") ? "" : " ";
        }
    }
}
