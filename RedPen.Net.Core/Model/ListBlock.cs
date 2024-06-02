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
    public record class ListBlock
    {
        public List<ListElement> ListElements { get; init; }

        public ListBlock()
        {
            this.ListElements = new List<ListElement>();
        }

        public int GetNumberOfListElements()
        {
            return this.ListElements.Count;
        }

        public ListElement GetListElement(int index)
        {
            return this.ListElements[index];
        }

        /// <summary>
        /// Append ListElement.
        /// </summary>
        /// <param name="contents">contents of list element</param>
        /// <param name="level">indentation level</param>
        public void AppendListElement(List<Sentence> contents, int level)
        {
            this.ListElements.Add(new ListElement(contents, level));
        }
    }
}
