namespace RedPen.Net.Core.Config
{
    public record class Symbol
    {
        private static readonly long serialVersionUID = 3826499136262740992L;

        public SymbolType Type { get; init; }
        public char Value { get; init; }
        public char[] InvalidChars { get; init; }
        public bool NeedBeforeSpace { get; init; }
        public bool NeedAfterSpace { get; init; }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Symbol"/> class.
        ///// </summary>
        ///// <param name="symbolType">The symbol type.</param>
        ///// <param name="charValue">The char value.</param>
        //public Symbol(SymbolType symbolType, char charValue)
        //    : this(symbolType, charValue, "", false, false)
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Symbol"/> class.
        ///// </summary>
        ///// <param name="symbolType">The symbol type.</param>
        ///// <param name="charValue">the character as symbol</param>
        ///// <param name="invalidCharsStr">list of invalid characters</param>
        //public Symbol(SymbolType symbolType, char charValue, string invalidCharsStr)
        //    : this(symbolType, charValue, invalidCharsStr, false, false)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="charValue">The char value.</param>
        /// <param name="InvalidCharsStr">list of invalid characters</param>
        /// <param name="HaveBeforeSpace">flag to have a space before the character</param>
        /// <param name="HaveAfterSpace">flag to have a pace after the character</param>
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
