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

using System.Globalization;

namespace RedPen.Net.Core.Errors
{
    /// <summary>応用アプリケーション側でValidatorを追加する場合に使用するErrorMessageの定義</summary>
    public record ErrorMessageDefinition
    {
        /// <summary>エラーメッセージに対応するValidationName</summary>
        public string ValidationName { get; init; }
        /// <summary>エラーメッセージに対応するMessageKey</summary>
        public string MessageKey { get; init; }
        /// <summary>エラーメッセージの言語</summary>
        public CultureInfo CultureInfo { get; init; }
        /// <summary>エラーメッセージのテンプレート文</summary>
        public string Message { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessageDefinition"/> class.
        /// </summary>
        /// <param name="validationName">The validation name.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="message">The message.</param>
        public ErrorMessageDefinition(string validationName, string messageKey, CultureInfo cultureInfo, string message)
        {
            this.ValidationName = validationName;
            this.MessageKey = messageKey;
            this.CultureInfo = cultureInfo;
            this.Message = message;
        }

        /// <summary>
        /// エラーメッセージ定義が指定された条件に一致するかどうかを判定する。
        /// ValidationName、Validationに対して複数パターンのメッセージが存在する場合はmessageKey、そしてメッセージの言語設定で判定する。
        /// </summary>
        /// <param name="validationName">The validation name.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>A bool.</returns>
        public bool Match(string validationName, string messageKey, CultureInfo cultureInfo)
        {
            return this.ValidationName == validationName && this.MessageKey == messageKey && this.CultureInfo.Equals(cultureInfo);
        }

        /// <summary>
        /// Validatorがメッセージに回す引数として検出した引数をMessageに適用する。
        /// </summary>
        /// <param name="messageArgs">The message args.</param>
        /// <returns>A string.</returns>
        public string GetFormatedMessage(object[] messageArgs)
        {
            return string.Format(CultureInfo, Message, messageArgs);
        }
    }
}
