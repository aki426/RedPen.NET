using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace VerifyBasicFunction
{
    public enum ValidationLevel
    {
        OFF, // 該当のValidatorを無効化する
        INFO,
        WARN,
        ERROR
    }

    public class ValidatorConfiguration
    {
        public ValidationLevel Level { get; set; }
    }

    public interface IMaxLengthConfigParameter
    {
        public int MaxLength { get; set; }
    }

    public interface IWordMapConfigParameter
    {
        public Dictionary<string, string> WordMap { get; set; }
    }

    public class SentenceLengthConfiguration : ValidatorConfiguration, IMaxLengthConfigParameter
    {
        public int MaxLength { get; set; }
    }

    public class JapaneseExpressionVariationConfiguration : ValidatorConfiguration, IWordMapConfigParameter
    {
        public Dictionary<string, string> WordMap { get; set; }
    }

    public class ValidatorConfigurationConverter : JsonConverter<ValidatorConfiguration>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(ValidatorConfiguration).IsAssignableFrom(typeToConvert);

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

            //// 2つ目のプロパティがLevelであることを確認する。
            //readerClone.Read();
            //if (readerClone.TokenType != JsonTokenType.PropertyName)
            //{
            //    throw new JsonException();
            //}

            //// 型情報はJsonではNameプロパティが持つ定義とする。
            //string? propertyLevel = readerClone.GetString();
            //if (propertyName != "Level")
            //{
            //    throw new JsonException();
            //}

            //readerClone.Read();
            //if (readerClone.TokenType != JsonTokenType.String)
            //{
            //    throw new JsonException();
            //}

            // NOTE: Nameプロパティの値で型指定さえているのでそれを元に型を決定する。
            // NOTE: 型情報がはっきりした時点でDeserializeを呼び出してオブジェクト読み込みを簡単化する。
            // NOTE: 「ペイロードが独自の型情報を指定できるようにすることは、Webアプリケーションの脆弱性の一般的な原因」
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

            // Pattern A: 具体的な型によってプロパティを指定する。
            //if (conf is SentenceLengthConfiguration sentenceLen)
            //{
            //    // プロパティは逐一指定する必要がある。
            //    writer.WriteNumber("MaxLength", sentenceLen.MaxLength);
            //}
            //else if (conf is JapaneseExpressionVariationConfiguration jaExpVariation)
            //{
            //    writer.WritePropertyName("WordMap");
            //    JsonSerializer.Serialize(writer, jaExpVariation.WordMap, jaExpVariation.WordMap.GetType(), options);
            //}

            // Pattern B: 継承したInterface型によってプロパティを指定する。
            // いちいち型指定して内容を判別するのは面倒なので、書き出す型とプロパティに対応したInterface型を継承させ、
            // そのInterface型との一致判定によってプロパティを指定する。
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

            // 一方、Writer内でSerialize関数を呼ぶ方法もあり、工夫すればこれでいけそうな気もする。
            // JsonSerializer.Serialize(writer, person, person.GetType(), options);
        }
    }

    public class PolymorphicJsonDeserializeValidatorConfigurationSampleModel
    {
        public static string Serialize()
        {
            List<ValidatorConfiguration> validatorConfigurations = new List<ValidatorConfiguration>
            {
                new SentenceLengthConfiguration {
                    Level = ValidationLevel.INFO,
                    MaxLength = 120 },
                new JapaneseExpressionVariationConfiguration {
                    Level = ValidationLevel.WARN,
                    WordMap = new Dictionary<string, string> { { "A", "a" }, { "B", "b" } } }
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = {
                    new ValidatorConfigurationConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Serialize(validatorConfigurations, options);
        }

        public static List<ValidatorConfiguration> Deserialize(string jsonText)
        {
            var options = new JsonSerializerOptions
            {
                Converters = {
                    new ValidatorConfigurationConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<List<ValidatorConfiguration>>(jsonText, options);
        }

        public static SentenceLengthConfiguration DeserializeSentenceLength(string jsonText)
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new ValidatorConfigurationConverter(),
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<SentenceLengthConfiguration>(jsonText, options);
        }
    }
}
