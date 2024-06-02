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

using System.Collections.Generic;

namespace RedPen.Net.Core.Model
{
    public record class ListElement
    {
        public List<Sentence> Sentences { get; init; }
        public int Level { get; init; }

        public ListElement(List<Sentence> sentences, int level)
        {
            Sentences = sentences;
            Level = level;
        }

        public Sentence GetSentence(int index)
        {
            return this.Sentences[index];
        }

        public int GetNumberOfSentences()
        {
            return this.Sentences.Count;
        }
    }
}
