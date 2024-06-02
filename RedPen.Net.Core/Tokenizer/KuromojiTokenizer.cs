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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// Lucene.Net.Analysis.Kuromojiをラップした日本語用Tokenizer。
    /// Neologd辞書はデフォルトでは保持しておらず、使用している辞書はMeCab-IPADIC辞書である。
    /// </summary>
    public class KuromojiTokenizer : IRedPenTokenizer
    {
        private JapaneseTokenizer tokenizer;

        // TODO: JapaneseTokenizerの生成コストが気になる。Singleton化して生成コストを削減することを検討する。

        /// <summary>
        /// Initializes a new instance of the <see cref="KuromojiTokenizer"/> class.
        /// </summary>
        public KuromojiTokenizer()
        {
            // MEMO: 本来なら何らかのTextReaderが必要？
            this.tokenizer = new JapaneseTokenizer(
                new StringReader(string.Empty), // 空のTextReaderとして暫定的に与える。
                null, // TODO: Neologd辞書を使うための辞書の与え方を調べる。
                false,
                JapaneseTokenizerMode.NORMAL);
        }

        /// <summary>
        /// Tokenize a sentence.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of TokenElements.</returns>
        public List<TokenElement> Tokenize(Sentence sentence)
        {
            List<TokenElement> tokens = new List<TokenElement>();
            try
            {
                foreach (TokenElement token in GetKuromojiTokens(sentence))
                {
                    tokens.Add(token);
                }
            }
            catch (IOException e)
            {
                // TODO: StackTraceを出力するように変更。
                throw;
            }

            return tokens;
        }

        /// <summary>
        /// Kuromojis the neologd.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>A list of TokenElements.</returns>
        private List<TokenElement> GetKuromojiTokens(Sentence src)
        {
            tokenizer.SetReader(new StringReader(src.Content));
            List<TokenElement> tokens = new List<TokenElement>();
            IBaseFormAttribute baseAttr = tokenizer.AddAttribute<IBaseFormAttribute>();
            ICharTermAttribute charAttr = tokenizer.AddAttribute<ICharTermAttribute>();
            IPartOfSpeechAttribute posAttr = tokenizer.AddAttribute<IPartOfSpeechAttribute>();
            IReadingAttribute readAttr = tokenizer.AddAttribute<IReadingAttribute>();
            IOffsetAttribute offsetAttr = tokenizer.AddAttribute<IOffsetAttribute>();
            IInflectionAttribute inflectionAttr = tokenizer.AddAttribute<IInflectionAttribute>();

            tokenizer.Reset();
            int currentOffset = 0;
            while (tokenizer.IncrementToken())
            {
                string surface = charAttr.ToString();

                tokens.Add(new TokenElement(
                    surface,
                    GetTagList(posAttr, inflectionAttr).ToImmutableList(),
                    //src.LineNumber,
                    //offsetAttr.StartOffset,
                    readAttr.GetReading(),
                    // surfaceに対応するOffsetMapをSentenceから取得する。TokenElementは正確に原文の出現位置を持つ。
                    Enumerable.Range(currentOffset, surface.Length).Select(i => src.ConvertToLineOffset(i)).ToImmutableList()
                ));

                // iteration.
                currentOffset += surface.Length;
            }
            tokenizer.Dispose();

            return tokens;
        }

        /// <summary>
        /// Gets the tag list.
        /// </summary>
        /// <param name="posAttr">The pos attr.</param>
        /// <param name="inflectionAttr">The inflection attr.</param>
        /// <returns>A list of string.</returns>
        private static List<string> GetTagList(IPartOfSpeechAttribute posAttr, IInflectionAttribute inflectionAttr)
        {
            List<string> posList = new List<string>();
            posList.AddRange(posAttr.GetPartOfSpeech().Split('-'));
            string form = inflectionAttr.GetInflectionForm() == null ? "*" : inflectionAttr.GetInflectionForm();
            string type = inflectionAttr.GetInflectionType() == null ? "*" : inflectionAttr.GetInflectionType();
            posList.Add(type);
            posList.Add(form);
            return posList;
        }
    }
}
