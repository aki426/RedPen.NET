using redpen_core.parser;
using redpen_core.tokenizer;

namespace redpen_core.model
{
    /// <summary>Sentence block in a Document.</summary>
    public record class Sentence
    {
        // TODO: recordで実装したが、隠ぺいしたほうが良いものはアクセシビリティを変更する。

        private static readonly long serialVersionUID = 3761982769692999924L;

        public string Content { get; init; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        public int LineNumber { get; init; } // need getter. setterはJAVA版でも使用実績がないので不要。
        public bool IsFirstSentence { get; set; } // need getter.
        public List<string> Links { get; init; } // need getter. List.add()はJAVA版でも使用実績がある。
        public List<TokenElement> Tokens { get; set; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        public int StartPositionOffset { get; init; } // need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。
        public List<LineOffset> OffsetMap { get; init; }// need getter. setterはJAVA版で使用実績があるが必要に応じて対応する。

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
