using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validator
{
    // MEMO: 【抽象クラスの関数について】
    // JAVAの場合、仮想関数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // final：仮想関数でなくなりオーバーライド不可、abstract：オーバーライド必須の抽象メソッド、それ以外：オーバーライド可能（@Overrideが必要）の仮想メソッド
    // C#の場合、仮想関数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // キーワード無し：オーバーライド不可、abstract：オーバーライド必須の抽象メソッド、virtual：オーバーライド可能の仮想メソッド
    // つまりJAVAでfinalがついている関数はC#ではキーワード無し、キーワード無しのものはvirtualをつける。

    // MEMO: 【抽象クラスのメンバー変数について】
    // JAVAの場合、メンバー変数の扱いは、Interfaceおよび抽象クラス内では次のとおり。
    // final：コンストラクタまたはstatic initializerで初期化必須、継承先の具象クラスからはアクセスできますが、オーバーライド(上書き)することはできません。
    // finalキーワードは、その変数が最終的な値を持つことを意味しています。finalな変数は一度初期化されると、その値を変更することはできません。
    // private：オーバーライド不可、abstract：オーバーライド必須、それ以外：オーバーライド可能
    // キーワード無し：これらの変数は、抽象クラスのインスタンスに所属します。抽象クラスそのものはインスタンス化できませんが、具象クラスのインスタンスを介してアクセスできます。
    // static：具象クラスにおけるStaticメンバー変数と同じ。
    // final：finalメンバー変数は、コンストラクタまたはスタティックイニシャライザで初期化する必要があります。
    // private：抽象クラス内でのみアクセス可能です。具象クラスからはアクセスできません。
    // protected：同一パッケージ内の他のクラスや、抽象クラスを継承した具象クラスからアクセス可能です。
    // public：どこからでもアクセス可能です。

    // MEMO: JAVAのInterfaceはC#と異なり、具象メソッドとメンバー変数を持つことができる。

    /// <summary>
    /// The validator.
    /// </summary>
    public abstract class Validator
    {
        /// <summary>Nlog</summary>
        private static Logger LOG = LogManager.GetCurrentClassLogger();

        // TODO: おそらく多言語対応のためのコードでありC#で対応する実装をする。C#ではResources.resxやCultureInfoを使う。
        // private final static ResourceBundle.Control fallbackControl = ResourceBundle.Control.getNoFallbackControl(FORMAT_DEFAULT);

        private Dictionary<string, object> defaultProps;

        // TODO: おそらく多言語対応のためのコードでありC#で対応する実装をする。C#ではResources.resxやCultureInfoを使う。
        private ResourceManager errorMessages = null;

        // MEMO: JAVAのprotectedはC#のprotected internalに相当する。protectedだとアセンブリ内からアクセスできない。
        protected internal ValidatorConfiguration config;

        // MEMO: JAVAのprotectedはC#のprotected internalに相当する。
        protected internal Configuration globalConfig;

        // MEMO: JAVAのLocaleはC#のCultureInfoに相当する。
        // MEMO: デフォルトのCultureはコンストラクタ内で与えられる。
        public CultureInfo Locale { get; private set; }

        // TODO: validatorNameはイコールクラス名。
        protected string validatorName;

        // MEMO: JAVAのキーワード無しコンストラクターはC#でもキーワード無しで良いはず。

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        public Validator() : this(new object[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Validator"/> class.
        /// </summary>
        /// <param name="keyValues">String key and Object value pairs for supported config Properties.</param>
        public Validator(object[] keyValues)
        {
            validatorName = this.GetType().Name;

            // MEMO: デフォルトのCultureInfoはDefaultThreadCurrentCultureに設定される。
            // TODO: デフォルトCultureの設定方法がこれで良いかはライブラリ全体の仕様を見て再検討する。
            setLocale(CultureInfo.DefaultThreadCurrentCulture);
            setDefaultProperties(keyValues);
        }

        /// <summary>
        /// sets the default properties.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        protected internal void setDefaultProperties(object[] keyValues)
        {
            defaultProps = new Dictionary<string, object>();
            addDefaultProperties(keyValues);
        }

        /// <summary>
        /// adds the default properties.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        protected internal void addDefaultProperties(object[] keyValues)
        {
            if (keyValues.Length % 2 != 0)
            {
                throw new ArgumentException("Not enough values specified");
            }

            for (int i = 0; i < keyValues.Length; i += 2)
            {
                defaultProps.Add(keyValues[i].ToString(), keyValues[i + 1]);
            }
        }

        private List<ValidationError> errors;

        /// <summary>
        /// sets the error list.
        /// TODO: プロパティへの変更を検討する。
        /// </summary>
        /// <param name="errors">The errors.</param>
        public void setErrorList(List<ValidationError> errors)
        {
            this.errors = errors;
        }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="sentence"></param>
        public virtual void PreValidate(Sentence sentence)
        { }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="section"></param>
        public virtual void PreValidate(Section section)
        { }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="document"></param>
        public virtual void PreValidate(Document document)
        { }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="document"></param>
        public virtual void Validate(Document document)
        { }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="sentence"></param>
        public virtual void Validate(Sentence sentence)
        { }

        /// <summary>
        /// Process input blocks before Run validation. This method is used to store the information needed to Run Validator before the validation process.
        /// </summary>
        /// <param name="section"></param>
        public virtual void Validate(Section section)
        { }

        /// <summary>
        /// Return an array of languages supported by this validator
        /// {@link cc.redpen.validator.Validator} provides empty implementation.
        /// Validator implementation validates sections can override this method.
        /// </summary>
        /// <returns>an array of the languages supported by this validator.
        /// An empty list implies there are no restrictions on the languages supported by this validator.</returns>
        public virtual List<string> getSupportedLanguages()
        {
            return new List<string>();
        }

        public virtual void PreInit(ValidatorConfiguration config, Configuration globalConfig)

        {
            this.config = config;
            this.globalConfig = globalConfig;
            Init();
        }

        /// <summary>
        /// Localeをセットするとともに、Locale情報に対応するリソースを引き当てる。
        /// TODO: プロパティへの変更を検討する。
        /// </summary>
        /// <param name="locale">The Locale.</param>
        public void setLocale(CultureInfo locale)
        {
            this.Locale = locale;

            // MEMO: Validatorが返す文章エラーの説明メッセージはValidationMessage.resxにまとめた。
            // JAVA版ではリソースをNameSpaceごとに分割していたが、C#版では分ける意義が薄いと判断し
            // すべてのValidatorのエラーメッセージを1つのリソースマネージャで管理するようにした。
            // これによってパッケージ名でリソースを指定する必要が無くなり、Validatorのクラス名のみをキーとして
            // エラーメッセージにアクセスできる。

            // MEMO: カルチャの違いでメッセージを返す機能はリソースマネージャから提供されるロジックを利用する。
            // リソースマネージャのデフォルトはen-US、日本語の場合はja-JPを指定する。
            // MEMO: JAVA版ではResourceBundleの割り当て時エラー処理をしているが、C#ではValidationMessage.ResourceManagerで
            // 解決するので不要。
            errorMessages = ValidationMessage.ResourceManager;

            // MEMO: 実際にこのクラスに対応するエラーメッセージがリソースに登録されているかの確認は、
            // JAVA版でもこの関数内ではしていないようなので、別途実施する。
        }

        // MEMO: JAVA版では@Deprecatedアノテーションを使用しているが、C#ではObsolete属性を使用する。
        /// <summary>
        /// Return the configuration Properties
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method is deprecated.")]
        public virtual Dictionary<string, string> getConfigAttributes()
        {
            return config.Properties;
        }

        /// <summary>
        /// Validation initialization, called after the configuration and symbol tables have been assigned
        /// MEMO: JAVA版ではエラーの場合はRedPenExceptionをthrowすることになっている。
        /// </summary>
        protected virtual void Init()
        { }

        public virtual Dictionary<string, object> getProperties()
        {
            return defaultProps;
        }

        private object? GetOrDefault(string name)
        {
            object? value = null;
            if (config != null && config.Properties.ContainsKey(name))
            {
                value = config.Properties[name];
            }
            // MEMO: 空のDictionaryやHashSetの場合、nullではないのでdefaultPropsから取得されないことに注意。
            if (value == null && defaultProps.ContainsKey(name))
            {
                value = defaultProps[name];
            }
            return value;
        }

        /// <summary>
        /// gets the int.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An int.</returns>
        protected internal int GetInt(string name)
        {
            var value = GetOrDefault(name);
            if (value == null)
            {
                throw new ArgumentException("Property " + name + " is not set.");
            }
            else if (value is int)
            {
                return (int)value;
            }
            else
            {
                // JAVA版でもParseしているが例外処理していないので一旦このままとする。
                // TODO: 例外処理を追加する。
                return int.Parse((string)value);
            }
        }

        /// <summary>
        /// Gets the float.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A float.</returns>
        protected internal float GetFloat(string name)
        {
            var value = GetOrDefault(name);
            if (value == null)
            {
                throw new ArgumentException("Property " + name + " is not set.");
            }
            else if (value is float)
            {
                return (float)value;
            }
            else
            {
                // JAVA版でもParseしているが例外処理していないので一旦このままとする。
                // TODO: 例外処理を追加する。
                return float.Parse((string)value);
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A T? .</returns>
        protected internal virtual T? GetProperty<T>(string name)
        {
            object? value = GetOrDefault(name);
            if (value == null)
            {
                // nullを返す場合はnull許容型に変換する。
                return (T?)value;
            }
            else
            {
                // MEMO: JAVA版では型が異なる場合はInteger.valueOf((String)value);などとして強引に変換しているが、
                // C#ではConfigで型の整合性がとれている前提を置いてもよかろうという判断で単純にキャストを行っている。
                // MEMO: intなど値型の変換を行うためasは使用できない。
                try
                {
                    // デバッグのため念のためキャスト前にisで判定を行う。
                    if (value is T v)
                    {
                        return (T?)v;
                    }
                    else
                    {
                        return (T?)value;
                    }
                }
                catch (InvalidCastException e)
                {
                    // キャストに失敗した場合Exception発生するのでログ取り。
                    LOG.Error(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// RedPenのConfファイルのプロパティ値からHashSetを取得する関数。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A HashSet.</returns>
        protected internal HashSet<string> GetHashSet(string name)
        {
            object value = null;
            if (config != null && config.Properties.ContainsKey(name))
            {
                value = config.Properties[name];
            }
            if ((value == null || ((string)value).Length == 0) && defaultProps.ContainsKey(name))
            {
                value = defaultProps[name];
            }
            if (value == null)
            {
                return null;
            }
            if (value is HashSet<string>)
            {
                return value as HashSet<string>;
            }

            HashSet<string> newValue = new HashSet<string>(((string)value).Split(',').Select(i => i.ToLower()));
            // defaultPropsに設定することで、次回からはHashSet<string>として直接取得できる。
            defaultProps[name] = newValue;

            return newValue;
        }

        /// <summary>
        /// RedPenのConfファイルのプロパティ値からDictionaryを取得する関数。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A Dictionary.</returns>
        protected internal Dictionary<string, string> GetDictionary(string name)
        {
            object value = null;
            if (config != null && config.Properties.ContainsKey(name))
            {
                value = config.Properties[name];
            }
            if ((value == null || ((string)value).Length == 0) && defaultProps.ContainsKey(name))
            {
                value = defaultProps[name];
            }
            if (value == null)
            {
                return null;
            }
            if (value is Dictionary<string, string>)
            {
                return (Dictionary<string, string>)value;
            }
            Dictionary<string, string> newValue = RedPenUtility.ParseMap((string)value);
            defaultProps[name] = newValue;

            return newValue;
        }

        // MEMO: parseMap関数はRedPenUtilityクラスに移行した。

        /// <summary>
        /// gets the config attribute.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A string? .</returns>
        protected internal virtual string? getConfigAttribute(string name)
        {
            if (config != null && config.Properties.ContainsKey(name))
            {
                return config.Properties[name].ToString();
            }

            return null;
        }

        protected internal void setValidatorName(string validatorName)
        {
            this.validatorName = validatorName;
        }

        //    /** @deprecated Please use constructor with default Properties instead, and then getXXX() methods */
        //    @Deprecated
        //    protected String getConfigAttribute(String name, String defaultValue)
        //    {
        //        return getConfigAttribute(name).orElse(defaultValue);
        //    }

        //    @Deprecated
        //    protected int getConfigAttributeAsInt(String name, int defaultValue)
        //    {
        //        return parseInt(getConfigAttribute(name, Integer.toString(defaultValue)));
        //    }

        //    @Deprecated
        //    protected boolean getConfigAttributeAsBoolean(String name, boolean defaultValue)
        //    {
        //        return parseBoolean(getConfigAttribute(name, Boolean.toString(defaultValue)));
        //    }

        //    @Deprecated
        //    protected double getConfigAttributeAsDouble(String name, double defaultValue)
        //    {
        //        return parseDouble(getConfigAttribute(name, Double.toString(defaultValue)));
        //    }

        /// <summary>Gets the symbol table.</summary>
        protected internal SymbolTable SymbolTable => globalConfig.SymbolTable;

        // TODO: ファイルからのロード系の処理は適切なクラスから実行するように見直す。

        /// <summary>
        /// finds the file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>A FileInfo.</returns>
        protected internal FileInfo findFile(string relativePath)
        {
            return globalConfig.FindFile(relativePath);
        }

        /// <summary>Validatorに割り当てられたエラーレベルを返す。</summary>
        protected internal Level Level => config == null ? Level.ERROR : config.Level;

        /// <summary>
        /// create a ValidationError for the specified position with specified message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sentenceWithError">The sentence with error.</param>
        protected internal void addError(string message, Sentence sentenceWithError)
        {
            errors.Add(new ValidationError(this.validatorName, message, sentenceWithError, this.Level));
        }

        /// <summary>
        /// create a ValidationError for the specified position with specified message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sentenceWithError">The sentence with error.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        protected internal void addErrorWithPosition(string message, Sentence sentenceWithError, int start, int end)
        {
            errors.Add(new ValidationError(this.validatorName, message, sentenceWithError, start, end, this.Level));
        }

        /// <summary>
        /// create a ValidationError for the specified position with localized default error message
        /// </summary>
        /// <param name="sentenceWithError"></param>
        /// <param name="args"></param>
        protected internal void addLocalizedError(Sentence sentenceWithError, params object[] args)
        {
            addLocalizedError(null, sentenceWithError, args);
        }

        /// <summary>
        /// create a ValidationError for the specified position with localized message with specified message key
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="sentenceWithError"></param>
        /// <param name="args"></param>
        protected internal void addLocalizedError(string? messageKey, Sentence sentenceWithError, params object[] args)
        {
            errors.Add(new ValidationError(
                this.validatorName,
                GetLocalizedErrorMessage(messageKey, args),
                sentenceWithError,
                Level));
        }

        /// <summary>
        /// create a ValidationError using the details within the given token &amp; localized message
        /// </summary>
        /// <param name="sentenceWithError"></param>
        /// <param name="token">the TokenElement that has the error</param>
        /// <param name="args"></param>
        protected internal void addLocalizedErrorFromToken(Sentence sentenceWithError, TokenElement token, params object[] args)
        {
            List<object> argList = new List<object>();
            foreach (object arg in args)
            {
                argList.Add(arg);
            }

            argList.Insert(0, token.Surface);

            addLocalizedErrorWithPosition(
                sentenceWithError,
                token.Offset,
                token.Offset + token.Surface.Length,
                argList.ToArray()
            );
        }

        /// <summary>
        /// create a ValidationError for the specified position with default localized error message
        /// </summary>
        /// <param name="sentenceWithError"></param>
        /// <param name="start">start position in parsed sentence</param>
        /// <param name="end">end position in parsed sentence</param>
        /// <param name="args"></param>
        protected internal void addLocalizedErrorWithPosition(Sentence sentenceWithError, int start, int end, params object[] args)
        {
            addLocalizedErrorWithPosition(null, sentenceWithError, start, end, args);
        }

        /// <summary>
        /// create a ValidationError for the specified position with specified message key
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="sentenceWithError"></param>
        /// <param name="start">start position in parsed sentence</param>
        /// <param name="end">end position in parsed sentence</param>
        /// <param name="args"></param>
        protected internal void addLocalizedErrorWithPosition(
            string? messageKey,
            Sentence sentenceWithError,
            int start,
            int end,
            params object[] args)
        {
            errors.Add(new ValidationError(
                this.validatorName,
                GetLocalizedErrorMessage(messageKey, args),
                sentenceWithError,
                start,
                end,
                Level));
        }

        /// <summary>
        /// returns localized error message for the given key formatted with argument
        /// </summary>
        /// <param name="key">message key</param>
        /// <param name="args">objects to format</param>
        /// <returns>localized error message</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected internal string GetLocalizedErrorMessage(string? key, params object[] args)
        {
            if (errorMessages != null)
            {
                string suffix = key != null ? "." + key : "";

                // Validatorのクラス名 + "." + キー名サフィックスで検索した現在のロケール用のメッセージ。
                string pattern = errorMessages.GetString(this.GetType().Name + suffix, Locale);

                // MessageFormatの代わりにstring.Formatを使用
                return string.Format(Locale, pattern, args);
            }
            else
            {
                throw new InvalidOperationException("message resource not found.");
            }
        }

        // 以下、Deprecated。代わりにaddLocalizedErrorを使用する。

        //    /**
        //     * create a ValidationError for the specified position with default error message
        //     *
        //     * @param sentenceWithError sentence
        //     * @param args              objects to format
        //     * @deprecated use {@link #addLocalizedError(Sentence, Object...)} instead
        //     */
        //    protected void addValidationError(Sentence sentenceWithError, Object...args)
        //    {
        //        addLocalizedError(sentenceWithError, args);
        //    }

        //    /**
        //     * create a ValidationError for the specified position with specified message key
        //     *
        //     * @param messageKey        messageKey
        //     * @param sentenceWithError sentence
        //     * @param args              objects to format
        //     * @deprecated use {@link #addLocalizedError(String, Sentence, Object...)} instead
        //     */
        //    protected void addValidationError(String messageKey, Sentence sentenceWithError, Object...args)
        //    {
        //        addLocalizedError(messageKey, sentenceWithError, args);
        //    }

        //    /**
        //     * create a ValidationError using the details within the given token
        //     *
        //     * @param sentenceWithError sentence
        //     * @param token             the TokenElement that has the error
        //     * @deprecated use {@link #addLocalizedErrorFromToken(Sentence, TokenElement)} instead
        //     */
        //    protected void addValidationErrorFromToken(Sentence sentenceWithError, TokenElement token)
        //    {
        //        addLocalizedError(sentenceWithError, token);
        //    }

        //    /**
        //     * create a ValidationError for the specified position with default error message
        //     *
        //     * @param sentenceWithError sentence
        //     * @param start             start position
        //     * @param end               end position
        //     * @param args              objects to format
        //     * @deprecated use {@link #addLocalizedErrorWithPosition(Sentence, int, int, Object...)} instead
        //     */
        //    protected void addValidationErrorWithPosition(Sentence sentenceWithError,
        //                                                  Optional<LineOffset> start, Optional<LineOffset> end, Object...args)
        //    {
        //        errors.add(new ValidationError(this.getClass(), GetLocalizedErrorMessage(null, args), sentenceWithError, start.get(), end.get()));
        //    }

        //    /**
        //     * create a ValidationError for the specified position with specified message key
        //     *
        //     * @param messageKey        messageKey
        //     * @param sentenceWithError sentence
        //     * @param start             start position
        //     * @param end               end position
        //     * @param args              objects to format
        //     * @deprecated use {@link #addLocalizedErrorWithPosition(String, Sentence, int, int, Object...)} instead
        //     */
        //    protected void addValidationErrorWithPosition(String messageKey, Sentence sentenceWithError,
        //                                                  Optional<LineOffset> start, Optional<LineOffset> end, Object...args)
        //    {
        //        errors.add(new ValidationError(this.getClass(), GetLocalizedErrorMessage(messageKey, args), sentenceWithError, start.get(), end.get()));
        //    }

        /// <summary>
        /// Convert To String.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return this.GetType().Name + defaultProps;
        }

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>A bool.</returns>
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o is not Validator)
            {
                return false;
            }
            Validator validator = (Validator)o;
            // TODO: ValidatorConfigurationのEqualsメソッドをユニットテストする。
            return GetType() == validator.GetType() && config.Equals(validator.config);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>An int.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            //hash.Add(defaultProps);
            //hash.Add(errorMessages);

            // MEMO: JAVA版ではObjects.hash(getClass(), config);のみ。
            hash.Add(this.GetType());
            hash.Add(config);
            //hash.Add(globalConfig);
            //hash.Add(Locale);
            //hash.Add(validatorName);
            //hash.Add(errors);
            //hash.Add(SymbolTable);
            //hash.Add(Level);
            return hash.ToHashCode();
        }

        /// <summary>Resource Extractor loads key-value dictionary</summary>
        protected internal static readonly DictionaryLoader<Dictionary<string, string>> KEY_VALUE =
            new DictionaryLoader<Dictionary<string, string>>(
                () => new Dictionary<string, string>(),
                (map, line) =>
                {
                    string[] result = line.Split('\t');

                    if (result.Length == 2)
                    {
                        map.Add(result[0], result[1]);
                    }
                    else
                    {
                        LOG.Error("Skip to load line... Invalid line: " + line);
                    }
                });

        /// <summary>Resource Extractor loads rule dictionary</summary>
        protected internal static readonly DictionaryLoader<HashSet<ExpressionRule>> RULE =
            new DictionaryLoader<HashSet<ExpressionRule>>(
                () => new HashSet<ExpressionRule>(),
                (set, line) => set.Add(RuleExtractor.Run(line))
            );

        /// <summary>Resource Extractor loads word list</summary>
        protected internal static readonly DictionaryLoader<HashSet<string>> WORD_LIST =
            new DictionaryLoader<HashSet<string>>(() => new HashSet<string>(), (set, line) => set.Add(line));

        /// <summary>Resource Extractor loads word list</summary>
        protected internal static readonly DictionaryLoader<HashSet<string>> WORD_LIST_LOWERCASED =
            new DictionaryLoader<HashSet<string>>(
                () => new HashSet<string>(),
                (set, line) => set.Add(line.ToLower())
            );
    }
}
