using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Errors
{
    public class ErrorMessageManager
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private static ErrorMessageManager instance;

        public static ErrorMessageManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ErrorMessageManager();
            }

            return instance;
        }

        public string GetErrorMessage(ValidationType type, string messageKey, CultureInfo cultureInfo, object[] messageArgs)
        {
            string suffix = messageKey == string.Empty ? "" : $"_{messageKey}";
            string key = $"{type.ValidationName()}{suffix}";
            string pattern = ValidationMessage.ResourceManager.GetString(key, cultureInfo);

            return string.Format(cultureInfo, pattern, messageArgs);
        }
    }
}
