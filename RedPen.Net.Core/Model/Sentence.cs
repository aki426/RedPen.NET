using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using System.Collections.Generic;

namespace RedPen.Net.Core.Model
{
    public record class Sentence
    {
        // TODO: recordで実装したが、隠ぺいしたほうが良いものはアクセシビリティを変更する。

        private static readonly long serialVersionUID = 3761982769692999924L;

        public string Content { get; set; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        public int LineNumber { get; init; } // need getter. setterはJAVA版でも使用実績がないので不要。
        public bool IsFirstSentence { get; set; } // need getter.
        public List<string> Links { get; init; } // need getter. List.add()はJAVA版でも使用実績がある。
        public List<TokenElement> Tokens { get; set; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        public int StartPositionOffset { get; init; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        /// <summary>SentenceのContentがLineOffset表現でどのような位置関係にあるかを1文字ずつ表現したもの</summary>
        public List<LineOffset> OffsetMap { get; init; }

        public Sentence(string sentenceContent, int lineNum) : this(sentenceContent, lineNum, 0)
        {
        }

        public Sentence(string sentenceContent, int lineNum, int startOffset)
        {
            this.Content = sentenceContent;
            this.LineNumber = lineNum;
            this.IsFirstSentence = false;
            this.Links = new List<string>();
            this.Tokens = new List<TokenElement>();
            this.StartPositionOffset = startOffset;
            this.OffsetMap = new List<LineOffset>();
        }

        // MEMMO: TokenElementにLineNumberを追加するにあたり、Sentenceは1行内に収まっており、
        // 改行された場合は別のSentenceと考えるルールを前提に置く。
        // TODO: この前提はテストケースによって検証されなければならない。
        // MEMO: 例えばMarkdownやAsciiDocの場合、改行はレンダリング時に無視されて1行に連結されてしまうので、
        // あくまでテキストファイルの行数を基準にした場合にSentenceを分割する必要があり、それが実際の意味上のセンテンスの区切り
        // （FullStopまでを1文とみなす）ことと食い違ってしまう可能性がある。

        // MEMO: この辺りの実態はList<LineOffset> OffsetMapに複数行の位置指定があるかどうかで判断する必要がある。

        public Sentence(string content, List<LineOffset> offsetMap, List<string> links)
        {
            this.Content = content;
            this.LineNumber = offsetMap[0].LineNum;
            this.IsFirstSentence = false;
            this.Links = links;
            this.Tokens = new List<TokenElement>();
            this.StartPositionOffset = offsetMap[0].Offset;
            this.OffsetMap = offsetMap;
        }

        /// <summary>
        /// Get offset position for specified character position.
        /// </summary>
        /// <param name="position">character position in a sentence</param>
        /// <returns>offset position</returns>
        public LineOffset? GetOffset(int position)
        {
            if (position >= 0)
            {
                // MEMO: あらかじめContentの全文字とLineOffsetの対応関係を表現した
                // List<LineOffset>が作成されており、Contentの文字数とList＜LineOffset>の要素数が同じに保たれていることが前提。
                if (this.OffsetMap.Count > position)
                {
                    return this.OffsetMap[position];
                }
                else if ((position > 0) && (this.OffsetMap.Count == position))
                {
                    // MEMO: Contentが空文字列ではない場合の末尾指定。改行かEOLのためのもの？
                    LineOffset last = this.OffsetMap[this.OffsetMap.Count - 1];
                    return new LineOffset(last.LineNum, last.Offset + 1);
                }

                // MEMO: Contentの長さを超えてLineOffsetを返すことになるはず。
                return new LineOffset(this.LineNumber, position);
            }

            return null;
        }

        /// <summary>
        /// Get the position of the supplied offset (ie: the position in the source text) in this sentence's normalized content
        /// </summary>
        /// <param name="offset">the position in the source text</param>
        /// <returns>the position in the setence's content</returns>
        public int GetOffsetPosition(LineOffset offset)
        {
            int position = this.OffsetMap.IndexOf(offset);
            return position < 0 ? 0 : position;
        }

        /// <summary>
        /// Get size of offset mapping table (the size should be same as the content length).
        /// </summary>
        /// <returns>size of position mapping table</returns>
        public int GetOffsetMapSize()
        {
            return this.OffsetMap.Count;
        }
    }
}
