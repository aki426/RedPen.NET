//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using RedPen.Net.Core.Config;
using Xunit;
using Assert = Xunit.Assert;

namespace RedPen.Net.Core.Tests.Config
{
    public class ValidatorConfigurationTests
    {
        //[Fact()]
        //public void CloneTest()
        //{
        //    ValidatorConfiguration conf = new ValidatorConfiguration("ERROR");
        //    ValidatorConfiguration clone = conf;

        //    // XUnitでの等価性と同一性の検証
        //    Assert.NotSame(conf, clone);
        //    Assert.Equal(conf, clone);

        //    // プロパティの等価性と同一性の検証
        //    Assert.Equal(conf.Level, clone.Level);
        //    Assert.NotSame(conf.Level, clone.Level);

        //    // with式はC#9で利用可能。
        //    //ValidatorConfiguration changedLevel = conf with { Level = ValidationLevel.INFO };
        //}
    }
}
