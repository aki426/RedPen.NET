using System;

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
    }
}
