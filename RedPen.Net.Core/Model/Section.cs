﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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

using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Model
{
    // MEMO: JAVA版にあった、プロパティおよびListのメソッドで済む関数は削除した。

    public record class Section
    {
        /// <summary>Subsections</summary>
        public List<Section> Subsections { get; init; }

        public List<Paragraph> Paragraphs { get; init; }
        public List<ListBlock> ListBlocks { get; init; }
        public List<Sentence> HeaderSentences { get; init; }
        public int Level { get; set; }
        public Section? ParentSection { get; set; }

        public Section(int level, List<Sentence> headerSentences)
        {
            this.Level = level;
            this.ParentSection = null;

            this.Subsections = new List<Section>();
            this.Paragraphs = new List<Paragraph>();
            this.ListBlocks = new List<ListBlock>();
            this.HeaderSentences = headerSentences;
        }

        public Section(int level) : this(level, new List<Sentence>())
        {
        }

        public Section(int level, string headerString) :
            this(level, new List<Sentence> { new Sentence(headerString, 0) })
        { }

        public void AppendSubSection(Section section)
        {
            section.ParentSection = this;
            this.Subsections.Add(section);
        }

        public Sentence? GetHeaderSentence(int index)
        {
            if (this.HeaderSentences.Count > index)
            {
                return this.HeaderSentences[index];
            }

            return null;
        }

        /// <summary>
        /// Get iterator of header sentences.
        /// When there is not specified header in the section, return null otherwise return specified id.
        /// </summary>
        /// <returns>sentence containing all header contents in the section</returns>
        public Sentence GetJoinedHeaderContents()
        {
            if (!this.HeaderSentences.Any())
            {
                throw new System.InvalidOperationException("No header sentence found in the section.");
            }

            int lineNum = HeaderSentences.Count > 0 ? HeaderSentences[0].LineNumber : 0;

            return new Sentence(string.Join(" ", HeaderSentences.Select(s => s.Content)), lineNum);
        }

        public void AppendListElement(List<Sentence> contents, int level)
        {
            if (this.ListBlocks.Count == 0)
            {
                this.ListBlocks.Add(new ListBlock());
            }
            this.ListBlocks.Last().AppendListElement(contents, level);
        }

        public void AppendListBlock()
        {
            this.ListBlocks.Add(new ListBlock());
        }
    }
}
