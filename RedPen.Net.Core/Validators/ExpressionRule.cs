using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators
{
    /// <summary>トークン列によって表現された表現ルール。</summary>
    public record ExpressionRule
    {
        /// <summary>Tokenのリスト。</summary>
        public ImmutableList<(bool direct, TokenElement token)> TokenPattern { get; init; }

        public ImmutableList<TokenElement> Tokens => TokenPattern.Select(i => i.token).ToList().ToImmutableList();

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
        /// ExpressionRuleの表現パターンを文字列に変換する。
        /// </summary>
        /// <returns>Surface:Tags:Reading [+=] Surface:Tags:Reading...という表現形式の文字列 </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.TokenPattern[0].token.ConvertToExpressionRuleText());

            foreach (var (direct, token) in this.TokenPattern.Skip(1))
            {
                sb.Append(direct ? " + " : " = ");
                sb.Append(token.ConvertToExpressionRuleText());
            }

            return sb.ToString();
        }

        #region '='パターンを無視して連続したGrammerRuleを検出するMatchesConsecutive（隣接連続パターンのマッチ）メソッド群。

        // 隣接連続パターンしか対応していなかったExpressionRule時代の名残だが、
        // これはこれであいまいな名詞接続のパターン「格助詞の "の" + 名詞連続 + 各助詞の "の"」が連続する場合に有用であるため残しておく。

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveByCondition(
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

            if (this.TokenPattern.Where(i => !i.direct).Any())
            {
                // 隣接連続ではない「=」による接続が含まれている場合は、このメソッドの想定ではないのでExceptionを投げる。
                throw new ArgumentException($"This method is only for consecutive patterns. If you want to use non-consecutive patterns '{this}', please use MatchExtend method.");
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
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveSurfaces(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceの並びで、
        /// しかも各TokenのTagsがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveSurfacesAndTags(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のReadingのパターンが1つあるいは複数存在するかどうかを判定する。
        /// MEMO: 主に複数Surfaceに対して読みが一意である日本語のフレーズマッチに使用する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveReadings(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchReading(y), tokens);

        /// <summary>
        /// 引数のトークンリスト内に、ExpressionRuleと同じ順序のSurface、Tags、Readingがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A (bool isMatch, List&lt;ImmutableList&lt;TokenElement&gt;&gt; tokens) .</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutive(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y) && x.MatchReading(y), tokens);

        #endregion '='パターンを無視して連続したGrammerRuleを検出するMatchesConsecutive（隣接連続パターンのマッチ）メソッド群。

        #region '+'パターンと'='パターン両方に対応したMatchメソッド群。同じ範囲を重複検出しない。

        /// <summary>
        /// Matches the by condition extend.
        /// </summary>
        /// <param name="condition">Token同士をマッチングする場合の条件式</param>
        /// <param name="tokens">ExpressionRuleに対してマッチングを取る相手のTokenリスト。通常SentenceのTokenリスト。</param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtendByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            List<TokenElement> tokens)
        {
            if (this.Tokens.Count == 0)
            {
                // MEMO: ExpressionRuleのパターンが空ならFalseで良い。
                return new List<ImmutableList<TokenElement>>();
            }

            List<TokenElement> restTokens = tokens;
            List<ImmutableList<TokenElement>> results = new List<ImmutableList<TokenElement>>();
            while (restTokens.Any())
            {
                var (list, rest) = MatchExtendByConditionRecursive(
                    condition,
                    this.TokenPattern.ToList(),
                    restTokens,
                    ImmutableList.Create<TokenElement>());

                if (list.Any())
                {
                    results.Add(list);
                }

                // iteration
                // 1トークン目でマッチ失敗した場合は、入力したトークン列がまるごと返ってきてしまうので、1つ頭を落とす。
                restTokens = restTokens.SequenceEqual(rest) ? rest.Skip(1).ToList() : rest;
            }

            return results;
        }

        /// <summary>
        /// ExpressionRuleのパターンとトークンリストのマッチングを再帰的に行う。
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="tokenPattern">The token pattern.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="result">The result.</param>
        /// <returns>マッチした場合1つ目のリストは空ではない。2つ目のリストはマッチしなかった時点の残りのTokenリスト。</returns>
        private (ImmutableList<TokenElement>, List<TokenElement> rest) MatchExtendByConditionRecursive(
            Func<TokenElement, TokenElement, bool> condition,
            List<(bool direct, TokenElement token)> tokenPattern,
            List<TokenElement> tokens,
            ImmutableList<TokenElement> result
            )
        {
            if (tokenPattern.Count == 0)
            {
                return (result, tokens);
            }

            if (tokens.Count == 0)
            {
                // マッチ先のトークンリストが先に底をついてしまったのでマッチしなかった。
                return (ImmutableList.Create<TokenElement>(), tokens);
            }

            // current token.
            var (direct, token) = tokenPattern.First();
            if (direct)
            {
                if (condition(token, tokens.First()))
                {
                    // マッチした場合は次のトークンへ移る。
                    return MatchExtendByConditionRecursive(
                        condition,
                        tokenPattern.Skip(1).ToList(),
                        tokens.Skip(1).ToList(),
                        result.Add(tokens.First()));
                }
                else
                {
                    // 直前のトークンにダイレクトに続くTokenのパターンなので、マッチしなかったらそこで終わり。
                    return (ImmutableList.Create<TokenElement>(), tokens);
                }
            }
            else
            {
                // 間隔を開けてマッチしてもよいパターン。
                if (condition(token, tokens.First()))
                {
                    // マッチした場合は次のトークンへ移る。
                    return MatchExtendByConditionRecursive(
                        condition,
                        tokenPattern.Skip(1).ToList(),
                        tokens.Skip(1).ToList(),
                        result.Add(tokens.First()));
                }
                else
                {
                    // マッチしなかった場合は次のトークンのマッチングへイテレーションする。
                    return MatchExtendByConditionRecursive(
                        condition,
                        tokenPattern,
                        tokens.Skip(1).ToList(),
                        result.Add(tokens.First()));
                }
            }
        }

        /// <summary>
        /// 拡張ExpressionRule表現に対して入力トークンリストのマッチングをSurface、Tags、Readingの全てで行う。
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtend(List<TokenElement> tokens) =>
            MatchExtendByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y) && x.MatchReading(y), tokens);

        #endregion '+'パターンと'='パターン両方に対応したMatchメソッド群。同じ範囲を重複検出しない。
    }
}
