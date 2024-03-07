namespace redpen_core.parser
{
    public record class LineOffset(int LineNum, int Offset) : IComparable<LineOffset>
    {
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
