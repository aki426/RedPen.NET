using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;
using RedPen.Net.Core.Validators.DocumentValidator;
using RedPen.Net.Core.Validators.SentenceValidator;

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
                // TODO: 「ペイロードが独自の型情報を指定できるようにすることは、Webアプリケーションの脆弱性の一般的な原因」
                // になりうるため、あらかじめ型指定文字列を変換してよい型かどうかをチェックするDictionaryなどが必要になる。

                var typeName = readerClone.GetString();
                ValidatorConfiguration conf = typeName switch
                {
                    // switch式でベタ指定しているが、あらかじめValidatorConfiguration継承型をリストアップした
                    // Dictionary<string, Type>を作成しておくとよい。
                    "SentenceLength" => JsonSerializer.Deserialize<SentenceLengthConfiguration>(ref reader, optionForNoLoop)!,
                    "JapaneseExpressionVariation" => JsonSerializer.Deserialize<JapaneseExpressionVariationConfiguration>(ref reader, optionForNoLoop)!,
                    _ => throw new JsonException()
                };

                return conf;
            }

            // Convertersを調整しないと、何回もValidatorConfigurationConverterが呼ばれて無限ループになってしまうため。
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

                // NOTE: TypeDiscriminatorというプロパティを追加して、型を判別するための情報を書き出している。
                // この方式はあらかじめEnumを定義しておかなければならず、汎用性が低い。
                // できるだけ型情報を明に出したいので、たとえばValidatorNameプロパティで型名を出力するなどの方法が良い。

                var typeName = conf.GetType().Name.Replace("Configuration", "");
                writer.WriteString("Name", typeName);
                writer.WriteString("Level", conf.Level.ToString());

                // ValidatorConfiguration.csに定義されたどのInterface型を継承しているかによってプロパティを特定する。
                if (conf is IMaxLengthConfigParameter maxLengthConf)
                {
                    writer.WriteNumber("MaxLength", maxLengthConf.MaxLength);
                }
                else if (conf is IWordMapConfigParameter wordMapConf)
                {
                    writer.WritePropertyName("WordMap");
                    JsonSerializer.Serialize(writer, wordMapConf.WordMap, wordMapConf.WordMap.GetType(), options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
