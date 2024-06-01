using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators
{
    /// <summary>Validatorの返すエラー情報</summary>
    public record ValidationError
    {
        /// <summary>Validationタイプ</summary>
        public ValidationType Type { get; init; }
        /// <summary>エラーレベル</summary>
        public ValidationLevel Level { get; init; }
        /// <summary>エラー発生したセンテンス。</summary>
        public Sentence Sentence { get; init; }
        /// <summary>エラー開始位置</summary>
        public LineOffset? StartPosition { get; init; }
        /// <summary>エラー終了位置</summary>
        public LineOffset? EndPosition { get; init; }
        /// <summary>Message生成のための引数。MEMO: あらかじめValidatorがエラーメッセージと対応した順番で要素を格納する。</summary>
        public object[] MessageArgs { get; init; }
        /// <summary>1つのValidation内で複数のMessageがある場合にどれを使用するか指定するための追加Key。</summary>
        public string MessageKey { get; init; }

        /// <summary>
        /// センテンス内の開始終了位置をLineOffset型で与えることができる<see cref="ValidationError"/>コンストラクタ。class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="level">The level.</param>
        /// <param name="sentenceWithError">The sentence with error.</param>
        /// <param name="StartPosition">The start position.</param>
        /// <param name="EndPosition">The end position.</param>
        /// <param name="MessageArgs">The message args.</param>
        public ValidationError(
            ValidationType type,
            ValidationLevel level,
            Sentence sentenceWithError,
            LineOffset? StartPosition = null,
            LineOffset? EndPosition = null,
            object[] MessageArgs = null,
            string MessageKey = "")
        {
            this.Type = type;
            this.Level = level;
            this.Sentence = sentenceWithError;
            this.StartPosition = StartPosition;
            this.EndPosition = EndPosition;
            this.MessageArgs = MessageArgs ?? new object[0];
            this.MessageKey = MessageKey;
        }

        ///// <summary>
        ///// センテンス内の開始終了位置をintで与えることができる<see cref="ValidationError"/>コンストラクタ。class.
        ///// </summary>
        ///// <param name="type">The type.</param>
        ///// <param name="Level">The level.</param>
        ///// <param name="sentenceWithError">The sentence with error.</param>
        ///// <param name="startPosition">The start position.</param>
        ///// <param name="endPosition">The end position.</param>
        ///// <param name="MessageArgs">The message args.</param>
        //internal ValidationError(
        //    ValidationType type,
        //    ValidationLevel Level,
        //    Sentence sentenceWithError,
        //    int startPosition,
        //    int endPosition,
        //    object[] MessageArgs = null,
        //    string MessageKey = "") :
        //    this(type, Level, sentenceWithError, null, null, MessageArgs, MessageKey)
        //{
        //    this.StartPosition = sentenceWithError.GetOffset(startPosition) ?? throw new NullReferenceException("No value present");
        //    this.EndPosition = sentenceWithError.GetOffset(endPosition) ?? throw new NullReferenceException("No value present");
        //}

        /// <summary>Get line number in which the error occurs.</summary>
        public int LineNumber => Sentence.LineNumber;

        /// <summary>Get column number in which the error occurs.</summary>
        public int StartColumnNumber => Sentence.StartPositionOffset;
    }
}
