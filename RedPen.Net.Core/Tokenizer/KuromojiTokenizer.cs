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
    /// Lucene.Net.Analysis.Kuromojiをラップした日本語用Tokenizer。
    /// Neologd辞書はデフォルトでは保持しておらず、使用している辞書はMeCab-IPADIC辞書である。
    /// </summary>
    public class KuromojiTokenizer : IRedPenTokenizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KuromojiTokenizer"/> class.
        /// </summary>
        public KuromojiTokenizer()
        {
        }

        /// <summary>
        /// Tokenize a sentence.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of TokenElements.</returns>
        public List<TokenElement> Tokenize(Sentence sentence) => KuromojiController.Tokenize(sentence);
    }
}
