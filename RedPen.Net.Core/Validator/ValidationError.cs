using System;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;

namespace RedPen.Net.Core.Validator
{
    // TODO: recordか何かで書き直すことを検討する。
    [Serializable]
    [ToString]
    public class ValidationError
    {
        private static readonly long serialVersionUID = -1273191135155157144L;

        private readonly string message;
        private readonly string validatorName;
        private readonly Sentence sentence;
        private readonly LineOffset startPosition;
        private readonly LineOffset endPosition;
        private readonly Level level;

        /**
         * Constructor.
         *
         * @param validatorName    validator name
         * @param errorMessage      error message
         * @param sentenceWithError sentence containing validation error
         */

        public ValidationError(string validatorName, string errorMessage, Sentence sentenceWithError)
            : this(validatorName, errorMessage, sentenceWithError, Level.ERROR)
        {
        }

        /**
         * Constructor.
         *
         * @param validatorName    validator name
         * @param errorMessage      error message
         * @param sentenceWithError sentence containing validation error
         */

        public ValidationError(
            string validatorName,
            string errorMessage,
            Sentence sentenceWithError,
            Level level)
        {
            this.message = errorMessage;
            this.validatorName = validatorName; ;
            this.sentence = sentenceWithError;
            this.startPosition = null;
            this.endPosition = null;
            this.level = level;
        }

        /**
         * Constructor.
         *
         * @param validatorName    validator name
         * @param errorMessage      error message
         * @param sentenceWithError sentence containing validation error
         * @param startPosition     position where error starts
         * @param endPosition       position where error ends
         */

        private ValidationError(
            string validatorName,
            string errorMessage,
            Sentence sentenceWithError,
            int startPosition,
            int endPosition)
            : this(
                  validatorName,
                  errorMessage,
                  sentenceWithError,
                  startPosition,
                  endPosition,
                  Level.ERROR)
        {
        }

        /**
         * Constructor.
         *
         * @param validatorName    validator name
         * @param errorMessage      error message
         * @param sentenceWithError sentence containing validation error
         * @param startPosition     position where error starts
         * @param endPosition       position where error ends
         */

        internal ValidationError(
            string validatorName,
            string errorMessage,
            Sentence sentenceWithError,
            int startPosition,
            int endPosition,
            Level level)
        {
            this.message = errorMessage;
            this.validatorName = validatorName;
            this.sentence = sentenceWithError;
            this.startPosition = sentenceWithError.GetOffset(startPosition) ?? throw new NullReferenceException("No value present");
            this.endPosition = sentenceWithError.GetOffset(endPosition) ?? throw new NullReferenceException("No value present");
            this.level = level;
        }

        /**
         * Constructor.
         *
         * @param validatorClass    validator class
         * @param errorMessage      error message
         * @param sentenceWithError sentence containing validation error
         * @param startPosition     position where error starts
         * @param endPosition       position where error ends
         * @deprecated
         */

        private ValidationError(
            Type validatorClass,
            string errorMessage,
            Sentence sentenceWithError,
            LineOffset startPosition,
            LineOffset endPosition)
        {
            this.message = errorMessage;
            this.validatorName = validatorClass.Name;
            this.sentence = sentenceWithError;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.level = Level.ERROR;
        }

        /// <summary>Get line number in which the error occurs.</summary>
        public int LineNumber => sentence.LineNumber;

        /// <summary>Get error message.</summary>
        public string Message => message;

        /// <summary>Get column number in which the error occurs.</summary>
        public int StartColumnNumber => sentence.StartPositionOffset;

        /// <summary>Get sentence containing the error.</summary>
        public Sentence Sentence => sentence;

        /// <summary>Get validator name.</summary>
        public string ValidatorName
        {
            get
            {
                if (validatorName.EndsWith("Validator"))
                {
                    return validatorName.Substring(0, validatorName.Length - "Validator".Length);
                }
                else
                {
                    return validatorName;
                }
            }
        }

        /// <summary>Get error start position.</summary>>
        public LineOffset? StartPosition => startPosition;

        /// <summary>Get error end position.</summary>
        public LineOffset? EndPosition => endPosition;

        /// <summary>Get error level.</summary>
        public Level Level => Level;
    }
}
