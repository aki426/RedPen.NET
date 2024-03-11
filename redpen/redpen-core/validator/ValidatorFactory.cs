using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using J2N.Collections.ObjectModel;
using NLog;
using redpen_core;
using redpen_core.config;
using redpen_core.validator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace redpen_core.validator
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
        private static void RegisterValidator<T>(T clazz) where T : Validator
        {
            TypeInfo typeInfo = typeof(clazz).GetTypeInfo();
            typeInfo.GetCustomAttribute
            // clazzインスタンスがObsolete属性を持っているかどうかを判定する。
            var deprecated = Attribute.GetCustomAttribute(element: typeof(clazz), typeof(ObsoleteAttribute)) != null;

            bool deprecated = clazz.GetCustomAttribute<ObsoleteAttribute>() == null ? false : true;

            bool deprecated = clazz.GetAnnotation(Deprecated.private class) == null ? false : true;

        if (deprecated) {
            LOG.warn(clazz.getName() + " is deprecated");
        }

    validators.put(clazz.getSimpleName().replace("Validator", ""), createValidator(clazz));
    }

static
{
    Reflections reflections = new Reflections("cc.redpen.validator");

    // register Validator implementations under cc.redpen.validator package
    reflections.getSubTypesOf(Validator.internal class).stream()

            .filter(validator-> !Modifier.isAbstract(validator.getModifiers()))
            .forEach(validator-> {
    try
    {
        registerValidator(validator);
    }
    catch (RuntimeException ignored)
    {
        // the validator doesn't implement default constructor
    }
});
Reflections jsReflections = new Reflections(
        new ConfigurationBuilder()
.setScanners(new ResourcesScanner())
                .setUrls(ClasspathHelper.forPackage("cc.redpen.validator")));
jsReflections.getResources(Pattern.compile(".*js"))
        .forEach(e-> {

    InputStream inputStream = ValidatorFactory.internal class.getResourceAsStream("/" + e);

try (InputStreamReader isr = new InputStreamReader(inputStream, Charset.forName("UTF-8"));
BufferedReader br = new BufferedReader(isr)) {
    StringBuilder sb = new StringBuilder(1024);
    String str;
    while ((str = br.readLine()) != null)
    {
        sb.append(str);
    }
    String validatorName = e.replaceFirst(".*/", "").replaceFirst("\\.js$", "");
    jsValidators.put(validatorName, sb.toString());
} catch (IOException ignored)
{
}
                });
    }

    public static List<ValidatorConfiguration> getConfigurations(String lang)
{
    List<ValidatorConfiguration> configurations = validators.entrySet().stream().filter(e-> {
        List<String> supportedLanguages = e.getValue().getSupportedLanguages();

        boolean deprecated = e.getValue().getClass().getAnnotation(Deprecated.internal class) == null ? false : true;

return (supportedLanguages.isEmpty() || supportedLanguages.contains(lang)) && !deprecated;
        }).map(e-> new ValidatorConfiguration(e.getKey(), toStrings(e.getValue().getProperties()))).collect(toList());
Map<String, String> emptyMap = new LinkedHashMap<>();
for (String jsValidator : jsValidators.keySet())
{
    try
    {
        Validator jsValidatorInstance = getInstance(jsValidator);
        List<String> supportedLanguages = jsValidatorInstance.getSupportedLanguages();
        if (supportedLanguages.isEmpty() || supportedLanguages.contains(lang))
        {
            configurations.add(new ValidatorConfiguration(jsValidator, emptyMap));
        }
    }
    catch (RedPenException ignored)
    {
    }
}
return configurations;
    }

    @SuppressWarnings("unchecked")
    static Map<String, String> toStrings(Map<String, Object> properties)
{
    Map<String, String> result = new LinkedHashMap<>();
    for (Map.Entry<String, Object> e : properties.entrySet()) {
    if (e.getValue() instanceof Iterable)
                result.put(e.getKey(), join((Iterable)e.getValue(), ','));
            else
        result.put(e.getKey(), e.getValue().toString());
}
return result;
    }

    public static Validator getInstance(String validatorName) throws RedPenException
{
    Configuration conf = Configuration.builder().addValidatorConfig(new ValidatorConfiguration(validatorName)).build();
return getInstance(conf.getValidatorConfigs().get(0), conf);
    }

    public static Validator getInstance(ValidatorConfiguration config, Configuration globalConfig) throws RedPenException
{
    String validatorName = config.getConfigurationName();
    // lookup JavaScript validators
    String script = jsValidators.get(validatorName);
    if (script != null)
    {
        JavaScriptLoader javaScriptValidator = new JavaScriptLoader(validatorName, script);
        javaScriptValidator.preInit(config, globalConfig);
        return javaScriptValidator;
    }

    // fallback to Java validators
    Validator prototype = validators.get(config.getConfigurationName());
    Class <? extends Validator > validatorClass = prototype != null ? prototype.getClass() : loadPlugin(validatorName);
    Validator validator = createValidator(validatorClass);
    validator.preInit(config, globalConfig);
    return validator;
    }

    @SuppressWarnings("unchecked")
    private static Class<? extends Validator> loadPlugin(String name) throws RedPenException
{
        for (String p : VALIDATOR_PACKAGES) {
        try
        {
            Class <? extends Validator > validatorClass = (Class)Class.forName(p + "." + name + "Validator");
            registerValidator(validatorClass);
            return validatorClass;
        }
        catch (ClassNotFoundException ignore)
        {
        }
    }
        throw new RedPenException("There is no such validator: " + name);
    }

    private static Validator createValidator(Class<? extends Validator> clazz)
{
    try
    {
        return clazz.newInstance();
    }
    catch (InstantiationException | IllegalAccessException e) {
        throw new RuntimeException("Cannot create instance of " + clazz + " using default constructor");
    }
    }
}
}
