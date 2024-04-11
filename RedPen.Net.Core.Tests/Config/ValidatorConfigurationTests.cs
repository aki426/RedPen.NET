using RedPen.Net.Core.Config;
using Xunit;
using Assert = Xunit.Assert;

namespace RedPen.Net.Core.Tests.Config
{
    public class ValidatorConfigurationTests
    {
        [Fact()]
        public void CloneTest()
        {
            ValidatorConfiguration conf = new ValidatorConfiguration("ERROR");
            ValidatorConfiguration clone = conf;

            // XUnitでの等価性と同一性の検証
            Assert.NotSame(conf, clone);
            Assert.Equal(conf, clone);

            // プロパティの等価性と同一性の検証
            Assert.Equal(conf.Level, clone.Level);
            Assert.NotSame(conf.Level, clone.Level);

            // with式はC#9で利用可能。
            //ValidatorConfiguration changedLevel = conf with { Level = ValidationLevel.INFO };
        }
    }
}
