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
    /// <summary>トークン列によって表現された文法ルール。Token列に対してパターンマッチする。</summary>
    public record GrammarRule
    {
        /// <summary>Tokenのリスト。</summary>
        public ImmutableList<(bool direct, TokenElement token)> Pattern { get; init; }

        public ImmutableList<TokenElement> Tokens => Pattern.Select(i => i.token).ToList().ToImmutableList();

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
            sb.Append(Pattern[0].token.ConvertToGrammarRuleText());

            foreach (var (direct, token) in Pattern.Skip(1))
            {
                sb.Append(direct ? " + " : " = ");
                sb.Append(token.ConvertToGrammarRuleText());
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

            if (Pattern.Where(i => !i.direct).Any())
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
        /// 指定されたトークンリスト内に、GrammarRuleと同じ順序のSurfaceの並びで、
        /// しかも各TokenのTagsがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchesConsecutiveSurfacesAndTags(IList<TokenElement> tokens) =>
            MatchesConsecutiveByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y), tokens);

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
        /// 引数のトークンリスト内に、GrammarRuleと同じ順序のSurface、Tags、Readingがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
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
        /// <param name="tokens">GrammarRuleに対してマッチングを取る相手のTokenリスト。通常SentenceのTokenリスト。</param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtendByCondition(
            Func<TokenElement, TokenElement, bool> condition,
            List<TokenElement> tokens)
        {
            if (Tokens.Count == 0)
            {
                // MEMO: GrammarRuleのパターンが空ならFalseで良い。
                return new List<ImmutableList<TokenElement>>();
            }

            var restTokens = tokens;
            var results = new List<ImmutableList<TokenElement>>();
            while (restTokens.Any())
            {
                var (list, rest) = MatchExtendByConditionRecursive(
                    condition,
                    Pattern.ToList(),
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
        /// GrammarRuleのパターンとトークンリストのマッチングを再帰的に行う。
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
        /// 拡張GrammarRule表現に対して入力トークンリストのマッチングをSurface、Tags、Readingの全てで行う。
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public List<ImmutableList<TokenElement>> MatchExtend(List<TokenElement> tokens) =>
            MatchExtendByCondition((x, y) => x.MatchSurface(y) && x.MatchTags(y) && x.MatchReading(y), tokens);

        #endregion '+'パターンと'='パターン両方に対応したMatchメソッド群。同じ範囲を重複検出しない。
    }
}
