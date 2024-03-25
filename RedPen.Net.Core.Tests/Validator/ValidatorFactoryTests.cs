using RedPen.Net.Core.Validator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator
{
    public class ValidatorFactoryTests
    {
        private readonly ITestOutputHelper output;

        public ValidatorFactoryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GetValidatorTypesTest()
        {
            ValidatorFactory.GetValidatorTypes().ForEach(i => output.WriteLine(i.Name));
        }
    }
}
