using System.Globalization;
using System.Threading;

namespace VerifyBasicFunction
{
    /// <summary>
    /// The resource sample model.
    /// </summary>
    public static class ResourceSampleModel
    {
        /// <summary>
        /// Gets the hello message.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>A string.</returns>
        public static string GetHelloMessage(CultureInfo cultureInfo)
        {
            // 英語
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            return ErrorMessage.HelloWorld;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>A string.</returns>
        public static string GetMessage(string propertyName, CultureInfo cultureInfo)
        {
            // propertyNameで渡された文字列をキーとして、ErrorMessageクラスのプロパティを取得する
            return ErrorMessage.ResourceManager.GetString(propertyName, cultureInfo);
        }
    }
}
