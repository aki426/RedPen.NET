using FluentAssertions;
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
            ValidatorConfiguration conf = new ValidatorConfiguration("test").AddProperty("foo", "bar");
            ValidatorConfiguration clone = conf.Clone();

            // XUnitでの等価性と同一性の検証
            Assert.NotSame(conf, clone);
            Assert.Equal(conf, clone);

            Assert.Equal(conf.ConfigurationName, clone.ConfigurationName);

            Assert.NotSame(conf.Properties, clone.Properties);
            Assert.Equal(conf.Properties, clone.Properties);

            Assert.Equal(conf.Level, clone.Level);
        }

        [Fact]
        public void EqualsTest()
        {
            ValidatorConfiguration conf = new ValidatorConfiguration("test").AddProperty("foo", "bar").SetLevel(ValidationLevel.ERROR);
            ValidatorConfiguration conf2 = new ValidatorConfiguration("test").AddProperty("foo", "bar").SetLevel(ValidationLevel.ERROR);

            Assert.Equal(conf, conf2);
        }

        [Fact]
        public void EqualsNamesTest()
        {
            ValidatorConfiguration conf = new ValidatorConfiguration("test");
            ValidatorConfiguration conf2 = new ValidatorConfiguration("test2");
            conf.Equals(conf2).Should().BeFalse();
        }

        [Fact]
        public void EqualsPropertiesTest()
        {
            ValidatorConfiguration conf = new ValidatorConfiguration("test").AddProperty("foo", "bar");
            ValidatorConfiguration conf2 = new ValidatorConfiguration("test").AddProperty("foo", "bar2");
            conf.Equals(conf2).Should().BeFalse();
        }

        [Fact]
        public void EqualsLevelsTest()
        {
            ValidatorConfiguration conf = new ValidatorConfiguration("test").SetLevel(ValidationLevel.INFO);
            ValidatorConfiguration conf2 = new ValidatorConfiguration("test").SetLevel(ValidationLevel.WARN);
            conf.Equals(conf2).Should().BeFalse();
        }
    }
}
