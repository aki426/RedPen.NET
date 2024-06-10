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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RedPen.Net.Core.Model.Tests
{
    /// <summary>
    /// The section tests.
    /// </summary>
    public class SectionTests
    {
        /// <summary>
        /// ヘッダーが複数センテンスを持つ場合の検証。
        /// </summary>
        [Fact()]
        public void GetJoinedHeaderContentsTest()
        {
            Section section;

            // Headerが1つの場合
            section = new Section(0, "header string");

            section.GetJoinedHeaderContents().Content.Should().Be("header string");
            section.GetJoinedHeaderContents().LineNumber.Should().Be(0);

            // 複数SentenceHeaderの場合
            List<Sentence> headers = new List<Sentence>()
            {
                new Sentence("header1.", 0),
                new Sentence("header2.", 0),
                new Sentence("header3.", 0),
            };

            section = new Section(0, headers);
            section.GetJoinedHeaderContents().Content.Should().Be("header1. header2. header3.");
            section.GetJoinedHeaderContents().LineNumber.Should().Be(0);

            // Headerがない場合
            section = new Section(0);
            section.HeaderSentences.Any().Should().BeFalse();

            Action act = () => section.GetJoinedHeaderContents();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("No header sentence found in the section.");
        }
    }
}
