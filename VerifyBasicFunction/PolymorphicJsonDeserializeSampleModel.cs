using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VerifyBasicFunction
{
    public class Person
    {
        public string Name { get; set; }
    }

    public class Customer : Person
    {
        public decimal CreditLimit { get; set; }
    }

    public class Employee : Person
    {
        public string? OfficeNumber { get; set; }
    }

    public class PersonConverterWithTypeDiscriminator : JsonConverter<Person>
    {
        // MS Learnの例では、型の判別にEnumを用いている。
        // https://learn.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-8-0#support-polymorphic-deserialization
        private enum TypeDiscriminator
        {
            Customer = 1,
            Employee = 2
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeof(Person).IsAssignableFrom(typeToConvert);

        ///// <summary>
        ///// Jsonコンバータの読み取り器。
        ///// </summary>
        ///// <param name="reader">The reader.</param>
        ///// <param name="typeToConvert">The type to convert.</param>
        ///// <param name="options">The options.</param>
        ///// <returns>A Person.</returns>
        //public override Person Read(
        //    ref Utf8JsonReader reader,
        //    Type typeToConvert,
        //    JsonSerializerOptions options)
        //{
        //    if (reader.TokenType != JsonTokenType.StartObject)
        //    {
        //        throw new JsonException();
        //    }

        //    reader.Read();
        //    if (reader.TokenType != JsonTokenType.PropertyName)
        //    {
        //        throw new JsonException();
        //    }

        //    // NOTE: 最初のプロパティ値が"TypeDiscriminator"でなければならないことになっている。
        //    string? propertyName = reader.GetString();
        //    if (propertyName != "TypeDiscriminator")
        //    {
        //        throw new JsonException();
        //    }

        //    reader.Read();
        //    if (reader.TokenType != JsonTokenType.Number)
        //    {
        //        throw new JsonException();
        //    }

        //    // 順番にアクセスし、決め打ちで"TypeDiscriminator"の値を取得しオブジェクトを生成。
        //    TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
        //    Person person = typeDiscriminator switch
        //    {
        //        TypeDiscriminator.Customer => new Customer(),
        //        TypeDiscriminator.Employee => new Employee(),
        //        _ => throw new JsonException()
        //    };

        //    while (reader.Read())
        //    {
        //        if (reader.TokenType == JsonTokenType.EndObject)
        //        {
        //            return person;
        //        }

        //        if (reader.TokenType == JsonTokenType.PropertyName)
        //        {
        //            propertyName = reader.GetString();
        //            reader.Read();
        //            switch (propertyName)
        //            {
        //                case "CreditLimit":
        //                    decimal creditLimit = reader.GetDecimal();
        //                    ((Customer)person).CreditLimit = creditLimit;
        //                    break;

        //                case "OfficeNumber":
        //                    string? officeNumber = reader.GetString();
        //                    ((Employee)person).OfficeNumber = officeNumber;
        //                    break;

        //                case "Name":
        //                    string? name = reader.GetString();
        //                    person.Name = name;
        //                    break;
        //            }
        //        }
        //    }

        //    throw new JsonException();
        //}

        public override Person Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            Utf8JsonReader readerClone = reader;

            if (readerClone.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string? propertyName = readerClone.GetString();
            if (propertyName != "TypeDiscriminator")
            {
                throw new JsonException();
            }

            readerClone.Read();
            if (readerClone.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            // NOTE: ここまで同じ

            // NOTE: 型情報がはっきりした時点でDeserializeを呼び出してオブジェクト読み込みを簡単化する。
            // NOTE: 「ペイロードが独自の型情報を指定できるようにすることは、Webアプリケーションの脆弱性の一般的な原因」
            // になりうるため、あらかじめ型指定文字列を変換してよい型かどうかをチェックするDictionaryなどが必要になる。
            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)readerClone.GetInt32();
            Person person = typeDiscriminator switch
            {
                TypeDiscriminator.Customer => JsonSerializer.Deserialize<Customer>(ref reader)!,
                TypeDiscriminator.Employee => JsonSerializer.Deserialize<Employee>(ref reader)!,
                _ => throw new JsonException()
            };
            return person;
        }

        /// <summary>
        /// Jsonコンバータの書き出し器。
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="person">The person.</param>
        /// <param name="options">The options.</param>
        public override void Write(
            Utf8JsonWriter writer,
            Person person,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // NOTE: TypeDiscriminatorというプロパティを追加して、型を判別するための情報を書き出している。
            // この方式はあらかじめEnumを定義しておかなければならず、汎用性が低い。
            // できるだけ型情報を明に出していので、たとえばValidatorNameプロパティで型名を出力するなどの方法が良い。

            if (person is Customer customer)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.Customer);
                // プロパティは逐一指定する必要がある。
                writer.WriteNumber("CreditLimit", customer.CreditLimit);
            }
            else if (person is Employee employee)
            {
                writer.WriteNumber("TypeDiscriminator", (int)TypeDiscriminator.Employee);
                writer.WriteString("OfficeNumber", employee.OfficeNumber);
            }

            writer.WriteString("Name", person.Name);

            writer.WriteEndObject();

            // 一方、Writer内でSerialize関数を呼ぶ方法もあり、工夫すればこれでいけそうな気もする。
            // JsonSerializer.Serialize(writer, person, person.GetType(), options);
        }
    }

    public class PolymorphicJsonDeserializeSampleModel
    {
        public static Person[] Deserialize()
        {
            string jsonText = @"[
  {
    ""TypeDiscriminator"": 1,
    ""CreditLimit"": 10000,
    ""Name"": ""John""
  },
  {
    ""TypeDiscriminator"": 2,
    ""OfficeNumber"": ""555-1234"",
    ""Name"": ""Nancy""
  }
]
";

            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new PersonConverterWithTypeDiscriminator());

            Person[]? persons = JsonSerializer.Deserialize<Person[]>(jsonText, serializeOptions);

            if (persons == null)
            {
                return new Person[0];
            }
            else
            {
                return persons;
            }

            //foreach (var person in persons)
            //{
            //    if (person is Customer customer)
            //    {
            //        Console.WriteLine($"Customer: {customer.Name}, {customer.CreditLimit}");
            //    }
            //    else if (person is Employee employee)
            //    {
            //        Console.WriteLine($"Employee: {employee.Name}, {employee.OfficeNumber}");
            //    }
            //}
        }
    }
}
