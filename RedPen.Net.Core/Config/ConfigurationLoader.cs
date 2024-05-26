using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// RedPenのConfファイルをロードするクラスです。
    /// MEMO: JAVA版がXML形式であるのに対して、C#版ではJson形式を基本とします。
    /// </summary>
    public class ConfigurationLoader
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        // NOTE: Configurationのデシリアライズ時はLangのチェックはしない。
        // ValidatorにDIした時点でLangのチェックが行われる。
        // なお、LevelがOFFだった場合はValidatorが作成されないので言語チェックもされない。

        /// <summary>
        /// Json形式の文字列からConfigurationを読み込む。
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>A Configuration.</returns>
        public static Configuration Load(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                Converters = {
                    new ValidatorConfigurationConverter(),
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
                // になりうるため、Enum型のValidationTypeにアセンブリ内に実在するConfigurationとValidatorのみを指定できる機能を付加している。

                // ConvertFromは対応するValidationTypeが存在しない場合は例外が発生する。
                ValidationType validationType = ValidationTypeExtend.ConvertFrom(typeName);
                Type? type = validationType.GetTypeAsConfigurationClass();
                if (type == null)
                {
                    throw new JsonException($"No such a ValidationType as {typeName}");
                }

                ValidatorConfiguration? conf = JsonSerializer.Deserialize(ref reader, type, optionForNoLoop) as ValidatorConfiguration;
                if (conf == null)
                {
                    throw new JsonException($"Deserialized {validationType.ConfigurationName()} is null.");
                }

                return conf;
            }

            // Convertersを調整しないと何回もValidatorConfigurationConverterが呼ばれて無限ループになってしまうため必要。
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

                #region Max/Min系のプロパティ

                if (conf is IMaxLengthConfigParameter maxLengthConf) { writer.WriteNumber("MaxLength", maxLengthConf.MaxLength); }
                if (conf is IMinLengthConfigParameter minLengthConf) { writer.WriteNumber("MinLength", minLengthConf.MinLength); }
                if (conf is IMinCountConfigParameter minCountConf) { writer.WriteNumber("MinCount", minCountConf.MinCount); }
                if (conf is IMinLevelConfigParameter minLevelConf) { writer.WriteNumber("MinLevel", minLevelConf.MinLevel); }
                if (conf is IMaxIntervalConfigParameter maxIntervalConf) { writer.WriteNumber("MaxInterval", maxIntervalConf.MaxInterval); }
                if (conf is IMaxRatioConfigParameter maxRatioConf) { writer.WriteNumber("MaxRatio", maxRatioConf.MaxRatio); }
                if (conf is IMinFreqConfigParameter minFreqConf) { writer.WriteNumber("MinFreq", minFreqConf.MinFreq); }
                if (conf is IMaxDistanceConfigParameter maxDistanceConf) { writer.WriteNumber("MaxDistance", maxDistanceConf.MaxDistance); }

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
