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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Grammar
{
    /// <summary>
    /// GrammarRuleにおける文法要素1つ分（※Token1つ分に対応する）
    /// </summary>
    /// <param name="Adjacent">直前の要素に対して隣接する場合True、間に任意個の何らかの要素を許容する場合False。</param>
    /// <param name="Token">Ruleを表現するためのTokenElement</param>
    public record TokenRule(bool Adjacent, TokenElement Token);

    /// <summary>トークン列によって表現された文法ルール。Token列に対してパターンマッチする。</summary>
    public record GrammarRule
    {
        /// <summary>Tokenのリスト。</summary>
        public ImmutableList<TokenRule> Pattern { get; init; }

        public ImmutableList<TokenElement> Tokens => Pattern.Select(i => i.Token).ToList().ToImmutableList();

        /// <summary>
        /// GrammarRuleの持つフレーズのSurfaceを連結した文字列を返す。
        /// </summary>
        /// <returns>A string.</returns>
        public string ToSurface() => string.Join("", Tokens.Select(e => e.Surface));

        /// <summary>
        /// GrammarRuleの持つフレーズのReadingを連結した文字列を返す。
        /// </summary>
        /// <returns>A string.</returns>
        public string ToReading() => string.Join("", Tokens.Select(e => e.Reading));

        /// <summary>
        /// GrammarRuleの表現パターンを文字列に変換する。
        /// </summary>
        /// <returns>Surface:Tags:Reading [+=] Surface:Tags:Reading...という表現形式の文字列 </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Pattern[0].Token.ToGrammarRuleString());

            foreach (var (direct, token) in Pattern.Skip(1))
            {
                sb.Append(direct ? " + " : " = ");
                sb.Append(token.ToGrammarRuleString());
            }

            return sb.ToString();
        }

        #region '='パターンを無視して連続したGrammerRuleを検出するMatchesConsecutive（隣接連続パターンのマッチ）メソッド群。

        // 隣接連続パターンしか対応していなかったExpressionRule時代の名残だが、
        // これはこれであいまいな名詞接続のパターン「格助詞の "の" + 名詞連続 + 各助詞の "の"」が連続する場合に有用であるため残しておく。

        /// <summary>
        /// 指定されたトークンリスト内に、GrammarRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            IList<TokenElement> tokens)
        {
            if (Tokens.Count == 0)
            {
                // MEMO: 集合論的に考えてthis.Tokensが空集合だった場合はTrueが返るべき。
                return (true, new List<ImmutableList<TokenElement>>());
            }

            if (tokens.Count == 0)
            {
                // マッチ先のトークンリストが空だった場合はthis.Tokensとマッチしようが無いのでFalseが返るべき。
                return (false, new List<ImmutableList<TokenElement>>());
            }

            if (Pattern.Where(i => !i.Adjacent).Any())
            {
                // 隣接連続ではない、「=」による接続が含まれている場合は、このメソッドの想定ではないのでExceptionを投げる。
                throw new ArgumentException($"This method is only for consecutive patterns. If you want to use non-consecutive patterns '{this}', please use MatchExtend method.");
            }

            // Tokensと同じ長さの部分Tokenリストを取り出し、conditionで比較しマッチングを取る。
            var matchedTokens = new List<ImmutableList<TokenElement>>();
            for (var i = 0; i <= tokens.Count - Tokens.Count; i++)
            {
                // 入力Token列からRuleのToken長だけ取り出す。
                var currentTokenSequence = tokens.Skip(i).Take(Tokens.Count).ToList();

                // RuleのTokenと入力テキストのTokenのマッチングを取る。
                var isMatch = true;
                for (var j = 0; j < Tokens.Count; j++)
                {
                    isMatch = condition(Tokens[j], currentTokenSequence[j]);
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
        /// 指定されたトークンリスト内に、GrammarRuleと同じ順序のSurfaceのパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveSurfaces(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、GrammarRuleと同じ順序のReadingのパターンが1つあるいは複数存在するかどうかを判定する。
        /// MEMO: 主に複数Surfaceに対して読みが一意である日本語のフレーズマッチに使用する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveReadings(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchReading(y), tokens);

        /// <summary>
        /// 指定されたトークンリスト内に、GrammarRuleと同じ順序のSurfaceの並びで、
        /// しかも各TokenのPoSがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveSurfacesAndTags(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y) && x.MatchPartOfSpeech(y), tokens);

        /// <summary>
        /// 引数のトークンリスト内に、GrammarRuleと同じ順序のTokenパターンがマッチするシーケンスが1つあるいは複数存在するかどうかを判定する。
        /// NOTE: マッチした結果のTokenシーケンスはラップしている可能性がある。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A (bool isMatch, List&lt;ImmutableList&lt;TokenElement&gt;&gt; tokens) .</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutive(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) =>
                x.MatchSurface(y)
                && x.MatchReading(y)
                && x.MatchPartOfSpeech(y)
                && x.MatchInflectionForm(y)
                && x.MatchInflectionType(y)
                && x.MatchBaseForm(y),
                tokens);

        #endregion '='パターンを無視して連続したGrammerRuleを検出するMatchesConsecutive（隣接連続パターンのマッチ）メソッド群。

        #region '+'パターンと'='パターン両方に対応したMatchメソッド群。同じ範囲を重複検出しない。

        // 「A = C + D」というパターンと「ABACDEF」というトークン列があった場合、マッチを取ると
        // 「ABACD」というヒット列と「EF」というrest列が返る。

        /// <summary>
        /// Matches the by condition extend.
        /// </summary>
        /// <param name="condition">Token同士をマッチングする場合の条件式</param>
        /// <param name="tokens">GrammarRuleに対してマッチングを取る相手のTokenリスト。通常SentenceのTokenリスト。</param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtendByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            ImmutableList<TokenElement> tokens)
        {
            // NOTE: GrammarRuleのパターンが空なら空リストで返す。
            if (!Tokens.Any()) { return new List<ImmutableList<TokenElement>>(); }

            var restTokens = tokens;
            var results = new List<ImmutableList<TokenElement>>();
            while (restTokens.Any())
            {
                var (list, rest) = MatchExtendByConditionRecursive(
                    condition,
                    Pattern,
                    restTokens.ToImmutableList(),
                    ImmutableList.Create<TokenElement>());

                if (list.Any())
                {
                    results.Add(list);
                }

                // iteration
                // 1トークン目でマッチ失敗した場合は、入力したトークン列がまるごと返ってきてしまうので、1つ頭を落とす。
                restTokens = restTokens.SequenceEqual(rest) ? rest.Skip(1).ToImmutableList() : rest;
            }

            return results;
        }

        /// <summary>
        /// GrammarRuleのパターンとトークンリストのマッチングを再帰的に行う。
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="tokenPattern">The token pattern.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="result">The result.</param>
        /// <returns>マッチした場合1つ目のリストは空ではない。2つ目のリストはマッチしなかった時点の残りのTokenリスト。</returns>
        private (ImmutableList<TokenElement>, ImmutableList<TokenElement> rest) MatchExtendByConditionRecursive(
            Func<TokenElement, TokenElement, bool> condition,
            ImmutableList<TokenRule> tokenPattern,
            ImmutableList<TokenElement> tokens,
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
            var (adjacent, token) = tokenPattern.First();
            if (adjacent)
            {
                if (condition(token, tokens.First()))
                {
                    // マッチした場合は次のトークンへ移る。
                    return MatchExtendByConditionRecursive(
                        condition,
                        tokenPattern.Skip(1).ToImmutableList(),
                        tokens.Skip(1).ToImmutableList(),
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
                        tokenPattern.Skip(1).ToImmutableList(),
                        tokens.Skip(1).ToImmutableList(),
                        result.Add(tokens.First()));
                }
                else
                {
                    // マッチしなかった場合は次のトークンのマッチングへイテレーションする。
                    return MatchExtendByConditionRecursive(
                        condition,
                        tokenPattern,
                        tokens.Skip(1).ToImmutableList(),
                        result.Add(tokens.First()));
                }
            }
        }

        /// <summary>
        /// 拡張GrammarRule表現に対して入力トークンリストのマッチングをSurface、Tags、Readingの全てで行う。
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtend(ImmutableList<TokenElement> tokens) =>
            MatchExtendByCondition((x, y) =>
                x.MatchSurface(y)
                && x.MatchReading(y)
                && x.MatchPartOfSpeech(y)
                && x.MatchInflectionForm(y)
                && x.MatchInflectionType(y)
                && x.MatchBaseForm(y),
                tokens);

        /// <summary>ReverseしたToken列に適用し同じ結果を得ることができるGrammarRuleの逆パターン。</summary>
        public ImmutableList<TokenRule> ReversedPattern
        {
            get
            {
                // NOTE: 「XAXXB」という文字列に対して、
                // "A = B"でパターンマッチすると「AXXB」がマッチする。
                // Reverseした「BXXAX」にReverseしたパターンつまり「B = A」でパターンマッチさせ「BXXA」を得たい。
                // つまり"A = B"から"B = A"となるPatternを取得するのがこの関数の目的。

                // NOTE: ルール文字列、パターン、リバースしたパターン、マッチングが可能な"正しい"リバースしたパターンの例を並べると
                // "A = B"
                // (true, A), (false, B)
                // (false, B), (true, A)
                // (true, B), (false, A)

                // Patternが空リストの場合空リストを返す。
                if (!this.Pattern.Any()) { return ImmutableList.Create<TokenRule>(); }

                // Reverseしたリストをdirectプロパティを考慮し詰めなおしていく。
                var list = this.Pattern.Reverse();
                List<TokenRule> result = new List<TokenRule>
                {
                    // 先頭はAdjacentがTrueになる。
                    new TokenRule(true, list.First().Token)
                };

                // 要素が1個しかない場合は先頭のものを詰めたらもう十分。
                if (list.Count >= 2)
                {
                    var pairs = list.Zip(list.Skip(1), (pre, next) => new { Pre = pre, Next = next });
                    foreach (var pair in pairs)
                    {
                        // 裏返したPattern列なので、Preが持っているDirectプロパティはNextに付け替える必要がある。
                        result.Add(new(pair.Pre.Adjacent, pair.Next.Token));
                    }
                }

                return result.ToImmutableList();
            }
        }

        /// <summary>
        /// Token列の終端にパターンがマッチするかどうかを検証する関数。
        /// NOTE: Sentenceの末尾にマッチするかしないか、のみ判定する。
        /// </summary>
        /// <param name="condition">Token同士をマッチングする場合の条件式</param>
        /// <param name="tokens">GrammarRuleに対してマッチングを取る相手のTokenリスト。通常SentenceのTokenリスト。</param>
        /// <returns></returns>
        public (bool success, ImmutableList<TokenElement> tokens) MatchExtendAtEndByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            ImmutableList<TokenElement> tokens)
        {
            // NOTE: 対象TokenリストまたはGrammarRuleのパターンが空なら空リストで返す。
            if (!Tokens.Any() || !Pattern.Any()) { return (true, ImmutableList<TokenElement>.Empty); }

            // 語尾に対してマッチさせるので、1回マッチしなかったらマッチング終わり。

            var restTokens = tokens;
            var results = new List<ImmutableList<TokenElement>>();

            var (list, rest) = MatchExtendByConditionRecursive(
                condition,
                this.ReversedPattern,
                tokens.Reverse(),
                ImmutableList.Create<TokenElement>());

            if (list.Any())
            {
                return (true, list.Reverse());
            }
            else
            {
                return (false, list.Reverse());
            }
        }

        /// <summary>
        /// Token列の終端にパターンがマッチするかどうかを検証する関数。
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public (bool success, ImmutableList<TokenElement> tokens) MatchExtendAtEnd(ImmutableList<TokenElement> tokens) =>
            MatchExtendAtEndByCondition((x, y) =>
                x.MatchSurface(y)
                && x.MatchReading(y)
                && x.MatchPartOfSpeech(y)
                && x.MatchInflectionForm(y)
                && x.MatchInflectionType(y)
                && x.MatchBaseForm(y),
                tokens);

        #endregion '+'パターンと'='パターン両方に対応したMatchメソッド群。同じ範囲を重複検出しない。
    }
}
