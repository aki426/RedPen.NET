using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Gets the embedded resource.
        /// </summary>
        /// <returns>A list of string.</returns>
        public static List<string> GetEmbeddedResource()
        {
            List<string> list;

            // 現在実行しているアセンブリを取得する
            var assm = Assembly.GetExecutingAssembly();

            // アセンブリに埋め込まれているリソースのStreamを取得する
            // MEMO: ファイルの単純な埋め込みでは上手く動作しない。GetManifestResourceStreamのっ結果がnullになってしまう。
            using (var stream = assm.GetManifestResourceStream($"{assm.GetName()}.Resources.ParentDirectory.SampleText.ja.txt"))
            {
                if (stream == null)
                {
                    return new List<string>();
                }
                else
                {
                    // Streamの内容をすべて読み込んで標準出力に表示する
                    var reader = new StreamReader(stream);
                    list = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }

            return list;
        }

        public static string[] GetManifestResources()
        {
            // 現在実行しているアセンブリを取得する
            var assm = Assembly.GetExecutingAssembly();

            return assm.GetManifestResourceNames();
        }

        public static string GetSampleText()
        {
            return FileResource.SampleText_ja;
        }
    }
}
