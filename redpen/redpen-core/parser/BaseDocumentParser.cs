using System.Text;
using redpen_core.model;
using redpen_core.tokenizer;

namespace redpen_core.parser
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

            public ValueWithOffsets(string sentenceContent, int lineNum)
                : base(sentenceContent, lineNum)
            {
            }

            public ValueWithOffsets(string sentenceContent, int lineNum, int startOffset)
                : base(sentenceContent, lineNum, startOffset)
            {
            }

            public ValueWithOffsets(string content, List<LineOffset> offsetMap, List<string> links)
                : base(content, offsetMap, links)
            {
            }

            protected ValueWithOffsets(Sentence original)
                : base(original)
            {
            }

            public bool IsEmpty()
            {
                return string.IsNullOrEmpty(Content);
            }

            public ValueWithOffsets Append(string line, List<LineOffset> offsets)
            {
                Content += line;
                OffsetMap.AddRange(offsets);
                return this;
            }

            public ValueWithOffsets Extract(int start, int end)
            {
                if (start == end)
                {
                    return new ValueWithOffsets();
                }

                return new ValueWithOffsets(Content.Substring(start, end - start), OffsetMap[start..end]);
            }
        }
    }
}
