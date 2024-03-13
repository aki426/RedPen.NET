using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// ImmutableなTokenElementを表現するクラス。
    /// </summary>
    [ToString]
    public class TokenElement : IEquatable<TokenElement>
    {
        // オリジナルでの使用実績がなかったのでコメントアウト。
        //private static readonly long serialVersionUID = -9055285891555999514L;

        /// <summary>
        /// the surface form of the token
        /// </summary>
        public string Surface { get; private set; }

        private List<string> tags;

        /// <summary>
        /// token metadata (POS, etc)
        /// </summary>
        public ReadOnlyCollection<string> Tags => tags.AsReadOnly();

        /// <summary>
        /// the character position of the token in the sentence
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// token reading
        /// </summary>
        public string Reading { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tagList">The tag list.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="reading">The reading.</param>
        public TokenElement(string word, List<string> tagList, int offset, string reading)
        {
            this.Surface = word;
            this.tags = new List<string>(tagList);
            this.Offset = offset;
            this.Reading = reading;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tagList">The tag list.</param>
        /// <param name="offset">The offset.</param>
        public TokenElement(string word, List<string> tagList, int offset) : this(word, tagList, offset, word)
        { }

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="obj">The that.</param>
        /// <returns>A bool.</returns>
        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj == null || this.GetType() != obj.GetType()) return false;

            return Equals(other: obj as TokenElement);
        }

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="other">The that.</param>
        /// <returns>A bool.</returns>
        public bool Equals(TokenElement? other)
        {
            if (this == other) return true;
            if (other == null) return false;

            if (this.Surface != other.Surface) return false;
            if (!this.tags.SequenceEqual(other.tags)) return false;
            if (this.Offset != other.Offset) return false;
            if (this.Reading != other.Reading) return false;
            return true;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>An int.</returns>
        public override int GetHashCode()
        {
            // MEMO: JAVAのオリジナル実装のHash値計算は以下のとおり。
            //    int result = surface.hashCode();
            //    result = 31 * result + tags.hashCode();
            //    result = 31 * result + offset;
            //    result = 31 * result + reading.hashCode();
            //    return result;

            return HashCode.Combine(Surface, tags, Tags, Offset, Reading);
        }
    }
}