using System;
using System.Collections.Immutable;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;

namespace RedPen.Net.Core.Validators
{
    // TODO: recordか何かで書き直すことを検討する。
    public record ValidationError
    {
        public ValidationType Type { get; init; }
        public ValidationLevel Level { get; init; }
        public Sentence Sentence { get; init; }
        public LineOffset? StartPosition { get; init; }
        public LineOffset? EndPosition { get; init; }
        public object[] MessageArgs { get; init; }
        public string MessageKey { get; init; }

        public string Message { get; init; }
        //public string ValidationName { get; init; }

        public ValidationError(
            ValidationType type,
            // string validationName,
            string errorMessage,
            Sentence sentenceWithError,
            ValidationLevel level = ValidationLevel.ERROR)
        {
            this.Message = errorMessage;
            this.Type = type;
            //this.ValidationName = validationName; ;
            this.Sentence = sentenceWithError;
            this.StartPosition = null;
            this.EndPosition = null;
            this.Level = level;
        }

        internal ValidationError(
            ValidationType type,
            //string validatorName,
            string errorMessage,
            Sentence sentenceWithError,
            int startPosition,
            int endPosition,
            ValidationLevel Level = ValidationLevel.ERROR)
        {
            this.Message = errorMessage;
            this.Type = type;
            //this.ValidationName = validatorName;
            this.Sentence = sentenceWithError;
            this.StartPosition = sentenceWithError.GetOffset(startPosition) ?? throw new NullReferenceException("No value present");
            this.EndPosition = sentenceWithError.GetOffset(endPosition) ?? throw new NullReferenceException("No value present");
            this.Level = Level;
        }

        /// <summary>Get line number in which the error occurs.</summary>
        public int LineNumber => Sentence.LineNumber;

        /// <summary>Get column number in which the error occurs.</summary>
        public int StartColumnNumber => Sentence.StartPositionOffset;
    }
}
