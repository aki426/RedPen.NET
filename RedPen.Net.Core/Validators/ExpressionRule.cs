using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators
{
    /// <summary>トークン分割された文のSurfaceレベルでの表現パターン。</summary>
    public record ExpressionRule
    {
        /// <summary>Tokenのリスト。</summary>
        public ImmutableList<TokenElement> Tokens { get; init; } = ImmutableList<TokenElement>.Empty;

        /// <summary>
        /// Tos the surface.
        /// </summary>
        /// <returns>A string.</returns>
        public string ToSurface() => string.Join("", this.Tokens.Select(e => e.Surface));

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。そのほかの場合は、ExpressionA bool.</returns>
        public bool MatchSurface(List<TokenElement> tokens)
        {
            if (this.Tokens.Count == 0)
            {
                // MEMO: 集合論的に考えてthis.Tokensが空集合だった場合はTrueが返るべき。
                return true;
            }

            if (tokens.Count == 0)
            {
                // マッチ先のトークンリストが空だった場合はthis.Tokensとマッチしようが無いのでFalseが返るべき。
                return false;
            }

            // 前提として、tokensが実際の文章で、this.Tokensより長いと考える。
            // 例として、
            // this.Tokens: This is
            // tokens: He said, "This is a pen."
            // だった場合、
            // He said, "This is a pen."
            //          [This is]
            // で部分的に一致するのでTrueを返す。

            bool isMatch = false;
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                isMatch = tokens.Skip(i).Take(this.Tokens.Count).Select(t => t.Surface).SequenceEqual(
                    this.Tokens.Select(t => t.Surface)
                );

                if (isMatch)
                {
                    break;
                }
            }

            return isMatch;
        }
    }
}
