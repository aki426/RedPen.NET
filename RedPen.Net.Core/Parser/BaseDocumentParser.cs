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

using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedPen.Net.Core.Parser
{
    public abstract class BaseDocumentParser : IDocumentParser
    {
        /// <summary>BOM無しUTF-8エンコーディング</summary>
        protected UTF8Encoding utf8Encoding = new UTF8Encoding(false);

        public Document Parse(Stream inputStream, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer)
        {
            return Parse(inputStream, null, sentenceExtractor, tokenizer);
        }

        public Document Parse(string content, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer)
        {
            // BOM無しUTF-8エンコーディング
            // UTF8Encoding utf8Encoding = new System.Text.UTF8Encoding(false);
            byte[] byteArray = utf8Encoding.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                return Parse(stream, null, sentenceExtractor, tokenizer);
            }
        }

        public Document Parse(FileInfo file, SentenceExtractor sentenceExtractor, IRedPenTokenizer tokenizer)
        {
            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                return Parse(stream, file.FullName, sentenceExtractor, tokenizer);
            }

            // TODO: 例外処理を検討する。
        }

        /// <summary>
        /// Given input stream, return Document instance from a stream.
        /// </summary>
        /// <param name="inputStream">input stream containing input content</param>
        /// <param name="fileName"></param>
        /// <param name="sentenceExtractor"></param>
        /// <param name="tokenizer"></param>
        /// <returns></returns>
        public abstract Document Parse(
            Stream inputStream,
            string? fileName,
            SentenceExtractor sentenceExtractor,
            IRedPenTokenizer tokenizer);

        protected PreprocessingReader CreateReader(Stream inputStream)
        {
            return new PreprocessingReader(inputStream, this);
        }

        protected int SkipWhitespace(string line, int start)
        {
            for (int i = start; i < line.Length; i++)
            {
                if (char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
                continue;
            }

            return line.Length;
        }

        // protected static class ValueWithOffsets : Sentence
        protected record class ValueWithOffsets : Sentence
        {
            public ValueWithOffsets()
                : base("", 0)
            {
            }

            public ValueWithOffsets(string content, List<LineOffset> offsetMap)
                : base(content, offsetMap, new List<string>())
            {
            }

            public bool IsEmpty()
            {
                return string.IsNullOrEmpty(Content);
            }

            public ValueWithOffsets Append(string line, List<LineOffset> offsets) =>
                this with { Content = Content + line, OffsetMap = OffsetMap.Concat(offsets).ToList() };

            public ValueWithOffsets Extract(int start, int end)
            {
                if (start == end)
                {
                    return new ValueWithOffsets();
                }

                // MEMO: JAVA版はgetOffsetMap().subList(start, end)としており、startは含むがendは含まない。
                // そのため、C#ではGetRangeに与えるcountをend - startとしている。
                return new ValueWithOffsets(Content.Substring(start, end - start), OffsetMap.GetRange(start, end - start));
            }
        }
    }
}
