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

using NLog;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    /// <summary>原文のテキスト位置と文字列の対応関係を記録した1センテンス分のオブジェクト。</summary>
    public record Sentence
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>センテンスがパラグラフ内で最初のセンテンスである場合Trueになるフラグ。</summary>
        public bool IsFirstSentence { get; init; }

        /// <summary>原文のセンテンスから改行など不要な要素を除去されたPlainな文字列。</summary>
        public string Content { get; init; }

        /// <summary>SentenceのContentがLineOffset表現でどのような位置関係にあるかを1文字ずつ表現したもの</summary>
        public List<LineOffset> OffsetMap { get; init; }

        /// <summary>＜非推奨＞センテンスの先頭文字が存在する行番号。</summary>
        public int LineNumber => this.OffsetMap[0].LineNum;

        /// <summary>＜非推奨＞センテンスの先頭文字が存在するオフセット位置。</summary>
        public int StartPositionOffset => this.OffsetMap[0].Offset;

        /// <summary>センテンスが参照しているリンクのリスト。</summary>
        public List<string> Links { get; init; }

        /// <summary>センテンスを構成するTokenのリスト。</summary>
        public List<TokenElement> Tokens { get; init; }

        /// <summary>
        /// lineNum行の位置startOffsetで開始する、改行コード\nのみを改行とするOffsetMapを持つSentenceを生成する。
        /// NOTE: ParserによりSentenceを生成されない場合のテストケース向けコンストラクタ。
        /// Initializes a new instance of the <see cref="Sentence"/> class.
        /// </summary>
        /// <param name="content">センテンスの文字列。改行コードは\nで\nのみを改行とみなす。</param>
        /// <param name="lineNum">The line num.</param>
        /// <param name="StartOffset">The start offset.</param>
        public Sentence(string content, int lineNum, int StartOffset = 0)
        {
            this.Content = content;
            this.IsFirstSentence = false;
            this.Links = new List<string>();
            this.Tokens = new List<TokenElement>();
            this.OffsetMap = LineOffset.MakeOffsetList(lineNum, StartOffset, content);
        }

        /// <summary>
        /// 1文字ずつのLineOffsetを記録した完全なSentenceインスタンスを生成する。
        /// NOTE: Parser向けの正規コンストラクタ。
        /// Initializes a new instance of the <see cref="Sentence"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="offsetMap">The offset map.</param>
        /// <param name="links">The links.</param>
        /// <param name="IsFirstSentence">If true, is first sentence.</param>
        public Sentence(string content, List<LineOffset> offsetMap, List<string> links, bool IsFirstSentence = false)
        {
            // MEMO: 位置が存在しないSentenceは存在しないという前提。
            if (!offsetMap.Any() || offsetMap.Count == 0)
            {
                string msg = "OffsetMap is empty. Sentence content: " + content;
                LOG.Error(msg);
                throw new System.ArgumentException(msg);
            }

            // MEMO: Contentの文字数とOffsetMapの要素数が一致していない場合はエラーとする。
            if (content.Length != offsetMap.Count)
            {
                string msg = "Content length and OffsetMap size are different. Sentence content: " + content;
                LOG.Error(msg);
                throw new System.ArgumentException(msg);
            }

            this.Content = content;
            this.IsFirstSentence = IsFirstSentence;
            this.Links = links;
            this.Tokens = new List<TokenElement>();
            this.OffsetMap = offsetMap;
        }

        /// <summary>
        /// Sentence.ContentのIndexに対して元のテキストのLineOffsetを返す関数。
        /// </summary>
        /// <param name="index">0始まり、Sentence.Content.Length - 1まで</param>
        /// <returns>指定IndexのLineOffset</returns>
        public LineOffset ConvertToLineOffset(int index)
        {
            // MEMO: OffsetMapはOffsetなので、文字の前に何文字存在するかを表す。
            // イメージとして「あいう」という文字列があったら、「い」のOffsetは「あ」と「い」の間の隙間の位置。

            // MEMO: OffsetMapは空ではないこととContentと同じ要素数であることがコンストラクタで保証されている。
            // 範囲外のIndexへのアクセスはエラーとする。
            if (index < 0 || this.OffsetMap.Count <= index)
            {
                string msg = $"Invalid index: {index} in sentence : {this.Content}";
                LOG.Error(msg);
                throw new System.ArgumentException(msg);
            }

            return this.OffsetMap[index];
        }

        /// <summary>
        /// LineOffsetからSentence.ContentのIndexを逆引きする関数。
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>見つからなかった場合は-1</returns>
        public int ConvertToIndex(LineOffset offset)
        {
            return this.OffsetMap.IndexOf(offset); // 発見されなかった場合は-1が返る。
        }
    }
}
