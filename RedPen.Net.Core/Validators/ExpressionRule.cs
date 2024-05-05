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

            List<ImmutableList<TokenElement>> matchedTokens = new List<ImmutableList<TokenElement>>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                // 入力Token列からRuleのToken長だけ取り出す。
                List<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count).ToList();

                // RuleのTokenと入力テキストのTokenのマッチングを取る。
                bool isMatch = true;
                for (int j = 0; j < this.Tokens.Count; j++)
                {
                    // MEMO: Surfaceにもワイルドカードがある場合は無条件マッチとする。
                    if (this.Tokens[j].Surface == "*")
                    {
                        continue;
                    }
                    else if (this.Tokens[j].Surface == currentTokenSequence[j].Surface.ToLower())
                    {
                        continue;
                    }
                    else
                    {
                        // マッチしないTokenがあればFalseで抜ける。
                        isMatch = false;
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
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のSurfaceの並びで、
        /// しかも各TokenのTagsがマッチするパターンが1つあるいは複数存在するかどうかを判定する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchSurfacesAndTags(IList<TokenElement> tokens)
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

            // 先にSurfaceの一致を確認し、一致していた場合はTagsの一致を判定する。
            List<ImmutableList<TokenElement>> matchedTokens = new List<ImmutableList<TokenElement>>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                // 入力Token列からRuleのToken長だけ取り出す。
                List<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count).ToList();

                // RuleのTokenと入力テキストのTokenのマッチングを取る。
                bool isMatch = true;
                for (int j = 0; j < this.Tokens.Count; j++)
                {
                    // MEMO: Surfaceにもワイルドカードがある場合は無条件マッチとし、Tagsのマッチを確認する。
                    if (this.Tokens[j].Surface == "*" && this.Tokens[j].MatchTags(currentTokenSequence[j]))
                    {
                        continue;
                    }
                    else if (this.Tokens[j].Surface == currentTokenSequence[j].Surface.ToLower()
                        && this.Tokens[j].MatchTags(currentTokenSequence[j]))
                    {
                        continue;
                    }
                    else
                    {
                        // マッチしないTokenがあればFalseで抜ける。
                        isMatch = false;
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
        /// 指定されたトークンリスト内に、ExpressionRuleと同じ順序のReadingのパターンが1つあるいは複数存在するかどうかを判定する。
        /// MEMO: 主に複数Surfaceに対して読みが一意である日本語のフレーズマッチに使用する。
        /// </summary>
        /// <param name="tokens">TokenElementのリスト</param>
        /// <returns>引数tokensが空だった場合はTrue。
        /// そのほかの場合は、ルールにマッチするものがあればisMatchがTrue、tokensにマッチした部分リストをマッチした個数分返す。</returns>
        public (bool isMatch, List<ImmutableList<TokenElement>> tokens) MatchReadings(IList<TokenElement> tokens)
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
            // this.Tokens: ナイ コト モ ナイ
            // tokens: この表現は無いことも無い。
            // だった場合、
            // コノ　ヒョウゲン　ハ　ナイ　コト　モ　ナイ　。
            //                      [ナイ　コト　モ　ナイ]
            // で部分的に一致するのでTrueを返す。
            // Readingによる一致なので「この表現はないこともない。」といった漢字使用の有無にも対応できる。

            List<ImmutableList<TokenElement>> matchedTokens = new List<ImmutableList<TokenElement>>();
            for (int i = 0; i <= tokens.Count - this.Tokens.Count; i++)
            {
                // 入力Token列からRuleのToken長だけ取り出す。
                List<TokenElement> currentTokenSequence = tokens.Skip(i).Take(this.Tokens.Count).ToList();

                // RuleのTokenと入力テキストのTokenのマッチングを取る。
                bool isMatch = true;
                for (int j = 0; j < this.Tokens.Count; j++)
                {
                    // MEMO:Readingにもワイルドカードがある場合は無条件マッチとする。
                    if (this.Tokens[j].Reading == "*")
                    {
                        continue;
                    }
                    else if (this.Tokens[j].Reading == currentTokenSequence[j].Reading.ToLower())
                    {
                        continue;
                    }
                    else
                    {
                        // マッチしないTokenがあればFalseで抜ける。
                        isMatch = false;
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
    }
}
