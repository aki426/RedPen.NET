using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace RedPen.Net.Core.Config
{
    // NOTE: ConfigurationLoaderはWriteつまりJsonシリアライズ機能を持つが、
    // 実質的にはJsonデシリアライズしか用途が無いので命名は"Loader"のままにしておく。

    /// <summary>
    /// RedPenのConfファイルをロードするクラスです。
    /// MEMO: JAVA版がXML形式であるのに対して、C#版ではJson形式を基本とします。
    /// </summary>
    public class ConfigurationLoader
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>RedPen.NETのJson設定ファイルとして読み込んでよいValidatorConfigurationの定義</summary>
        public ImmutableDictionary<string, Type> ValidatorConfigurationDefinitions { get; init; }

        /// <summary>
        /// ConfigurationLoaderとして読み込むべきValidatorConfigurationの定義を指定して新しいインスタンスを初期化します。
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        /// <param name="validatorConfigurationDefinitions">The validator configuration definitions.</param>
        public ConfigurationLoader(ImmutableDictionary<string, Type> validatorConfigurationDefinitions)
        {
            ValidatorConfigurationDefinitions = validatorConfigurationDefinitions;
        }

        // NOTE: Configurationのデシリアライズ時はLangのチェックはしない。
        // ValidatorにDIした時点でValidatorのコンストラクタでLangのチェックが行われる。

        /// <summary>
        /// Json形式の文字列からConfigurationを読み込む。
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>A Configuration.</returns>
        public Configuration Load(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                Converters = {
                    new ValidatorConfigurationConverter(this.ValidatorConfigurationDefinitions),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },

                // コメント付きJsonファイルを読み込めるようにする。
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            Configuration? configuration = JsonSerializer.Deserialize<Configuration>(jsonString, options);
            if (configuration == null)
            {
                LOG.Error("Failed to load configuration from Json string.");
                throw new JsonException();
            }

            return configuration;
        }

        /// <summary>
        /// ValidatorConfiguration用のJsonConverter。読み込み（Deserialize）だけでなく書き出し（Serialize）も行う。
        /// </summary>
        public class ValidatorConfigurationConverter : JsonConverter<ValidatorConfiguration>
        {
            private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

            /// <summary>RedPen.NETのJson設定ファイルとして読み込んでよいValidatorConfigurationの定義</summary>
            public ImmutableDictionary<string, Type> ValidatorConfigurationDefinitions { get; init; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ValidatorConfigurationConverter"/> class.
            /// </summary>
            /// <param name="validatorConfigurationDefinitions">The load config definitions.</param>
            public ValidatorConfigurationConverter(ImmutableDictionary<string, Type> validatorConfigurationDefinitions)
            {
                ValidatorConfigurationDefinitions = validatorConfigurationDefinitions;
            }

            /// <summary>
            /// Cans the convert.
            /// </summary>
            /// <param name="typeToConvert">The type to convert.</param>
            /// <returns>A bool.</returns>
            public override bool CanConvert(Type typeToConvert) =>
                typeof(ValidatorConfiguration).IsAssignableFrom(typeToConvert);

            /// <summary>
            /// Read ValidatorConfiguration from Json.
            /// </summary>
            /// <param name="reader">The reader.</param>
            /// <param name="typeToConvert">The type to convert.</param>
            /// <param name="options">The options.</param>
            /// <returns>A ValidatorConfiguration.</returns>
            public override ValidatorConfiguration Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                Utf8JsonReader readerClone = reader;

                if (readerClone.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                // 1つ目のプロパティがNameであることを確認する。
                readerClone.Read();
                if (readerClone.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                // 型情報はJsonではNameプロパティが持つ定義とする。
                string? propertyName = readerClone.GetString();
                if (propertyName != "Name")
                {
                    throw new JsonException();
                }

                readerClone.Read();
                if (readerClone.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                // NOTE: Nameプロパティの値で型指定されているのでそれを元に型を決定する。
                // NOTE: 型情報がはっきりした時点でDeserializeを呼び出してオブジェクト読み込みを簡単化する。

                // ValidationName文字列に対応したクラスのTypeオブジェクトを取得する。
                string? typeName = readerClone.GetString();
                if (typeName == null)
                {
                    throw new JsonException($"Loaded configuration name is null.");
                }

                // NOTE: 「ペイロードが独自の型情報を指定できるようにすることは、Webアプリケーションの脆弱性の一般的な原因」
                // になりうるため、ConfigurationLoaderのコンストラクト時に渡されたValidatorConfigurationの定義に無い設定は
                // 読み込まずに例外を発生させる。

                //ValidationType validationType = ValidationTypeExtend.ConvertFrom(typeName);
                //Type? type = validationType.GetTypeAsConfigurationClass();

                if (!this.ValidatorConfigurationDefinitions.ContainsKey(typeName))
                {
                    throw new JsonException($"No such a ValidationType as {typeName}");
                }

                ValidatorConfiguration? conf = JsonSerializer.Deserialize(
                    ref reader,
                    this.ValidatorConfigurationDefinitions[typeName],
                    optionForNoLoop) as ValidatorConfiguration;

                if (conf == null)
                {
                    throw new JsonException($"Deserialized {this.ValidatorConfigurationDefinitions[typeName].Name} is null.");
                }

                return conf;
            }

            // Convertersを調整しないと何回もValidatorConfigurationConverterが呼ばれて無限ループになってしまうため必要。
            // NOTE: 特にLodaerの設定に依存しないのでstaticで良い。
            private static JsonSerializerOptions optionForNoLoop = new JsonSerializerOptions
            {
                // MEMO: Enumを文字列へ変換し、フォーマットはCamelCaseにする。
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            /// <summary>
            /// Jsonコンバータの書き出し器。
            /// </summary>
            /// <param name="writer">The writer.</param>
            /// <param name="conf">The person.</param>
            /// <param name="options">The options.</param>
            public override void Write(
                Utf8JsonWriter writer,
                ValidatorConfiguration conf,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WriteString("Name", conf.Name);
                writer.WriteString("Level", conf.Level.ToString());

                // ValidatorConfiguration.csに定義されたどのInterface型を継承しているかによってプロパティを特定する。
                // TODO: ValidatorConfiguration向けのプロパティ用Interfaceを追加した場合はここに書き出しロジックを追加する。
                // NOTE: Interface型を検査しているだけなので、ValidatorConfiguration自体を追加した場合にWriteメソッドに変更を加える必要は無い。

                #region Max/Min系のプロパティ

                if (conf is IMaxLengthConfigParameter maxLengthConf) { writer.WriteNumber("MaxLength", maxLengthConf.MaxLength); }
                if (conf is IMinLengthConfigParameter minLengthConf) { writer.WriteNumber("MinLength", minLengthConf.MinLength); }

                if (conf is IMaxCountConfigParameter maxCountConf) { writer.WriteNumber("MaxCount", maxCountConf.MaxCount); }
                if (conf is IMinCountConfigParameter minCountConf) { writer.WriteNumber("MinCount", minCountConf.MinCount); }

                if (conf is IMaxLevelConfigParameter maxLevelConf) { writer.WriteNumber("MaxLevel", maxLevelConf.MaxLevel); }
                if (conf is IMinLevelConfigParameter minLevelConf) { writer.WriteNumber("MinLevel", minLevelConf.MinLevel); }

                if (conf is IMaxIntervalConfigParameter maxIntervalConf) { writer.WriteNumber("MaxInterval", maxIntervalConf.MaxInterval); }
                if (conf is IMinIntervalConfigParameter minIntervalConf) { writer.WriteNumber("MinInterval", minIntervalConf.MinInterval); }

                if (conf is IMaxRatioConfigParameter maxRatioConf) { writer.WriteNumber("MaxRatio", maxRatioConf.MaxRatio); }
                if (conf is IMinRatioConfigParameter minRatioConf) { writer.WriteNumber("MinRatio", minRatioConf.MinRatio); }

                if (conf is IMaxFreqConfigParameter maxFreqConf) { writer.WriteNumber("MaxFreq", maxFreqConf.MaxFreq); }
                if (conf is IMinFreqConfigParameter minFreqConf) { writer.WriteNumber("MinFreq", minFreqConf.MinFreq); }

                if (conf is IMaxDistanceConfigParameter maxDistanceConf) { writer.WriteNumber("MaxDistance", maxDistanceConf.MaxDistance); }
                if (conf is IMinDistanceConfigParameter minDistanceConf) { writer.WriteNumber("MinDistance", minDistanceConf.MinDistance); }

                #endregion Max/Min系のプロパティ

                #region Set/Map系のプロパティ

                if (conf is ICharSetConfigParameter charSetConf)
                {
                    writer.WritePropertyName("CharSet");
                    JsonSerializer.Serialize(writer, charSetConf.CharSet, charSetConf.CharSet.GetType(), options);
                }

                if (conf is IWordSetConfigParameter wordSetConf)
                {
                    writer.WritePropertyName("WordSet");
                    JsonSerializer.Serialize(writer, wordSetConf.WordSet, wordSetConf.WordSet.GetType(), options);
                }

                if (conf is IExpressionSetConfigParameter expressionSetConf)
                {
                    writer.WritePropertyName("ExpressionSet");
                    JsonSerializer.Serialize(writer, expressionSetConf.ExpressionSet, expressionSetConf.ExpressionSet.GetType(), options);
                }

                if (conf is IGrammarRuleSetConfigParameter grammarRuleSetConf)
                {
                    writer.WritePropertyName("GrammarSet");
                    JsonSerializer.Serialize(writer, grammarRuleSetConf.GrammarRuleSet, grammarRuleSetConf.GrammarRuleSet.GetType(), options);
                }

                if (conf is ICharMapConfigParameter charMapConf)
                {
                    writer.WritePropertyName("CharMap");
                    JsonSerializer.Serialize(writer, charMapConf.CharMap, charMapConf.CharMap.GetType(), options);
                }

                if (conf is IWordMapConfigParameter wordMapConf)
                {
                    writer.WritePropertyName("WordMap");
                    JsonSerializer.Serialize(writer, wordMapConf.WordMap, wordMapConf.WordMap.GetType(), options);
                }

                if (conf is IExpressionMapConfigParameter expressionMapConf)
                {
                    writer.WritePropertyName("ExpressionMap");
                    JsonSerializer.Serialize(writer, expressionMapConf.ExpressionMap, expressionMapConf.ExpressionMap.GetType(), options);
                }

                if (conf is IGrammarRuleMapConfigParameter grammarRuleMapConf)
                {
                    writer.WritePropertyName("GrammarMap");
                    JsonSerializer.Serialize(writer, grammarRuleMapConf.GrammarRuleMap, grammarRuleMapConf.GrammarRuleMap.GetType(), options);
                }

                #endregion Set/Map系のプロパティ

                #region その他のプロパティ

                if (conf is IEnableDefaultDictConfigParameter enableDefaultDictConf) { writer.WriteBoolean("EnableDefaultDict", enableDefaultDictConf.EnableDefaultDict); }
                if (conf is IJodoshiStyleConfigParameter jodoshiStyleConf) { writer.WriteString("JodoshiStyle", jodoshiStyleConf.JodoshiStyle.ToString()); }
                if (conf is INumberStyleConfigParameter numberStyleConf) { writer.WriteString("NumberStyle", numberStyleConf.NumberStyle.ToString()); }
                if (conf is IForbiddenConfigParameter forbiddenConf) { writer.WriteBoolean("Forbidden", forbiddenConf.Forbidden); }
                if (conf is ISkipAfterConfigParameter skipAfterConf) { writer.WriteString("SkipAfter", skipAfterConf.SkipAfter); }
                if (conf is ISkipBeforeConfigParameter skipBeforeConf) { writer.WriteString("SkipBefore", skipBeforeConf.SkipBefore); }

                #endregion その他のプロパティ

                writer.WriteEndObject();
            }
        }
    }
}
