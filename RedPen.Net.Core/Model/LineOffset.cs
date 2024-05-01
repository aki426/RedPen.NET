using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    /// <summary>原文テキストにおける行番号とオフセット位置を表す。
    /// 原文テキストにおけるある文字の位置を、1始まりの行Indexと0始まるの列Indexで表現したもの、という解釈も成り立つ。</summary>
    public record LineOffset : IComparable<LineOffset>
    {
        /// <summary>行番号。1始まり。</summary>
        public int LineNum { get; init; }
        /// <summary>オフセット位置。0始まり。ある1文字の列番号と解釈してもよい。</summary>
        public int Offset { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineOffset"/> class.
        /// </summary>
        /// <param name="lineNum">The line num.</param>
        /// <param name="offset">The offset.</param>
        public LineOffset(int lineNum, int offset)
        {
            LineNum = lineNum;
            Offset = offset;
        }

        /// <summary>
        /// Compares the to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>An int.</returns>
        public int CompareTo(LineOffset? other)
        {
            if (other == null)
            {
                return 1;
            }

            if (LineNum != other.LineNum)
            {
                return LineNum - other.LineNum;
            }

            return Offset - other.Offset;
        }

        /// <summary>
        /// (L1,2)のような行＋オフセットの表示形式に変換する。
        /// </summary>
        /// <returns>A string.</returns>
        public string ConvertToShortText()
        {
            return $"(L{LineNum},{Offset})";
        }

        /// <summary>
        /// ある行に存在する文字列strに対して、開始位置を指定して、その文字列の各文字に対するLineOffsetリストを生成する。
        /// </summary>
        /// <param name="lineNum"></param>
        /// <param name="startOffset"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<LineOffset> MakeOffsetList(int lineNum, int startOffset, string str)
        {
            return Enumerable.Range(startOffset, str.Length).Select(offset => new LineOffset(lineNum, offset)).ToList();
        }
    }
}
