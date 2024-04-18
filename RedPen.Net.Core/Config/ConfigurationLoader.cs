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
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
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
                Type? type = validationType.TypeOfConfigurationClass();
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
                if (conf is IMaxLengthConfigParameter maxLengthConf)
                {
                    writer.WriteNumber("MaxLength", maxLengthConf.MaxLength);
                }

                if (conf is IWordMapConfigParameter wordMapConf)
                {
                    writer.WritePropertyName("WordMap");
                    JsonSerializer.Serialize(writer, wordMapConf.WordMap, wordMapConf.WordMap.GetType(), options);
                }

                if (conf is IWordListConfigParameter wordListConf)
                {
                    writer.WritePropertyName("WordList");
                    JsonSerializer.Serialize(writer, wordListConf.WordList, wordListConf.WordList.GetType(), options);
                }

                if (conf is IMaxNumberConfigParameter maxNumberConf)
                {
                    writer.WriteNumber("MaxNumber", maxNumberConf.MaxNumber);
                }

                writer.WriteEndObject();
            }
        }
    }
}
