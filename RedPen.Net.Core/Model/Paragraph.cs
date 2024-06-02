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
    public record class Paragraph
    {
        public List<Sentence> Sentences { get; init; }

        public Paragraph(List<Sentence> sentences)
        {
            this.Sentences = sentences;
        }

        public Paragraph() : this(new List<Sentence>())
        {
        }

        public Sentence GetSentence(int index)
        {
            return this.Sentences[index];
        }

        public Paragraph AppendSentence(string content, int lineNum)
        {
            // MEMO: センテンスは1行に1つというモデルなのか？
            // TODO: Mutableな実装なのでImmutableに書き換えられればそうする。
            this.Sentences.Add(new Sentence(content, lineNum));
            return this;
        }

        public Paragraph AppendSentence(Sentence sentence)
        {
            // MEMO: 追加されたSentenceと既存のSentenceの整合性は？
            this.Sentences.Add(sentence);
            return this;
        }

        public int GetNumberOfSentences()
        {
            return this.Sentences.Count;
        }
    }
}
