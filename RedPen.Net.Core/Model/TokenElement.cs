using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RedPen.Net.Core.Model
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
        public int LineNumber => OffsetMap.First().LineNum;

        /// <summary>the position of the first character in this token.</summary>
        public int Offset => OffsetMap.First().Offset;

        /// <summary>
        /// SurfaceとTagsの1つ目の文字列を取って人が目視可能な文字列表現を取得する関数。
        /// </summary>
        /// <returns>A string.</returns>
        public string GetSurfaceAndTagString()
        {
            if (Tags.Any())
            {
                return $"{Surface}({Tags[0]})";
            }
            else
            {
                return $"{Surface}(unknown)";
            }
        }

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
            Surface = surface;
            Tags = tags;
            Reading = reading;
            OffsetMap = offsetMap;
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
            var tags = string.Join(", ", Tags.Select(i => $"\"{i}\""));
            return $"TokenElement {{ Surface = \"{Surface}\", Reading = \"{Reading}\", LineNumber = {LineNumber}, Offset = {Offset} , Tags = [ {tags} ]}}";
        }

        /// <summary>
        /// 2つのTokenElementのTagの内容がマッチするかを判定する。
        /// MEMO: タグ長が不一致の場合は、短い方の長さ分で一致したらTrue、片方が「*」だった場合はそのタグは一致したとみなす。
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>2つのTokenElementが意味的にマッチするならTrue、不一致ならFalse</returns>
        public bool MatchTags(TokenElement other)
        {
            // どちらかが空集合の場合はマッチしているとみなす。
            if (Tags.Count == 0 || other.Tags.Count == 0)
            {
                return true;
            }

            // 完全一致ならTrueを返す。
            if (Tags.SequenceEqual(other.Tags))
            {
                return true;
            }

            // どちらかのタグ長が足りない場合、それは以降「*」と同じ扱いでマッチしているとみなす。
            // よってどちらか短いほうの長さまえ走査して不一致が見つからなければ一致としてよい。
            var minLen = Math.Min(Tags.Count, other.Tags.Count);

            for (var i = 0; i < minLen; i++)
            {
                // TODO: タグがstring.Emptyの場合、それは「*」にすべきか要検討。現在はstring.Emptyはstring.Emptyとして完全一致しなければならないルール。

                if (Tags[i] == "*" || other.Tags[i] == "*")
                {
                    // どちらかのタグが*の場合、それはマッチしているとみなす。
                    continue;
                }
                else if (Tags[i] == other.Tags[i])
                {
                    // タグが一致する場合はもちろんマッチしているとみなす。
                    continue;
                }
                else
                {
                    // マッチしていない場合はfalseを返す。
                    return false;
                }
            }

            return true;
        }
    }
}
