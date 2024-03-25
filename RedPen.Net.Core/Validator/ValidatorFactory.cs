using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NLog;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validator
{
    /// <summary>
    /// Validatorを一元管理するFactoryクラス
    /// </summary>
    public class ValidatorFactory
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private static readonly string validatorPackage = typeof(Validator).Assembly.GetName().Name;

        private static readonly List<string> VALIDATOR_PACKAGES = new List<string> {
            validatorPackage,
            $"{validatorPackage}.DocumentValidator",
            $"{validatorPackage}.SectionValidator",
            $"{validatorPackage}.SentenceValidator",
        };

        // MEMO: MapはDictionaryに置き換え。LinkedHashMapは一旦Dictionaryに変更。
        private static readonly Dictionary<string, Validator> validators = new Dictionary<string, Validator>();

        private static readonly Dictionary<string, string> jsValidators = new Dictionary<string, string>();

        /// <summary>
        /// Gets the validator types.
        /// </summary>
        /// <returns>A list of Types.</returns>
        public static List<Type> GetValidatorTypes() => Assembly.GetExecutingAssembly().GetTypes()
            .Where(i => typeof(Validator).IsAssignableFrom(i) && !i.IsAbstract).ToList();

        /// <summary>
        /// Registers the validator.
        /// </summary>
        /// <param name="t">The t.</param>
        private static void RegisterValidator(Type t)
        {
            // clazzインスタンスがObsolete属性を持っているかどうかを判定する。
            // TODO: Obsolete属性があっても登録は行う。これが期待する仕様かどうかは要検討。
            //Attribute.GetCustomAttribute(element: typeInfo, typeof(ObsoleteAttribute)) != null;
            if (t.IsDefined(typeof(ObsoleteAttribute), inherit: false))
            {
                LOG.Warn(t.Name + " is deprecated");
            }
            validators.Add(t.Name.Replace("Validator", ""), (Validator)t.Assembly.CreateInstance(t.FullName));
            //validators.Add(typeInfo.Name.Replace("Validator", ""), CreateValidator<T>(clazz));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorFactory"/> class.
        /// MEMO: JAVAのStatic Initializerの代わりにStaticコンストラクタを使用。挙動の変化をテストケースで抑止すること。
        /// </summary>
        static ValidatorFactory()
        {
            // Validatorの実装を登録する。
            foreach (Type validatorType in GetValidatorTypes())
            {
                try
                {
                    RegisterValidator(validatorType);
                }
                catch (SystemException ignored)
                {
                    // デフォルトコンストラクタがない場合は無視
                }
            }

            // *.jsファイルを読み込む
            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceNames = assembly.GetManifestResourceNames().Where(x => x.EndsWith(".js"));

            //foreach (var resourceName in resourceNames)
            //{
            //    using (var stream = assembly.GetManifestResourceStream(resourceName))
            //    using (var reader = new StreamReader(stream, Encoding.UTF8))
            //    {
            //        var content = reader.ReadToEnd();
            //        var validatorName = resourceName.Substring(resourceName.LastIndexOf('/') + 1).Replace(".js", "");
            //        jsValidators[validatorName] = content;
            //    }
            //}
        }

        /// <summary>
        /// gets the configurations.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns>A list of ValidatorConfigurations.</returns>
        public static List<ValidatorConfiguration> GetConfigurations(string lang)
        {
            List<ValidatorConfiguration> configurations = validators.Where(e =>
            {
                List<string> supportedLanguages = e.Value.getSupportedLanguages();

                // MEMO: Validatorについて、JAVAではDeprecated属性、C#ではObsoleteAttributeを持っているかどうかを判定する。
                //bool deprecated = e.Value.GetType().GetCustomAttributes(typeof(ObsoleteAttribute), false).Length == 0 ? false : true;
                bool deprecated = e.Value.GetType().IsDefined(typeof(ObsoleteAttribute), inherit: false);

                return (!supportedLanguages.Any() || supportedLanguages.Contains(lang)) && !deprecated;
            }).Select(e => new ValidatorConfiguration(e.Key, ToStrings(e.Value.getProperties()))).ToList();

            // TODO: JSのValidatorについては未実装。
            //Dictionary<string, string> emptyMap = new Dictionary<string, string>();
            //foreach (var jsValidator in jsValidators.Keys)
            //{
            //    try
            //    {
            //        Validator jsValidatorInstance = GetInstance(jsValidator);
            //        List<String> supportedLanguages = jsValidatorInstance.getSupportedLanguages();
            //        if (supportedLanguages.isEmpty() || supportedLanguages.contains(lang))
            //        {
            //            configurations.add(new ValidatorConfiguration(jsValidator, emptyMap));
            //        }
            //    }
            //    catch (RedPenException ignored)
            //    {
            //    }
            //}

            return configurations;
        }

        //    @SuppressWarnings("unchecked")
        /// <summary>
        /// Convert object Dictionary to string Dictionary.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>A Dictionary.</returns>
        private static Dictionary<string, string> ToStrings(Dictionary<string, object> properties)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> e in properties)
            {
                if (e.Value is IEnumerable)
                {
                    result.Add(e.Key, string.Join(",", e.Value as IEnumerable));
                }
                else
                {
                    result.Add(e.Key, e.Value.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="validatorName">The validator name.</param>
        /// <returns>A Validator.</returns>
        public static Validator GetInstance(string validatorName)

        {
            Configuration conf = Configuration.Builder().AddValidatorConfig(new ValidatorConfiguration(validatorName)).Build();

            return GetInstance(conf.ValidatorConfigs[0], conf);
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="globalConfig">The global config.</param>
        /// <returns>A Validator.</returns>
        public static Validator GetInstance(ValidatorConfiguration config, Configuration globalConfig)
        {
            string validatorName = config.ConfigurationName;
            // lookup JavaScript validators
            //string script = jsValidators[validatorName];
            //if (script != null)
            //{
            //    JavaScriptLoader javaScriptValidator = new JavaScriptLoader(validatorName, script);
            //    javaScriptValidator.PreInit(config, globalConfig);
            //    return javaScriptValidator;
            //}

            // fallback to validators
            Validator prototype = validators[config.ConfigurationName];

            Type validatorClass = prototype != null ? prototype.GetType() : LoadPlugin(validatorName);
            Validator validator = CreateValidator(validatorClass);
            validator.PreInit(config, globalConfig);
            return validator;
        }

        //    @SuppressWarnings("unchecked")
        /// <summary>
        /// loads the plugin.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A Type.</returns>
        private static Type LoadPlugin(string name)

        {
            foreach (string p in VALIDATOR_PACKAGES)
            {
                try
                {
                    Type validatorClass = Type.GetType($"{p}.{name}Validator");
                    RegisterValidator(validatorClass);
                    return validatorClass;
                }
                catch (TypeLoadException ignore)
                {
                }
            }
            throw new RedPenException("There is no such validator: " + name);
        }

        /// <summary>
        /// Creates the validator.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>A Validator.</returns>
        private static Validator CreateValidator(Type t)
        {
            try
            {
                return (Validator)t.Assembly.CreateInstance(t.FullName);
            }
            catch (Exception e)
            {
                if (e is MemberAccessException || e is FieldAccessException || e is MethodAccessException)
                {
                    throw new InvalidOperationException($"Cannot create instance of {t.Name} using default constructor");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
