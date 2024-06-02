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

using System.Globalization;
using FluentAssertions;
using RedPen.Net.Core.Errors;
using Xunit;

namespace RedPen.Net.Core.Tests.Errors
{
    public class ErrorMessageManagerTests
    {
        [Fact]
        public void BasicTest()
        {
            var manager = ErrorMessageManager.GetInstance();

            manager.GetErrorMessage(
                "SentenceLength",
                "",
                CultureInfo.GetCultureInfo("ja-JP"),
                new object[] { 30, 20 })
                    .Should().Be("文の長さ（30文字）が規定値（20文字）以上でした。");

            manager.GetErrorMessage(
                "SpaceWithAlphabeticalExpression",
                "After",
                CultureInfo.GetCultureInfo("ja-JP"),
                new object[] { "hogehoge" })
                    .Should().Be("半角アルファベット表現 \"hogehoge\" の後にスペースが必要です。");
        }
    }
}
