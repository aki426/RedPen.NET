using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
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

        /// <summary>
        /// 引数を元情報としてエラーメッセージを生成する関数。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="cultureInfo">カルチャ指定でエラーメッセージの言語を切り替える。</param>
        /// <param name="messageArgs">The message args.</param>
        /// <returns>A string.</returns>
        public string GetErrorMessage(ValidationType type, string messageKey, CultureInfo cultureInfo, object[] messageArgs)
        {
            string suffix = messageKey == string.Empty ? "" : $"_{messageKey}";
            string key = $"{type.ValidationName()}{suffix}";
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
            return GetErrorMessage(error.Type, error.MessageKey, cultureInfo, error.MessageArgs);
        }
    }
}
