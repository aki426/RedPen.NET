﻿using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Parser
{
    /// <summary>
    /// The plain text parser.
    /// </summary>
    public sealed class PlainTextParser : BaseDocumentParser
    {
        /// <summary>
        /// Parses the.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="sentenceExtractor">The sentence extractor.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <returns>A Document.</returns>
        public override Document Parse(
            Stream inputStream,
            string? fileName,
            SentenceExtractor sentenceExtractor,
            IRedPenTokenizer tokenizer)
        {
            DocumentBuilder documentBuilder = Document.Builder(tokenizer);
            // JAVA版ではOptionalを使っているが、C#版ではnull許容参照型を使っているのでそのまま代入する。
            documentBuilder.FileName = fileName;

            List<Sentence> headers = new List<Sentence>() { new Sentence("", 0) }; // PlainTextではヘッダー無し。
            documentBuilder.AddSection(0, headers);
            documentBuilder.AddParagraph();

            PreprocessingReader br = CreateReader(inputStream);
            string? line;
            int linesRead = 0;
            int startLine = 1;
            string paragraph = "";
            try
            {
                while ((line = br.ReadLine()) != null)
                {
                    linesRead++;
                    if (line.Equals(""))
                    {
                        if (paragraph != string.Empty)
                        {
                            this.ExtractSentences(startLine, paragraph, sentenceExtractor, documentBuilder);
                        }
                        startLine = linesRead + 1;
                        documentBuilder.AddParagraph();
                        paragraph = "";
                    }
                    else
                    {
                        paragraph += (string.IsNullOrEmpty(paragraph) ? "" : "\n") + line;
                    }
                }
            }
            catch (IOException e)
            {
                throw new RedPenException(e);
            }

            if (!string.IsNullOrEmpty(paragraph))
            {
                this.ExtractSentences(startLine, paragraph, sentenceExtractor, documentBuilder);
            }

            documentBuilder.SetPreprocessorRules(br.PreprocessorRules);

            return documentBuilder.Build();
        }

        /// <summary>
        /// Extracts the sentences.
        /// </summary>
        /// <param name="lineNum">The line num.</param>
        /// <param name="paragraphText">The paragraph text.</param>
        /// <param name="sentenceExtractor">The sentence extractor.</param>
        /// <param name="builder">The builder.</param>
        private void ExtractSentences(
            int lineNum,
            string paragraphText,
            SentenceExtractor sentenceExtractor,
            DocumentBuilder builder)
        {
            LineOffset lineOffset = new LineOffset(lineNum, 0);

            int periodPosition = sentenceExtractor.GetSentenceEndPosition(paragraphText);
            if (periodPosition == -1)
            {
                addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
            }
            else
            {
                while (true)
                {
                    // MEMO: periodPositionが文字列の長さを超える値だった場合、Java ではStringIndexOutOfBoundsExceptionが発生しますが、
                    // C#では単に文字列の終端までの部分文字列が返されます。
                    lineOffset = addSentence(lineOffset, paragraphText.Substring(0, periodPosition + 1), sentenceExtractor, builder);
                    paragraphText = paragraphText.Substring(periodPosition + 1, paragraphText.Length - (periodPosition + 1));

                    periodPosition = sentenceExtractor.GetSentenceEndPosition(paragraphText);
                    if (periodPosition == -1)
                    {
                        if (paragraphText != string.Empty)
                        {
                            addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
                        }
                        return;
                    }
                }
            }
        }

        public static LineOffset addSentence(
            LineOffset lineOffset,
            string rawSentenceText,
            SentenceExtractor sentenceExtractor,
            DocumentBuilder builder)
        {
            int lineNum = lineOffset.LineNum;
            int offset = lineOffset.Offset;

            int sentenceStartLineNum = lineNum;
            int sentenceStartLineOffset = offset;

            List<LineOffset> offsetMap = new List<LineOffset>();
            string normalizedSentence = "";
            int i; // MEMO: 連続する2つのfor文用。
            // skip leading line breaks to find the start line of the sentence
            for (i = 0; i < rawSentenceText.Length; i++)
            {
                char ch = rawSentenceText[i];
                // 改行コードはLF。
                if (ch == '\n')
                {
                    sentenceStartLineNum++;
                    lineNum++;
                    sentenceStartLineOffset = 0;
                    offset = 0;
                }
                else
                {
                    break;
                }
            }
            for (; i < rawSentenceText.Length; i++)
            {
                char ch = rawSentenceText[i];
                if (ch == '\n')
                {
                    if (sentenceExtractor.getBrokenLineSeparator() != string.Empty)
                    {
                        offsetMap.Add(new LineOffset(lineNum, offset));
                        normalizedSentence += sentenceExtractor.getBrokenLineSeparator();
                    }
                    lineNum++;
                    offset = 0;
                }
                else
                {
                    normalizedSentence += ch;
                    offsetMap.Add(new LineOffset(lineNum, offset));
                    offset++;
                }
            }
            Sentence sentence = new Sentence(normalizedSentence, sentenceStartLineNum, sentenceStartLineOffset);
            sentence = sentence with { OffsetMap = offsetMap };
            // sentence.OffsetMap = offsetMap;
            builder.AddSentence(sentence);

            return new LineOffset(lineNum, offset);
        }
    }
}
