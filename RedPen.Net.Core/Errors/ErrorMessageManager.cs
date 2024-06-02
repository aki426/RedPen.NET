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

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core.Errors
{
    /// <summary>
    /// エラーメッセージの生成を引き受けるクラス。エラーメッセージはリソースで定義されており環境依存なのでSingleton。
    /// </summary>
    public class ErrorMessageManager
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private static ErrorMessageManager instance;

        /// <summary>Singletonのインスタンスを取得する。</summary>
        /// <returns>An ErrorMessageManager.</returns>
        public static ErrorMessageManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ErrorMessageManager();
            }

            return instance;
        }

        private ImmutableList<ErrorMessageDefinition> ErrorMessageDefinitions = null;

        public void SetValidatorDefinition(ImmutableList<ErrorMessageDefinition> errorMessageDefinitions)
        {
            ErrorMessageDefinitions = errorMessageDefinitions;
        }

        /// <summary>
        /// 引数を元情報としてエラーメッセージを生成する関数。
        /// </summary>
        /// <param name="validationName">The type.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="cultureInfo">カルチャ指定でエラーメッセージの言語を切り替える。</param>
        /// <param name="messageArgs">The message args.</param>
        /// <returns>A string.</returns>
        public string GetErrorMessage(string validationName, string messageKey, CultureInfo cultureInfo, object[] messageArgs)
        {
            // 先に定義されたエラーメッセージを検索し、あればそれを優先する。
            if (ErrorMessageDefinitions != null && ErrorMessageDefinitions.Any())
            {
                var definition = ErrorMessageDefinitions.Where(e => e.Match(validationName, messageKey, cultureInfo)).FirstOrDefault();
                if (definition != null)
                {
                    return definition.GetFormatedMessage(messageArgs);
                }
            }

            // Coreライブラリ内のリソースからエラーメッセージを取得する。
            string suffix = messageKey == string.Empty ? "" : $"_{messageKey}";
            string key = $"{validationName}{suffix}";
            string pattern = ValidationMessage.ResourceManager.GetString(key, cultureInfo);

            return string.Format(cultureInfo, pattern, messageArgs);
        }

        /// <summary>
        /// 引数を元情報としてエラーメッセージを生成する関数。
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="cultureInfo">カルチャ指定でエラーメッセージの言語を切り替える。</param>
        /// <returns>A string.</returns>
        public string GetErrorMessage(ValidationError error, CultureInfo cultureInfo)
        {
            return GetErrorMessage(error.ValidationName, error.MessageKey, cultureInfo, error.MessageArgs);
        }
    }
}
