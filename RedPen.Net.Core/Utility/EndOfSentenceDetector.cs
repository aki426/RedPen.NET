using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NLog;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// センテンスの終了位置（文末）を検出するクラス。
    /// 開始位置を指定すると、最も出現位置が早いセンテンスの終わりを検出する。
    /// ピリオド（文末を表す記号概念）のパターンとホワイトワードのリストを指定することで挙動を制御できる。
    /// </summary>
    public class EndOfSentenceDetector
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        // TODO: 関数名、変数名などの命名を見直してわかりやすくする。

        /// <summary>ピリオド文字列が含まれていても文末とはみなさないホワイトワードのリスト。</summary>
        private List<string> whiteWords;

        /// <summary>ピリオド文字列のパターン。</summary>
        private Regex periodPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndOfSentenceDetector"/> class.
        /// </summary>
        /// <param name="periodPattern">The periodPattern.</param>
        public EndOfSentenceDetector(Regex periodPattern) : this(periodPattern, new List<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndOfSentenceDetector"/> class.
        /// </summary>
        /// <param name="periodPattern">The periodPattern.</param>
        /// <param name="whiteWordList">The white word list.</param>
        public EndOfSentenceDetector(Regex periodPattern, List<string> whiteWordList)
        {
            this.periodPattern = periodPattern;
            this.whiteWords = whiteWordList;
        }

        /// <summary>
        /// 位置指定のIndexが文字列長の範囲内に収まっており適切かを判定する。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool CheckPosition(int position, string str)
        {
            // positionがstrのIndexとして成立するかどうか、だけでなく末尾の1文字前までかどうかを判定する。
            // つまりpositionの取りうる値は0からstr.Length - 2までとなり末尾のstr.Length - 1の位置の文字は含まれない。
            return -1 < position && position < str.Length - 1;
        }

        // TODO: efficient computing with common prefix search.

        /// <summary>
        /// センテンスの終わりと判定されても無視するPositionの集合をあらかじめ計算する関数。
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public HashSet<int> ExtractNonEndOfSentencePositions(string inputString)
        {
            HashSet<int> nonEndOfSentencePositions = new HashSet<int>();

            // ホワイトリストに含まれる文字列は、センテンスの終わりには該当しない。
            foreach (string whiteWord in this.whiteWords)
            {
                int offset = 0;
                while (true)
                {
                    // MEMO: String.IndexOf(string, int, StringComparison)は.NET 2.0以降で使用可能。
                    // MEMO: StringComparison.Ordinalは.NET 1.1以降で使用可能。
                    // MEMO: StringComparison.Ordinalは大文字と小文字を区別する。
                    int matchStartPosition = inputString.IndexOf(whiteWord, offset, StringComparison.Ordinal);
                    if (matchStartPosition == -1)
                    {
                        // ホワイトワードが発見されなかった。
                        break;
                    }

                    // マッチしたホワイトワードの全文字位置を登録する。
                    int matchEndPosition = matchStartPosition + whiteWord.Length;
                    for (int i = matchStartPosition; i < matchEndPosition; i++)
                    {
                        nonEndOfSentencePositions.Add(i);
                    }

                    // next loop
                    offset = matchEndPosition;
                }
            }

            return nonEndOfSentencePositions;
        }

        private bool IsNonAlphabetEndOfSentenceWithPartialSentence(string str, int position, int matchPosition)
        {
            return (matchPosition == -1 && (!StringUtils.IsBasicLatin(str[position])));
        }

        private bool IsNonAlphabetWithoutSucessiveEnd(string str, int nextPosition, int matchPosition)
        {
            return matchPosition > -1 && (!StringUtils.IsBasicLatin(str[matchPosition]))
                    && matchPosition != nextPosition;
        }

        private int HandleSuccessivePeriods(string str, int position, HashSet<int> whitePositions)
        {
            int nextPosition = position + 1;
            System.Text.RegularExpressions.Match matcher = this.periodPattern.Match(str, nextPosition);
            int matchPosition = -1;
            if (matcher.Success)
            {
                matchPosition = matcher.Index;
            }

            if (IsNonAlphabetWithoutSucessiveEnd(str, nextPosition, matchPosition)
                || IsNonAlphabetEndOfSentenceWithPartialSentence(str, position, matchPosition))
            {
                // NOTE: Non Latin languages (especially Asian languages, periods do not
                // have tailing spaces in the end of sentences)
                return position;
            }

            // マッチしたのがpositionのすぐ次の位置だった場合。
            if (matchPosition == nextPosition)
            {
                // NOTE: handling of period in succession
                if ((position + 1) == str.Length - 1)
                {
                    return nextPosition;
                }
                else
                {
                    return GetEndPosition(str, nextPosition, whitePositions);
                }
            }
            else
            {
                return GetEndPosition(str, nextPosition, whitePositions);
            }
        }

        /// <summary>
        /// Match.Index ~ Match.Index + Match.Length - 1の範囲のPositionのうち、
        /// 1つでもホワイトポジションに該当するものがあればtrueを返す関数。
        /// </summary>
        /// <param name="match"></param>
        /// <param name="whitePositions"></param>
        /// <returns></returns>
        private static bool IsPositionOverWrapping(System.Text.RegularExpressions.Match match, HashSet<int> whitePositions)
        {
            // MEMO: Matchの範囲表現として、Indexは0始まりのStringのIndex。
            // Index1つ＝1文字に該当するので、match.Index + match.Lengthはマッチした部分文字列の最後の文字の次の位置を指す。
            // つまり[startPosition, endPosition)の左閉右開の半開区間である。
            int startPosition = match.Index;
            int endPosition = match.Index + match.Length;

            for (int i = startPosition; i < endPosition; i++)
            {
                // MEMO: ホワイトポジションは、あくまで文字1つ分を表すIndexの集合なので区間は意識せずともよく完全一致である。
                if (whitePositions.Contains(i))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// offset以降のPatternで指定された正規表現にマッチする部分文字列の範囲を「EndPosition」として返す関数。
        /// ただし、マッチした範囲がホワイトワードに一部でも重なっていた場合はスキップして次のマッチ範囲を返す。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="offset"></param>
        /// <param name="whitePositions"></param>
        /// <returns>マッチしなかった場合はsuccess == false。マッチ範囲は[startPosition, endPosition)の左閉右開の半開区間。</returns>
        private (bool success, int startPosition, int endPosition) GetEndPositionSkippingWhiteList(
            string str,
            int offset,
            HashSet<int> whitePositions)
        {
            var matchCollection = periodPattern.Matches(str, offset);
            if (matchCollection.Count == 0)
            {
                // マッチしなかった場合はfalseを返す。
                return (false, -1, -1);
            }

            // MEMO: patternは通常句読点だが、長さのある文字列とみなす。
            // よって検出されたデリミタとホワイトワードの突合は範囲の突合であり1文字でもPositionが一致した場合は
            // そのデリミタはセンテンスの終わりとはみなさない。
            foreach (System.Text.RegularExpressions.Match match in matchCollection)
            {
                if (IsPositionOverWrapping(match, whitePositions))
                {
                    // skip.
                    continue;
                }
                else
                {
                    // HIT
                    return (true, match.Index, match.Index + match.Length);
                }
            }

            // ホワイトワードに重ならないデリミタは見つからなかった。
            return (false, -1, -1);
        }

        /// <summary>
        /// 文末位置を検出する。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="offset"></param>
        /// <param name="whitePositions"></param>
        /// <returns>文末位置のIndex。文はピリオドなど文末を表す文字を含むためほとんどの場合ピリオド文字の位置となる。</returns>
        private int GetEndPosition(string str, int offset, HashSet<int> whitePositions)
        {
            // ホワイトワードの位置は無視して文末位置を検出する。
            var value = GetEndPositionSkippingWhiteList(str, offset, whitePositions);

            // MEMO: [startPosition, endPosition)であるため、デリミタの最後の文字位置はendPosition - 1。
            if (CheckPosition(value.endPosition - 1, str))
            {
                // BasicLatinに該当し、デリミタの最後の文字の次の文字が空白か改行の場合、デリミタの最後の位置のIndexが答え。
                // MEMO: 改行コードは\nを改行として扱っている。
                if ((StringUtils.IsBasicLatin(str[value.startPosition])
                    && (' ' == str[value.endPosition] || '\n' == str[value.endPosition])))
                {
                    return value.endPosition - 1;
                }

                return HandleSuccessivePeriods(str, value.endPosition - 1, whitePositions);
            }

            if (value.endPosition == str.Length)
            {
                // NOTE: period in end of sentence should be the end of the sentence
                // even if there is NO tailing whitespace.
                // 文字列末尾が空白や改行でなく、ピリオドが置かれていた場合はそこを文末とみます。
                return value.endPosition - 1;
            }

            // 文字列中に文末が存在しない場合は-1を返す。
            return -1;
        }

        /// <summary>
        /// Gets the sentence end position.
        /// </summary>
        /// <param name="str">The str.</param>
        /// <param name="startPosition">The start position.</param>
        /// <returns>position of full stop when there is a full stop, -1 otherwise</returns>
        public int GetSentenceEndPosition(string str, int startPosition)
        {
            HashSet<int> nonEndOfSentencePositions = ExtractNonEndOfSentencePositions(str);

            int result = str.Length;
            try
            {
                result = GetEndPosition(str, startPosition, nonEndOfSentencePositions);
            }
            catch (StackOverflowException)
            {
                LOG.Error("Catch StackOverflowException parsing :" + str + " from position: " + startPosition);
                throw;
            }
            return result;
        }
    }
}