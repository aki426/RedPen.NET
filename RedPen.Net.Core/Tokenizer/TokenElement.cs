using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RedPen.Net.Core.Parser;

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

        // MEMO: JapaneseExpressionVariationValidator.ConvertToTokenPositionsText()では、
        // シンプルにTokenElementに紐づけられたSentence.LineNumberをTokenElementの出現行数として使っている。
        // このことから、一旦Sentenceが複数行にまたがるケースの検討は留保し、
        // Sentence.LineNumberをTokenElement.LineNumberに引きうつすこととする。

        /// <summary>the line of the sentence as the line of the token.</summary>
        public int LineNumber { get; init; }

        // TODO: TokenElementにLineOffsetを持たせることで、TokenElementの出現位置をより詳細に表現できるようにする。

        /// <summary>
        /// the character position of the token in the sentence
        /// </summary>
        public int Offset { get; init; }

        /// <summary>
        /// token reading
        /// </summary>
        public string Reading { get; init; }

        /// <summary>TokenのSurfaceがLineOffset表現でどのような位置関係にあるかを1文字ずつ表現したもの</summary>
        public ImmutableList<LineOffset> OffsetMap { get; init; }

        public TokenElement(string word, ImmutableList<string> tagList, string reading, ImmutableList<LineOffset> offsetMap)
        {
            this.Surface = word;
            this.Tags = tagList;
            this.LineNumber = offsetMap.First().LineNum; // 位置指定子が空はおかしいのでExceptionを投げたいのでFirst関数を使う。
            this.Offset = offsetMap.First().Offset; // 位置指定子が空はおかしいのでExceptionを投げたいのでFirst関数を使う。
            this.Reading = reading;
            this.OffsetMap = offsetMap;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tagList">The tag list.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="reading">The reading.</param>
        public TokenElement(string word, IList<string> tagList, int lineNumber, int offset, string reading)
        {
            this.Surface = word;
            this.Tags = tagList.ToImmutableList();
            this.LineNumber = lineNumber;
            this.Offset = offset;
            this.Reading = reading;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="offset">The offset.</param>
        public TokenElement(string word, IList<string> tags, int lineNumber, int offset) :
            this(word, tags, offset, lineNumber, word)
        { }

        /// <summary>
        /// ToString()
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            string tags = string.Join(", ", this.Tags.Select(i => $"\"{i}\""));
            return $"TokenElement {{ Surface = \"{this.Surface}\", Reading = \"{this.Reading}\", LineNumber = {this.LineNumber}, Offset = {this.Offset} , Tags = [ {tags} ]}}";
        }
    }
}
