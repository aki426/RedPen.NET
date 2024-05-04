using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RedPen.Net.Core.Model;

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
        public (bool isMatch, List<TokenElement> tokens) MatchSurface(List<TokenElement> tokens)
        {
            if (this.Tokens.Count == 0)
            {
                // MEMO: 集合論的に考えてthis.Tokensが空集合だった場合はTrueが返るべき。
                return (true, new List<TokenElement>());
            }

            if (tokens.Count == 0)
            {
                // マッチ先のトークンリストが空だった場合はthis.Tokensとマッチしようが無いのでFalseが返るべき。
                return (false, new List<TokenElement>());
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
            List<TokenElement> matchedTokens = new List<TokenElement>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                IEnumerable<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count);
                isMatch = currentTokenSequence.Select(t => t.Surface).SequenceEqual(
                    this.Tokens.Select(t => t.Surface)
                );

                if (isMatch)
                {
                    matchedTokens = currentTokenSequence.ToList();
                    break;
                }
            }

            return (isMatch, matchedTokens);
        }

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchSurfaces(IList<TokenElement> tokens)
        {
            if (this.Tokens.Count == 0)
            {
                // MEMO: 集合論的に考えてthis.Tokensが空集合だった場合はTrueが返るべき。
                return (true, new List<ImmutableList<TokenElement>>());
            }

            if (tokens.Count == 0)
            {
                // マッチ先のトークンリストが空だった場合はthis.Tokensとマッチしようが無いのでFalseが返るべき。
                return (false, new List<ImmutableList<TokenElement>>());
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
            List<ImmutableList<TokenElement>> matchedTokens = new List<ImmutableList<TokenElement>>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                IEnumerable<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count);
                isMatch = currentTokenSequence.Select(t => t.Surface).SequenceEqual(
                    this.Tokens.Select(t => t.Surface)
                );

                if (isMatch)
                {
                    matchedTokens.Add(currentTokenSequence.ToImmutableList());
                }
            }

            return (matchedTokens.Any(), matchedTokens);
        }
    }
}
