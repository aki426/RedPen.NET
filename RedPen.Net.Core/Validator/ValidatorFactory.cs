using Microsoft.Extensions.Configuration;
using NLog;
using RedPen.Net.Core;
using RedPen.Net.Core.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Validator
{
    public class ValidatorFactory
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private static readonly string validatorPackage = typeof(Validator).Assembly.GetName().Name;

        private static readonly List<string> VALIDATOR_PACKAGES =
            new List<string> { validatorPackage, $"{validatorPackage}.sentence", $"{validatorPackage}.section" };

        // MEMO: MapはDictionaryに置き換え。LinkedHashMapは一旦Dictionaryに変更。
        private static readonly Dictionary<string, Validator> validators = new Dictionary<string, Validator>();

        private static readonly Dictionary<string, string> jsValidators = new Dictionary<string, string>();

        // C#で、引数にValidatorクラスを継承したクラスのみを取る関数を書いてください。
        //        private static void RegisterValidator<T>(T clazz) where T : Validator
        //        {
        //            TypeInfo typeInfo = typeof(clazz).GetTypeInfo();
        //            typeInfo.GetCustomAttribute
        //            // clazzインスタンスがObsolete属性を持っているかどうかを判定する。
        //            var deprecated = Attribute.GetCustomAttribute(element: typeof(clazz), typeof(ObsoleteAttribute)) != null;

        //            bool deprecated = clazz.GetCustomAttribute<ObsoleteAttribute>() == null ? false : true;

        //            bool deprecated = clazz.GetAnnotation(Deprecated.private class) == null ? false : true;

        //        if (deprecated) {
        //            LOG.warn(clazz.getName() + " is deprecated");
        //        }

        //    validators.put(clazz.getSimpleName().replace("Validator", ""), createValidator(clazz));
        //    }

        //static
        //{
        //    Reflections reflections = new Reflections("cc.redpen.validator");

        //    // register Validator implementations under cc.redpen.validator package
        //    reflections.getSubTypesOf(Validator.internal class).stream()

        //            .filter(validator-> !Modifier.isAbstract(validator.getModifiers()))
        //            .forEach(validator-> {
        //    try
        //    {
        //        registerValidator(validator);
        //    }
        //    catch (RuntimeException ignored)
        //    {
        //        // the validator doesn't implement default constructor
        //    }
        //});
        //Reflections jsReflections = new Reflections(
        //        new ConfigurationBuilder()
        //.setScanners(new ResourcesScanner())
        //                .setUrls(ClasspathHelper.forPackage("cc.redpen.validator")));
        //jsReflections.getResources(Pattern.compile(".*js"))
        //        .forEach(e-> {
        //    InputStream inputStream = ValidatorFactory.internal class.getResourceAsStream("/" + e);

        //try (InputStreamReader isr = new InputStreamReader(inputStream, Charset.forName("UTF-8"));
        //BufferedReader br = new BufferedReader(isr)) {
        //    StringBuilder sb = new StringBuilder(1024);
        //    String str;
        //    while ((str = br.readLine()) != null)
        //    {
        //        sb.append(str);
        //    }
        //    String validatorName = e.replaceFirst(".*/", "").replaceFirst("\\.js$", "");
        //    jsValidators.put(validatorName, sb.toString());
        //} catch (IOException ignored)
        //{
        //}
        //                });
        //    }

        public static List<ValidatorConfiguration> getConfigurations(string lang)
        {
            List<ValidatorConfiguration> configurations = validators.Where(e =>
            {
                List<string> supportedLanguages = e.Value.getSupportedLanguages();

                // MEMO: Validatorについて、JAVAではDeprecated属性、C#ではObsoleteAttributeを持っているかどうかを判定する。
                bool deprecated = e.Value.GetType().GetCustomAttributes(typeof(ObsoleteAttribute), false).Length == 0 ? false : true;

                return (!supportedLanguages.Any() || supportedLanguages.Contains(lang)) && !deprecated;
            }).Select(e => new ValidatorConfiguration(e.Key, ToStrings(e.Value.getProperties()))).ToList();

            Dictionary<string, string> emptyMap = new Dictionary<string, string>();
            //foreach (var jsValidator in jsValidators.Keys)
            //{
            //    try
            //    {
            //        Validator jsValidatorInstance = getInstance(jsValidator);
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

        //public static Validator getInstance(string validatorName)

        //{
        //    Configuration conf = Configuration.Builder().AddValidatorConfig(new ValidatorConfiguration(validatorName)).Build();

        //    return getInstance(conf.ValidatorConfigs[0], conf);
        //}

        //public static Validator getInstance(ValidatorConfiguration config, Configuration globalConfig)
        //{
        //    string validatorName = config.ConfigurationName;
        //    // lookup JavaScript validators
        //    string script = jsValidators[validatorName];
        //    if (script != null)
        //    {
        //        JavaScriptLoader javaScriptValidator = new JavaScriptLoader(validatorName, script);
        //        javaScriptValidator.PreInit(config, globalConfig);
        //        return javaScriptValidator;
        //    }

        //    // fallback to Java validators
        //    Validator prototype = validators.get(config.getConfigurationName());
        //    Class <? extends Validator > validatorClass = prototype != null ? prototype.getClass() : loadPlugin(validatorName);
        //    Validator validator = createValidator(validatorClass);
        //    validator.PreInit(config, globalConfig);
        //    return validator;
        //}

        //    @SuppressWarnings("unchecked")
        //    private static Class<? extends Validator> loadPlugin(String name) throws RedPenException
        //{
        //        for (String p : VALIDATOR_PACKAGES) {
        //        try
        //        {
        //            Class <? extends Validator > validatorClass = (Class)Class.forName(p + "." + name + "Validator");
        //            registerValidator(validatorClass);
        //            return validatorClass;
        //        }
        //        catch (ClassNotFoundException ignore)
        //        {
        //        }
        //    }
        //        throw new RedPenException("There is no such validator: " + name);
        //    }

        //    private static Validator createValidator(Class<? extends Validator> clazz)
        //{
        //    try
        //    {
        //        return clazz.newInstance();
        //    }
        //    catch (InstantiationException | IllegalAccessException e) {
        //        throw new RuntimeException("Cannot create instance of " + clazz + " using default constructor");
        //    }
        //    }
    }
}
