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

using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators
{
    /// <summary>Validatorの返すエラー情報</summary>
    public record ValidationError
    {
        /// <summary>ValidationName</summary>
        public string ValidationName { get; init; }
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
        /// <param name="validationName">The vadlidation name.</param>
        /// <param name="level">The level.</param>
        /// <param name="sentenceWithError">The sentence with error.</param>
        /// <param name="StartPosition">The start position.</param>
        /// <param name="EndPosition">The end position.</param>
        /// <param name="MessageArgs">The message args.</param>
        public ValidationError(
            string validationName,
            ValidationLevel level,
            Sentence sentenceWithError,
            LineOffset? StartPosition = null,
            LineOffset? EndPosition = null,
            object[]? MessageArgs = null,
            string MessageKey = "")
        {
            this.ValidationName = validationName;
            this.Level = level;
            this.Sentence = sentenceWithError;
            this.StartPosition = StartPosition;
            this.EndPosition = EndPosition;
            this.MessageArgs = MessageArgs ?? new object[0];
            this.MessageKey = MessageKey;
        }

        /// <summary>Get line number in which the error occurs.</summary>
        public int LineNumber => Sentence.LineNumber;

        /// <summary>Get column number in which the error occurs.</summary>
        public int StartColumnNumber => Sentence.StartPositionOffset;
    }
}
