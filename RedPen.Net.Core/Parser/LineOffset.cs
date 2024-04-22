using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Parser
{
    public record class LineOffset : IComparable<LineOffset>
    {
        public int LineNum { get; init; }
        public int Offset { get; init; }

        public LineOffset(int lineNum, int offset)
        {
            this.LineNum = lineNum;
            this.Offset = offset;
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

            if (this.LineNum != other.LineNum)
            {
                return this.LineNum - other.LineNum;
            }

            return this.Offset - other.Offset;
        }

        public string ConvertToText()
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
