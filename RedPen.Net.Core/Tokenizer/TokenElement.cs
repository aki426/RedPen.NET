using System.Collections.Generic;
using System.Collections.Immutable;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// ImmutableなTokenElementを表現するクラス。
    /// </summary>
    public record TokenElement
    {
        // オリジナルでの使用実績がなかったのでコメントアウト。
        //private static readonly long serialVersionUID = -9055285891555999514L;

        /// <summary>
        /// the surface form of the token
        /// </summary>
        public string Surface { get; init; }

        /// <summary>
        /// token metadata (POS, etc)
        /// </summary>
        public ImmutableList<string> Tags { get; init; }

        /// <summary>
        /// the character position of the token in the sentence
        /// </summary>
        public int Offset { get; init; }

        /// <summary>
        /// token reading
        /// </summary>
        public string Reading { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tagList">The tag list.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="reading">The reading.</param>
        public TokenElement(string word, IList<string> tagList, int offset, string reading)
        {
            this.Surface = word;
            this.Tags = tagList.ToImmutableList();
            this.Offset = offset;
            this.Reading = reading;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="offset">The offset.</param>
        public TokenElement(string word, IList<string> tags, int offset) :
            this(word, tags, offset, word)
        { }
    }
}
