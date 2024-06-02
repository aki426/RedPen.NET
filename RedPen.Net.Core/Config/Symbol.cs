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
