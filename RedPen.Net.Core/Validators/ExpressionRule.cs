using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators
{
    /// <summary>トークン列によって表現された表現ルール。</summary>
    public record ExpressionRule
    {
        /// <summary>Tokenのリスト。</summary>
        public ImmutableList<TokenElement> Tokens { get; init; } = ImmutableList<TokenElement>.Empty;

        /// <summary>
        /// ExpressionRuleの持つフレーズのSurfaceを連結した文字列を返す。
        /// </summary>
        /// <returns>A string.</returns>
        public string ToSurface() => string.Join("", this.Tokens.Select(e => e.Surface));

        /// <summary>
        /// ExpressionRuleの持つフレーズのReadingを連結した文字列を返す。
        /// </summary>
        /// <returns>A string.</returns>
        public string ToReading() => string.Join("", this.Tokens.Select(e => e.Reading));

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            IList<TokenElement> tokens)
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

            // ExpressionRuleと同じ長さの部分Tokenリストを取り出し、conditionで比較しマッチングを取る。
            List<ImmutableList<TokenElement>> matchedTokens = new List<ImmutableList<TokenElement>>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                // 入力Token列からRuleのToken長だけ取り出す。
                List<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count).ToList();

                // RuleのTokenと入力テキストのTokenのマッチングを取る。
                bool isMatch = true;
                for (int j = 0; j < this.Tokens.Count; j++)
                {
                    isMatch = condition(this.Tokens[j], currentTokenSequence[j]);
                    if (!isMatch)
                    {
                        break;
                    }
                }

                if (isMatch)
                {
                    matchedTokens.Add(currentTokenSequence.ToImmutableList());
                }
            }

            return (matchedTokens.Any(), matchedTokens);
        }

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchSurfaces(IList<TokenElement> tokens) =>
            MatchByCondition((x, y) => x.MatchSurface(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceの並びで、
        /// しかも各TokenのTagsがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchSurfacesAndTags(IList<TokenElement> tokens) =>
            MatchByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のReadingのパターンが1つあるいは複数存在するかどうかを判定する。
        /// MEMO: 主に複数Surfaceに対して読みが一意である日本語のフレーズマッチに使用する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchReadings(IList<TokenElement> tokens) =>
            MatchByCondition((x, y) => x.MatchReading(y), tokens);

        /// <summary>
        /// 引数のトークンリスト内に、ExpressionRuleと同じ順序のSurface、Tags、Readingがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A (bool isMatch, List&lt;ImmutableList&lt;TokenElement&gt;&gt; tokens) .</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) Matches(IList<TokenElement> tokens) =>
            MatchByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y) && x.MatchReading(y), tokens);
    }
}
