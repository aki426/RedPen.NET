using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using RedPen.Net.Core.Config;

namespace RedPen.Net.Core.Validators
{
    /// <summary>
    /// Validatorを一元管理するFactoryクラス
    /// </summary>
    public class ValidatorFactory
    {
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>"RedPen.Net.Core.Validator"</summary>
        private static readonly string validatorPackage = typeof(Validator).Assembly.GetName().Name;

        /// <summary>クラス検索対象NameSpaceとして登録されたValidatorフォルダと配下のフォルダ</summary>
        private static readonly ImmutableList<string> VALIDATOR_PACKAGES = ImmutableList.Create(
            validatorPackage,
            $"{validatorPackage}.DocumentValidator",
            $"{validatorPackage}.SectionValidator",
            $"{validatorPackage}.SentenceValidator"
        );

        // MEMO: MapはDictionaryに置き換え。LinkedHashMapは一旦Dictionaryに変更。
        /// <summary>Validatorを実装したクラスのインスタンスを名前をキーとしたDictionaryに詰め込んだもの。
        /// キーであるValidator名はクラス名から文字列"Validator"を除いたもの。</summary>
        private static readonly Dictionary<string, Validator> validators = new Dictionary<string, Validator>();

        // TODO: JSのValidatorについては未実装。
        ///// <summary>Javascriptで実装されたValidatorを名前をキーとしてDictionaryに詰め込んだもの。</summary>
        //private static readonly Dictionary<string, string> jsValidators = new Dictionary<string, string>();

        /// <summary>Reflectionで取得したValidatorクラスを実装した具象クラスのTypeリスト</summary>
        public static List<Type> ValidatorTypes => Assembly.GetExecutingAssembly().GetTypes()
            .Where(i => typeof(Validator).IsAssignableFrom(i) && !i.IsAbstract).ToList();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorFactory"/> class.
        /// MEMO: JAVAのStatic Initializerの代わりにStaticコンストラクタを使用。挙動の変化をテストケースで抑止すること。
        /// </summary>
        static ValidatorFactory()
        {
            // Validatorの実装を登録する。
            foreach (Type validatorType in ValidatorTypes)
            {
                try
                {
                    RegisterValidator(validatorType);
                }
                catch (Exception ignored)
                {
                    // MEMO: JAVA版は次のコメントがあるのみで、Static Initializer内でのValidatorロード時
                    // 例外発生したValidatorは無視して処理を続ける仕様。
                    // the validator doesn't implement default constructor

                    // MEMO: JAVA版は非推奨のnewInstance関数を使用していたこともあり、例外のフィルタリングをして
                    // throwする例外と握りつぶす例外を区別していたのかどうか意図が不明。
                    // C#版ではValidatorの生成および登録全体で失敗した場合はログのみ出して処理を続ける仕様とする。
                    LOG.Warn(validatorType.Name + " is not registered");
                }
            }

            // TODO: JSのValidatorについては未実装。
        }

        /// <summary>
        /// Validator名からValidatorクラスのインスタンスを返す関数。
        /// </summary>
        /// <param name="validatorName">The validator name.</param>
        /// <returns>A Validator.</returns>
        public static Validator GetInstance(string validatorName)
        {
            // TODO: 普通にvalidatorsから取得するだけのコードに切り替えて良いかどうか要検討。
            Configuration conf = Configuration.Builder().AddValidatorConfig(new ValidatorConfiguration(validatorName)).Build();

            return GetInstance(conf.ValidatorConfigurations[0], conf);
        }

        /// <summary>
        /// ConfigurationからValidatorクラスのインスタンスを返す関数。
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="globalConfig">The global config.</param>
        /// <returns>A Validator.</returns>
        public static Validator GetInstance(ValidatorConfiguration config, Configuration globalConfig)
        {
            // MEMO: Configurationは必ずXXXConfigurationの形式で名前がつけられているため、
            // XXX部分を取り出してValidator名として扱う。
            string validatorName = config.GetType().Name.Replace("Configuration", "");

            // lookup JavaScript validators
            //string script = jsValidators[validatorName];
            //if (script != null)
            //{
            //    JavaScriptLoader javaScriptValidator = new JavaScriptLoader(validatorName, script);
            //    javaScriptValidator.PreInit(config, globalConfig);
            //    return javaScriptValidator;
            //}

            // fallback to validators
            Validator prototype = validators[config.GetType().Name.Replace("Configuration", "")];

            Type validatorClass = prototype != null ? prototype.GetType() : LoadPlugin(validatorName);

            // MEMO: GetInstanceでは都度CreateValidatorを実行して新規インスタンスを返している。
            // validators内に存在するValidatorのインスタンスが使用されることはなく、
            // Factoryメソッドの仕組みとしてこれで良いか再考の余地あり。DIフレームワークでも使うか？
            Validator validator = CreateValidator(validatorClass);
            validator.PreInit(config, globalConfig);
            return validator;
        }

        //    @SuppressWarnings("unchecked")
        /// <summary>
        /// GetInstance関数から呼ばれる、アプリ実行中の動的なValidatorロードのための関数？
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
        /// TypeからValidatorのインスタンスを生成して返す関数。Exception発生時はログを出力して例外をスローする。
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
                    LOG.Error(e);
                    throw new InvalidOperationException($"Cannot create instance of {t.Name} using default constructor");
                }
                else
                {
                    LOG.Error(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Registers the validator.
        /// </summary>
        /// <param name="t">The t.</param>
        private static void RegisterValidator(Type t)
        {
            // Type tのクラスがObsolete属性を持っているかどうかを判定し、ログを出力する。
            // Obsolete属性があってもvalidators Dictionaryへ登録する。
            // TODO: Obsolete属性のValidatorを今後も使用することが、期待する仕様かどうかは要検討。
            if (t.IsDefined(typeof(ObsoleteAttribute), inherit: false))
            {
                LOG.Warn(t.Name + " is deprecated");
            }

            // MEMO: CreateValidatorで例外が発生する可能性があるが、JAVA版が握りつぶさない仕様だったのでC#でも踏襲。
            // TODO: ロードできないValidatorがある場合、そのValidatorを無視して続けた方が良いかどうかは要検討。
            validators.Add(t.Name.Replace("Validator", ""), CreateValidator(t));
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

                // MEMO: サポート言語が未指定の場合は全言語対応とみなす。
                // また、サポート言語がある場合は指定のlangに対応しているかどうかとdeprecatedではないことが条件となる。
                // TODO: Deprecated属性のValidatorに関してPropertyが取得できないことになるが、これが期待する仕様かどうかは要検討。
                return (!supportedLanguages.Any() || supportedLanguages.Contains(lang)) && !deprecated;
                // }).Select(e => new ValidatorConfiguration(e.Key, ToStrings(e.Value.getProperties()))).ToList();
                // TODO: 一旦ビルドを通すために暫定的にValidatorConfigurationを生成。
            }).Select(e => new ValidatorConfiguration(ValidationLevel.ERROR)).ToList();

            // TODO: JSのValidatorについては未実装。

            return configurations;
        }

        ////    @SuppressWarnings("unchecked")
        ///// <summary>
        ///// Convert object Dictionary to string Dictionary.
        ///// TODO: JAVA版ではObject型を多用するスタイルだったが、C#版でもそれでよいのか、
        ///// 実体がstringなのでstringでの運用に切り替えたほうが良いのかは要検討。
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <returns>A Dictionary.</returns>
        //private static Dictionary<string, string> ToStrings(Dictionary<string, object> properties)
        //{
        //    Dictionary<string, string> result = new Dictionary<string, string>();
        //    foreach (KeyValuePair<string, object> e in properties)
        //    {
        //        if (e.Value is IEnumerable)
        //        {
        //            result.Add(e.Key, string.Join(",", e.Value as IEnumerable));
        //        }
        //        else
        //        {
        //            result.Add(e.Key, e.Value.ToString());
        //        }
        //    }

        //    return result;
        //}
    }
}
