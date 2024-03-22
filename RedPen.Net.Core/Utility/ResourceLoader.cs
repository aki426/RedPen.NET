using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace RedPen.Net.Core.Utility
{
    public class ResourceLoader
    {
        private readonly ResourceManager _resourceManager;

        public ResourceLoader()
        {
            _resourceManager = new ResourceManager("SentenceValidator.error-messages", Assembly.GetExecutingAssembly());
        }

        public string GetErrorMessage(string key, CultureInfo cultureInfo)
        {
            string resourceFile = _resourceManager.GetString(key, cultureInfo);
            if (string.IsNullOrEmpty(resourceFile))
            {
                // 指定された言語のリソースが見つからない場合は、デフォルト(英語)のリソースを使用する
                resourceFile = _resourceManager.GetString(key, CultureInfo.InvariantCulture);
            }
            return resourceFile;
        }
    }
}
