namespace RedPen.Net.Core.Config
{
    /// <summary>Symbol character model</summary>
    public record class Symbol
    {
        /// <summary>SymbolType</summary>
        public SymbolType Type { get; init; }
        /// <summary>Symbol's char value</summary>
        public char Value { get; init; }
        /// <summary>array of invalid characters</summary>
        public char[] InvalidChars { get; init; }
        /// <summary>flag to have a space before the character</summary>
        public bool NeedBeforeSpace { get; init; }
        /// <summary>flag to have a space after the character</summary>
        public bool NeedAfterSpace { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="charValue">The char value.</param>
        /// <param name="InvalidCharsStr">string containing invalid characters</param>
        /// <param name="HaveBeforeSpace">flag to have a space before the character</param>
        /// <param name="HaveAfterSpace">flag to have a space after the character</param>
        public Symbol(SymbolType symbolType,
            char charValue,
            string InvalidCharsStr = "",
            bool HaveBeforeSpace = false,
            bool HaveAfterSpace = false)
        {
            this.Type = symbolType;
            this.Value = charValue;
            this.InvalidChars = InvalidCharsStr.ToCharArray();
            this.NeedBeforeSpace = HaveBeforeSpace;
            this.NeedAfterSpace = HaveAfterSpace;
        }
    }
}
