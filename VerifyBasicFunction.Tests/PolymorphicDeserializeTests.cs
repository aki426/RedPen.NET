using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    public class PolymorphicDeserializeTests
    {
        [Fact]
        public void PolymorphicJsonDeserializeSampleTest()
        {
            var people = PolymorphicJsonDeserializeSampleModel.Deserialize();

            people[0].GetType().Name.Should().Be("Customer");
            (people[0] as Customer).CreditLimit.Should().Be(10000);
            people[0].Name.Should().Be("John");

            people[1].GetType().Name.Should().Be("Employee");
            (people[1] as Employee).OfficeNumber.Should().Be("555-1234");
            people[1].Name.Should().Be("Nancy");
        }

        [Fact]
        public void PolymorphicJsonSerializeTest()
        {
            string v = PolymorphicJsonDeserializeValidatorConfigurationSampleModel.Serialize();
            v.Should().NotBeNullOrEmpty();
            v.Should().Be(@"[
  {
    ""Name"": ""SentenceLength"",
    ""Level"": ""INFO"",
    ""MaxLength"": 120
  },
  {
    ""Name"": ""JapaneseExpressionVariation"",
    ""Level"": ""WARN"",
    ""WordMap"": {
      ""A"": ""a"",
      ""B"": ""b""
    }
  }
]");
        }

        [Fact]
        public void DeserializeSentenceLengthTest()
        {
            var sentenceLengthConfiguration = PolymorphicJsonDeserializeValidatorConfigurationSampleModel.DeserializeSentenceLength(@"{
          ""Name"": ""SentenceLength"",
          ""Level"": ""Info"",
          ""MaxLength"": 120
        }");

            sentenceLengthConfiguration.Level.Should().Be(ValidationLevel.INFO);
            sentenceLengthConfiguration.MaxLength.Should().Be(120);
        }

        [Fact]
        public void PolymorphicJsonDeserializeTest()
        {
            var validatorConfigurations = PolymorphicJsonDeserializeValidatorConfigurationSampleModel.Deserialize(@"[
  {
    ""Name"": ""SentenceLength"",
    ""Level"": ""INFO"",
    ""MaxLength"": 120
  },
  {
    ""Name"": ""JapaneseExpressionVariation"",
    ""Level"": ""WARN"",
    ""WordMap"": {
      ""A"": ""a"",
      ""B"": ""b""
    }
  }
]");

            (validatorConfigurations[0] is SentenceLengthConfiguration).Should().BeTrue();
            var sentenceLen = validatorConfigurations[0] as SentenceLengthConfiguration;
            sentenceLen.Level.Should().Be(ValidationLevel.INFO);
        }
    }
}
