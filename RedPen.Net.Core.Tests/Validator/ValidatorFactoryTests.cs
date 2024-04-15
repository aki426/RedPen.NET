using RedPen.Net.Core.Validators;
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
            // TODO: Implement GetValidatorTypesTest
            // ValidatorFactory.ValidatorTypes.ForEach(i => output.WriteLine(i.Name));
        }
    }
}
