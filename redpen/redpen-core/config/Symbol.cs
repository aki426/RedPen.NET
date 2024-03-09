namespace redpen_core.config
{
    public record class Symbol
    {
        private static readonly long serialVersionUID = 3826499136262740992L;

        public SymbolType Type { get; init; }
        public char Value { get; init; }
        public char[] InvalidChars { get; init; }
        public bool NeedBeforeSpace { get; init; }
        public bool NeedAfterSpace { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="charValue">The char value.</param>
        public Symbol(SymbolType symbolType, char charValue)
            : this(symbolType, charValue, "", false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="charValue">the character as symbol</param>
        /// <param name="invalidCharsStr">list of invalid characters</param>
        public Symbol(SymbolType symbolType, char charValue, string invalidCharsStr)
            : this(symbolType, charValue, invalidCharsStr, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="charValue">The char value.</param>
        /// <param name="invalidCharsStr">list of invalid characters</param>
        /// <param name="haveBeforeSpace">flag to have a space before the character</param>
        /// <param name="haveAfterSpace">flag to have a pace after the character</param>
        public Symbol(SymbolType symbolType,
            char charValue,
            string invalidCharsStr,
            bool haveBeforeSpace,
            bool haveAfterSpace)
        {
            this.Type = symbolType;
            this.Value = charValue;
            this.InvalidChars = invalidCharsStr.ToCharArray();
            this.NeedBeforeSpace = haveBeforeSpace;
            this.NeedAfterSpace = haveAfterSpace;
        }
    }
}
