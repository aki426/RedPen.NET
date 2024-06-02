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
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// The red pen Tokenizer.
    /// </summary>
    public interface IRedPenTokenizer
    {
        /// <summary>
        /// Tokenize input sentence into tokens.
        /// </summary>
        /// <param name="sentence">input sentence</param>
        /// <returns>a set of tokens in the input sentence</returns>
        public List<TokenElement> Tokenize(Sentence sentence);

        // public List<TokenElement> Tokenize(string sentence);
    }
}
