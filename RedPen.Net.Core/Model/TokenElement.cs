﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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
using RedPen.Net.Core.Utility;

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
            // NOTE: KuromojiでTokenizeした場合、英語表現はReadingがNullになるため、その場合はSurfaceをToLowerしたものをReadingとして扱う。
            Reading = reading ?? surface.ToLower();
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
            return $"TokenElement {{ Surface = \"{Surface}\", Reading = \"{Reading}\", Tags = [ {tags} ], OffsetMap = {string.Join("-", OffsetMap.Select(o => o.ConvertToShortText()))}}}";
        }

        /// <summary>
        /// GrammarRuleに変換可能なToken表現形式文字列を取得する。
        /// </summary>
        /// <returns>A string.</returns>
        public string ConvertToGrammarRuleText()
        {
            // NOTE: 末尾の空タグは削除する。
            ImmutableList<string> currentTags = Tags.Reverse();
            while (currentTags.Any())
            {
                if (currentTags[0] == "")
                {
                    currentTags = currentTags.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
            string tagStr = currentTags.Any() ? string.Join(",", currentTags.Reverse()) : "";

            // 後の構成要素が無ければ省略して良いのでなるべく省略する。見やすさのため。
            if (Reading != "")
            {
                return $"{Surface}:{tagStr}:{Reading}";
            }
            else if (tagStr != "")
            {
                return $"{Surface}:{tagStr}";
            }
            else
            {
                return $"{Surface}";
            }
        }

        /// <summary>
        /// 相手のTokenと一部でも位置が重なっているかどうかを判定する関数。
        /// MEMO: 完全一致ではないことに注意せよ。
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public bool Overlap(TokenElement other) => OffsetMap.Any(i => other.OffsetMap.Contains(i));

        /// <summary>
        /// カタカナ語＝すべてカタカナで構成されたSurfaceかどうかを判定する関数。
        /// 1文字でも非カタカナ文字があればFalseを返す。
        /// MEMO: 記号を含む場合はFalseを返すので注意。
        /// </summary>
        /// <returns>A bool.</returns>
        public bool IsKatakanaWord() => Surface.All(UnicodeUtility.IsKatakana);

        /// <summary>
        /// カタカナ文字を1文字でも含むかどうかを判定する関数。
        /// 1文字でもカタカナ文字があればTrueを返す。
        /// </summary>
        /// <returns>A bool.</returns>
        public bool HasKatakana() => Surface.Any(c => UnicodeUtility.IsKatakana(c));

        /// <summary>
        /// Surfaceがマッチするかどうか＝GrammarRuleとしてマッチするかどうかを判定する関数。
        /// MEMO: 完全一致ではないことに注意。
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public bool MatchSurface(TokenElement other)
        {
            if (this.Surface == "*" || other.Surface == "*")
            {
                return true;
            }
            else if (this.Surface == string.Empty || other.Surface == string.Empty)
            {
                // Surfaceにおいても空文字列はワイルドカードとして扱う。
                return true;
            }
            else if (this.Surface.ToLower() == other.Surface.ToLower())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool MatchReading(TokenElement other)
        {
            if (this.Reading == "*" || other.Reading == "*")
            {
                return true;
            }
            else if (this.Reading == string.Empty || other.Reading == string.Empty)
            {
                // Readingにおいても空文字列はワイルドカードとして扱う。
                return true;
            }
            else if (this.Reading.ToLower() == other.Reading.ToLower())
            {
                return true;
            }
            else
            {
                return false;
            }
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
            // よってどちらか短いほうの長さまで走査して不一致が見つからなければ一致としてよい。
            var minLen = Math.Min(Tags.Count, other.Tags.Count);

            for (var i = 0; i < minLen; i++)
            {
                if (Tags[i] == "*" || other.Tags[i] == "*")
                {
                    // どちらかのタグが*の場合、それはマッチしているとみなす。
                    continue;
                }
                else if (Tags[i] == string.Empty || other.Tags[i] == string.Empty)
                {
                    // MEMO: タグがstring.Emptyの場合、それは「*」と同じ扱いとする。
                    // どちらかのタグが空文字列の場合、それはマッチしているとみなす。
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

        /// <summary>
        /// 一連のTokenリスト中の隣接する名詞を連結して1つのTokenにまとめたリストを返す。
        /// MEMO: 結果のTokenリストのSurfaceを連結すると元の文章＝元のTokenリストのSurfaceを連結したものとなる。
        /// ただし、区切り位置は異なる。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>A list of TokenElements.</returns>
        public static List<TokenElement> ConvertToConcatedNouns(List<TokenElement> tokens)
        {
            List<TokenElement> result = new List<TokenElement>();

            // 連続する名詞をStackに入れて連結して結果リストに入れる。
            List<TokenElement> nounStack = new List<TokenElement>();

            foreach (var token in tokens)
            {
                if (token.Tags[0] == "名詞")
                {
                    nounStack.Add(token);
                }
                else
                {
                    if (nounStack.Count == 1)
                    {
                        result.Add(nounStack[0]);
                        // スタックをクリアして次の名詞連結を開始。
                        nounStack.Clear();
                    }
                    else if (nounStack.Count > 1)
                    {
                        result.Add(new TokenElement(
                            string.Join("", nounStack.Select(t => t.Surface)),
                            new List<string> { "名詞", "一般", "*", "*" }.ToImmutableList(),
                            string.Join("", nounStack.Select(t => t.Reading)),
                            nounStack.SelectMany(t => t.OffsetMap).ToImmutableList()
                        ));
                        // スタックをクリアして次の名詞連結を開始。
                        nounStack.Clear();
                    }

                    // 現在のトークンを追加。
                    result.Add(token);
                }
            }

            // 体言止めの場合を考慮して、最後のスタックを処理する。
            if (nounStack.Count == 1)
            {
                result.Add(nounStack[0]);
            }
            else if (nounStack.Count > 1)
            {
                result.Add(new TokenElement(
                    string.Join("", nounStack.Select(t => t.Surface)),
                    new List<string> { "名詞", "一般", "*", "*" }.ToImmutableList(),
                    string.Join("", nounStack.Select(t => t.Reading)),
                    nounStack.SelectMany(t => t.OffsetMap).ToImmutableList()
                ));
            }

            return result;
        }

        /// <summary>
        /// KuromojiによるToken分割は、カタカナとそれ以外文字を含む固有名詞を1つのTokenに分割するため、さらにそれを再分割する。
        /// 例）「仮面ライダー」は固有名詞として1Tokenに分割され、「仮面」と「ライダー」には分割されない。
        /// これを「仮面」と「ライダー」に分割したTokenにする。
        /// 一方、仮面とライダーに対応するRedingを「カメンライダー」から抽出することは困難なので、混乱を避けるために分割後は空文字列とする。
        /// MEMO: Readingを必要とするロジックではこの関数を使わないこと。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>カタカナ文字を含むTokenは分割された結果のリスト。分割されたTokenのReadingはSurfaceと対応しないことに注意。</returns>
        public static List<TokenElement> ParseKatakanaToken(List<TokenElement> tokens)
        {
            List<TokenElement> result = new List<TokenElement>();

            foreach (var token in tokens)
            {
                // カタカナを含むがすべてがカタカナではないTokenを判定する。
                if (token.HasKatakana() && !token.IsKatakanaWord())
                {
                    List<(char c, LineOffset lo)> cache = new List<(char c, LineOffset lo)>();

                    for (int i = 0; i < token.Surface.Length; i++)
                    {
                        if (cache.Any())
                        {
                            // 文字種が異なる場合はそこが区切り。
                            if (UnicodeUtility.IsKatakana(cache.Last().c) != UnicodeUtility.IsKatakana(token.Surface[i]))
                            {
                                // Readingを分割することは困難なので空文字列とする。
                                result.Add(new TokenElement(
                                    string.Join("", cache.Select(i => i.c)),
                                    token.Tags,
                                    "",
                                    cache.Select(i => i.lo).ToImmutableList()));

                                cache.Clear();
                            }

                            cache.Add((token.Surface[i], token.OffsetMap[i]));
                        }
                        else
                        {
                            cache.Add((token.Surface[i], token.OffsetMap[i]));
                        }
                    }

                    // 最後に残ったキャッシュを追加。
                    if (cache.Any())
                    {
                        // Readingを分割することは困難なので空文字列とする。
                        result.Add(new TokenElement(
                            string.Join("", cache.Select(i => i.c)),
                            token.Tags,
                            "",
                            cache.Select(i => i.lo).ToImmutableList()));
                    }
                }
                else
                {
                    // カタカナ文字を含まないか、すべてカタカナ文字のTokenはそのまま追加する。
                    result.Add(token);
                }
            }

            return result;
        }

        /// <summary>
        /// 一連のTokenリスト中の隣接する複数のカタカナ語を連結して1つのTokenにまとめたもののみのリストを返す。
        /// MEMO: カタカナ語1語のTokenは結果に含まれない。また、隣接する複数のカタカナ語を部分的に連結したものも含まれない。
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>連結されたカタカナ語のみのリスト。</returns>
        public static List<TokenElement> GetConcatedKatakanaWords(List<TokenElement> tokens)
        {
            List<TokenElement> result = new List<TokenElement>();

            // 連続する名詞をStackに入れて連結して結果リストに入れる。
            List<TokenElement> katakanaStack = new List<TokenElement>();

            foreach (var token in tokens)
            {
                if (token.IsKatakanaWord())
                {
                    katakanaStack.Add(token);
                }
                else
                {
                    // カタカナ語ではない＝連続カタカナ語の区切りに到達した場合。
                    if (katakanaStack.Count == 1)
                    {
                        // スタックをクリアして次のカタカナ語連結を開始。
                        katakanaStack.Clear();
                    }
                    else if (katakanaStack.Count > 1)
                    {
                        result.Add(new TokenElement(
                            string.Join("", katakanaStack.Select(t => t.Surface)),
                            // 連結したカタカナ語は名詞、一般固定で良いはず。
                            // TODO: Kuromozji + IPA辞書以外のTokenizerの場合、動詞として扱うカタカナ語もあるかもしれないので注意。
                            new List<string> { "名詞", "一般", "*", "*" }.ToImmutableList(),
                            string.Join("", katakanaStack.Select(t => t.Reading)),
                            katakanaStack.SelectMany(t => t.OffsetMap).ToImmutableList()
                        ));
                        // スタックをクリアして次の名詞連結を開始。
                        katakanaStack.Clear();
                    }
                }
            }

            // 体言止めの場合を考慮して、最後のスタックを処理する。
            if (katakanaStack.Count > 1)
            {
                result.Add(new TokenElement(
                    string.Join("", katakanaStack.Select(t => t.Surface)),
                    new List<string> { "名詞", "一般", "*", "*" }.ToImmutableList(),
                    string.Join("", katakanaStack.Select(t => t.Reading)),
                    katakanaStack.SelectMany(t => t.OffsetMap).ToImmutableList()
                ));
            }

            return result;
        }
    }
}
