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

using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Config.Tests
{
    /// <summary>
    /// Configurationクラスの基本機能のテスト。
    /// </summary>
    public class ConfigurationTests
    {
        [Fact()]
        public void ConfigurationTest()
        {
            // デフォルトコンストラクタで生成された後、オブジェクト初期化子で初期化されなかった場合すべてデフォルト値となる。
            var defaultConstructed = new Configuration();
            defaultConstructed.DocumentLang.Should().Be("ja-JP");
            defaultConstructed.Variant.Should().Be("");
            defaultConstructed.MessageLang.Should().Be("ja-JP");
            defaultConstructed.ValidatorConfigurations.Should().BeEmpty();
            defaultConstructed.Symbols.Should().BeEmpty();

            // デフォルトコンストラクタの場合、各初期値は他の値に影響を与えない。
            var withDocumentLang = new Configuration() { DocumentLang = "en-US" };
            withDocumentLang.DocumentLang.Should().Be("en-US");
            withDocumentLang.Variant.Should().Be("");
            withDocumentLang.MessageLang.Should().Be("ja-JP");

            var withVariant = new Configuration() { Variant = "zenkaku" };
            withVariant.DocumentLang.Should().Be("ja-JP");
            withVariant.Variant.Should().Be("zenkaku");
            withVariant.MessageLang.Should().Be("ja-JP");

            var withMessageLang = new Configuration() { MessageLang = "en-US" };
            withMessageLang.DocumentLang.Should().Be("ja-JP");
            withMessageLang.Variant.Should().Be("");
            withMessageLang.MessageLang.Should().Be("en-US");
        }
    }
}
