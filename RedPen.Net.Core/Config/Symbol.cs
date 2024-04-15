using System.Text.Json.Serialization;

namespace RedPen.Net.Core.Config
{
    /// <summary>Symbol1つ分を表すrecord。Json形式での読み書きに対応する。</summary>
    public record class Symbol
    {
        /// <summary>SymbolType</summary>
        public SymbolType Type { get; init; }
        /// <summary>Symbol's char value</summary>
        public char Value { get; init; }
        /// <summary>array of invalid characters</summary>
        [JsonIgnore]
        public char[] InvalidChars { get; init; }
        /// <summary>InvalidCharsをStringにまとめたもの。Json書き出しの場合はこれがInvalidCharsの代わりになる。</summary>
        public string InvalidCharsStr => new string(InvalidChars);
        /// <summary>flag to have a space before the character</summary>
        public bool NeedBeforeSpace { get; init; }
        /// <summary>flag to have a space after the character</summary>
        public bool NeedAfterSpace { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="type">The symbol type.</param>
        /// <param name="value">The char value.</param>
        /// <param name="InvalidCharsStr">string containing invalid characters</param>
        /// <param name="NeedBeforeSpace">flag to have a space before the character</param>
        /// <param name="NeedAfterSpace">flag to have a space after the character</param>
        public Symbol(SymbolType type,
            char value,
            string InvalidCharsStr = "",
            bool NeedBeforeSpace = false,
            bool NeedAfterSpace = false)
        {
            this.Type = type;
            this.Value = value;
            this.InvalidChars = InvalidCharsStr.ToCharArray();
            this.NeedBeforeSpace = NeedBeforeSpace;
            this.NeedAfterSpace = NeedAfterSpace;
        }
    }
}
