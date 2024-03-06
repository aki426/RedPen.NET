using System;
using Xunit;
using Assert = Xunit.Assert;

namespace redpen_core.config.Tests
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
            // a

            throw new NotImplementedException();
        }

        //        @Test
        //  void canBeCloned() throws Exception
        //        {
        //            ValidatorConfiguration conf = new ValidatorConfiguration("test").addProperty("foo", "bar");
        //        ValidatorConfiguration clone = conf.clone();

        //        assertNotSame(conf, clone);
        //        assertEquals(conf, clone);

        //        assertNotSame(conf.getProperties(), clone.getProperties());
        //        assertEquals(conf.getProperties(), clone.getProperties());

        //        assertNotSame(conf.getProperties(), clone.getProperties());
        //        assertEquals(conf.getLevel(), clone.getLevel());
        //    }

        //    @Test
        //  void equals() throws Exception
        //    {
        //        ValidatorConfiguration conf = new ValidatorConfiguration("test").addProperty("foo", "bar").setLevel(ValidatorConfiguration.LEVEL.ERROR);
        //    ValidatorConfiguration conf2 = new ValidatorConfiguration("test").addProperty("foo", "bar").setLevel(ValidatorConfiguration.LEVEL.ERROR);
        //    assertEquals(conf, conf2);
        //}

        //@Test
        //  void equals_properties() throws Exception {
        //    ValidatorConfiguration conf = new ValidatorConfiguration("test").addProperty("foo", "bar");
        //ValidatorConfiguration conf2 = new ValidatorConfiguration("test").addProperty("foo", "bar2");
        //assertFalse(conf.equals(conf2));
        //  }

        //  @Test
        //  void equals_names() throws Exception {
        //    ValidatorConfiguration conf = new ValidatorConfiguration("test");
        //ValidatorConfiguration conf2 = new ValidatorConfiguration("test2");
        //assertFalse(conf.equals(conf2));
        //  }

        //  @Test
        //  void equals_levels() throws Exception {
        //    ValidatorConfiguration conf = new ValidatorConfiguration("test").setLevel(ValidatorConfiguration.LEVEL.INFO);
        //ValidatorConfiguration conf2 = new ValidatorConfiguration("test2").setLevel(ValidatorConfiguration.LEVEL.INFO); ;
        //assertFalse(conf.equals(conf2));
        //  }
    }
}