using NLog;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    /// <summary>原文のテキスト位置と文字列の対応関係を記録した1センテンス分のオブジェクト。</summary>
    public record class Sentence
    {
        // TODO: recordで実装したが、隠ぺいしたほうが良いものはアクセシビリティを変更する。

        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        //private static readonly long serialVersionUID = 3761982769692999924L;

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
        /// lineNum行にオフセット位置0で開始しているSentenceを生成する。
        /// Initializes a new instance of the <see cref="Sentence"/> class.
        /// </summary>
        /// <param name="sentenceContent">The sentence content.</param>
        /// <param name="lineNum">The line num.</param>
        public Sentence(string sentenceContent, int lineNum) : this(sentenceContent, lineNum, 0)
        {
        }

        /// <summary>
        /// lineNum行の位置startOffsetで開始しているSentenceを生成する。
        /// Initializes a new instance of the <see cref="Sentence"/> class.
        /// </summary>
        /// <param name="sentenceContent">The sentence content.</param>
        /// <param name="lineNum">The line num.</param>
        /// <param name="startOffset">The start offset.</param>
        public Sentence(string sentenceContent, int lineNum, int startOffset)
        {
            this.Content = sentenceContent;
            //this.LineNumber = lineNum;
            this.IsFirstSentence = false;
            this.Links = new List<string>();
            this.Tokens = new List<TokenElement>();
            //this.StartPositionOffset = startOffset;

            // MEMO: Sentenceは1行内に収まっていることを前提にしている。
            this.OffsetMap = Enumerable.Range(startOffset, sentenceContent.Length).Select(offset => new LineOffset(lineNum, offset)).ToList();
        }

        // MEMO: 複数行に分割されたセンテンスはParserによってList<LineOffset> OffsetMapを計算され与えられる。

        /// <summary>
        /// 複数行にまたがるSentenceをサポートする、1文字ずつのLineOffsetを記録した完全なSentenceインスタンスを生成する。
        /// Initializes a new instance of the <see cref="Sentence"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="offsetMap">The offset map.</param>
        /// <param name="links">The links.</param>
        public Sentence(string content, List<LineOffset> offsetMap, List<string> links)
        {
            // MEMO: 位置が存在しないSentenceは存在しないという前提。
            if (!offsetMap.Any() || offsetMap.Count == 0)
            {
                string msg = "OffsetMap is empty. Sentence content: " + content;
                LOG.Error(msg);
                throw new System.ArgumentException(msg);
            }
            // MEMO: Contentの文字数とOffsetMapの要素数が一致していない場合はエラーとする。
            if (content.Count() != offsetMap.Count)
            {
                string msg = "Content length and OffsetMap size are different. Sentence content: " + content;
                LOG.Error(msg);
                throw new System.ArgumentException(msg);
            }

            this.Content = content;
            //this.LineNumber = offsetMap[0].LineNum;
            this.IsFirstSentence = false;
            this.Links = links;
            this.Tokens = new List<TokenElement>();
            //this.StartPositionOffset = offsetMap[0].Offset;
            this.OffsetMap = offsetMap;
        }

        ///// <summary>
        ///// Get offset position for specified character position.
        ///// </summary>
        ///// <param name="position">character position in a sentence</param>
        ///// <returns>offset position</returns>
        //public LineOffset? GetOffset(int position)
        //{
        //    if (position >= 0)
        //    {
        //        // MEMO: あらかじめContentの全文字とLineOffset（元テキストでの出現位置）の対応関係を表現した
        //        // List<LineOffset>が作成されており、Contentの文字数とList＜LineOffset>の要素数が同じに保たれていることが前提。
        //        if (position < this.OffsetMap.Count)
        //        {
        //            return this.OffsetMap[position];
        //        }
        //        else if ((position > 0) && (this.OffsetMap.Count == position))
        //        {
        //            // MEMO: Contentが空文字列ではない場合の末尾指定。改行かEOLのためのもの？
        //            LineOffset last = this.OffsetMap[this.OffsetMap.Count - 1];
        //            return last with { Offset = last.Offset + 1 };
        //        }
        //        else if (this.OffsetMap.Any())
        //        {
        //            LineOffset last = this.OffsetMap[this.OffsetMap.Count - 1];
        //            return last with { Offset = position - this.OffsetMap.Count }; // 改行に関して考慮するとこのようになる。
        //        }
        //        else
        //        {
        //            // MEMO: Contentの長さを超えてLineOffsetを返すことになるはず。
        //            return new LineOffset(this.LineNumber, position);
        //        }
        //    }

        //    return null;
        //}

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

        ///// <summary>
        ///// Get size of offset mapping table (the size should be same as the content length).
        ///// </summary>
        ///// <returns>size of position mapping table</returns>
        //public int GetOffsetMapSize()
        //{
        //    return this.OffsetMap.Count;
        //}
    }
}
