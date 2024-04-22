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
        /// <summary>the surface form of the token</summary>
        public string Surface { get; init; }

        /// <summary>token metadata (POS, etc)</summary>
        public ImmutableList<string> Tags { get; init; }

        /// <summary>the reading of this token</summary>
        public string Reading { get; init; }

        /// <summary>TokenのSurfaceがLineOffset表現でどのような位置関係にあるかを1文字ずつ表現したもの</summary>
        public ImmutableList<LineOffset> OffsetMap { get; init; }

        // MEMO: 位置指定子が空はおかしいのでExceptionを投げたいのでFirst関数を使う。

        /// <summary>the line of the token's first character.</summary>
        public int LineNumber => this.OffsetMap.First().LineNum;

        /// <summary>the position of the first character in this token.</summary>
        public int Offset => this.OffsetMap.First().Offset;

        /// <summary>
        /// Surface, Tags, Reading, OffsetMapを完全に指定してTokenElementを生成する。
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="surface">The word.</param>
        /// <param name="tags">The tag list.</param>
        /// <param name="reading">The reading.</param>
        /// <param name="offsetMap">The offset map.</param>
        public TokenElement(string surface, ImmutableList<string> tags, string reading, ImmutableList<LineOffset> offsetMap)
        {
            this.Surface = surface;
            this.Tags = tags;
            this.Reading = reading;
            this.OffsetMap = offsetMap;
        }

        /// <summary>
        /// Token開始行、開始オフセット位置のみ与えて、OffsetMapを自動生成するコンストラクタ。Surface全体が連続していて1行に収まっていることを前提とする。
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="surface">The word.</param>
        /// <param name="tags">The tag list.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="reading">The reading.</param>
        public TokenElement(string surface, IList<string> tags, int lineNumber, int offset, string reading) :
            this(surface, tags.ToImmutableList(), reading, LineOffset.MakeOffsetList(lineNumber, offset, surface).ToImmutableList())
        { }

        /// <summary>
        /// ReadingがSurfaceと同じで、かつToken開始行、開始オフセット位置のみ与えて、OffsetMapを自動生成するコンストラクタ。Surface全体が連続していて1行に収まっていることを前提とする。
        /// Initializes a new instance of the <see cref="TokenElement"/> class.
        /// </summary>
        /// <param name="surface">The word.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="offset">The offset.</param>
        public TokenElement(string surface, IList<string> tags, int lineNumber, int offset) :
            this(surface, tags, offset, lineNumber, surface)
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
