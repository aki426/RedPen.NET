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

using System;

namespace RedPen.Net.Core
{
    /// <summary>
    /// The red pen exception.
    /// </summary>
    public class RedPenException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="RedPenException"/> class.</summary>
        public RedPenException()
        {
            // MEMO: JAVA版ではsuper()が呼ばれているが、C#では引数なしコンストラクタは継承されて暗黙的に実行されるため不要。
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedPenException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public RedPenException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedPenException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RedPenException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedPenException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        public RedPenException(Exception? innerException) : base(innerException?.Message, innerException)
        {
            // MEMO: Exceptionをラップするだけの実装に意味があるのか不明だが、JAVA版に合わせて実装。
            // TODO: より適切な実装に変更する。
        }
    }
}
