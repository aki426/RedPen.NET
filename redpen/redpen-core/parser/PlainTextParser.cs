using redpen_core.model;
using redpen_core.tokenizer;

namespace redpen_core.parser
{
    public sealed class PlainTextParser : BaseDocumentParser
    {
        public override Document Parse(
            Stream inputStream,
            string? fileName,
            SentenceExtractor sentenceExtractor,
            IRedPenTokenizer tokenizer)
        {
            DocumentBuilder documentBuilder = Document.Builder(tokenizer);
            // JAVA版ではOptionalを使っているが、C#版ではnull許容参照型を使っているのでそのまま代入する。
            documentBuilder.FileName = fileName;

            List<Sentence> headers = new List<Sentence>();
            headers.Add(new Sentence("", 0));
            documentBuilder.AddSection(0, headers);
            documentBuilder.AddParagraph();

            PreprocessingReader br = CreateReader(inputStream);
            string line;
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

        //public static LineOffset addSentence(
        //    LineOffset lineOffset,
        //    string rawSentenceText,
        //    SentenceExtractor sentenceExtractor,
        //    DocumentBuilder builder)
        //{
        //    int lineNum = lineOffset.LineNum;
        //    int offset = lineOffset.Offset;

        //    int sentenceStartLineNum = lineNum;
        //    int sentenceStartLineOffset = offset;

        //    List<LineOffset> offsetMap = new List<LineOffset>();
        //    string normalizedSentence = "";
        //    int i; // MEMO: 連続する2つのfor文用。
        //    // skip leading line breaks to find the start line of the sentence
        //    for (i = 0; i < rawSentenceText.Length; i++)
        //    {
        //        char ch = rawSentenceText[i];
        //        // 改行コードはLF。
        //        if (ch == '\n')
        //        {
        //            sentenceStartLineNum++;
        //            lineNum++;
        //            sentenceStartLineOffset = 0;
        //            offset = 0;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    for (; i < rawSentenceText.Length; i++)
        //    {
        //        char ch = rawSentenceText[i];
        //        if (ch == '\n')
        //        {
        //            if (!sentenceExtractor.GetBrokenLineSeparator().isEmpty())
        //            {
        //                offsetMap.add(new LineOffset(lineNum, offset));
        //                normalizedSentence += sentenceExtractor.getBrokenLineSeparator();
        //            }
        //            lineNum++;
        //            offset = 0;
        //        }
        //        else
        //        {
        //            normalizedSentence += ch;
        //            offsetMap.Add(new LineOffset(lineNum, offset));
        //            offset++;
        //        }
        //    }
        //    Sentence sentence = new Sentence(normalizedSentence, sentenceStartLineNum, sentenceStartLineOffset);
        //    sentence = sentence with { OffsetMap = offsetMap };
        //    // sentence.OffsetMap = offsetMap;
        //    builder.AddSentence(sentence);

        //    return new LineOffset(lineNum, offset);
        //}

        private void ExtractSentences(
            int lineNum,
            string paragraphText,
            SentenceExtractor sentenceExtractor,
            DocumentBuilder builder)
        {
            //int periodPosition = sentenceExtractor.GetSentenceEndPosition(paragraphText);
            //LineOffset lineOffset = new LineOffset(lineNum, 0);
            //if (periodPosition == -1)
            //{
            //    addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
            //}
            //else
            //{
            //    while (true)
            //    {
            //        lineOffset = addSentence(lineOffset, paragraphText.substring(0, periodPosition + 1), sentenceExtractor, builder);
            //        paragraphText = paragraphText.substring(periodPosition + 1, paragraphText.length());
            //        periodPosition = sentenceExtractor.getSentenceEndPosition(paragraphText);
            //        if (periodPosition == -1)
            //        {
            //            if (!paragraphText.isEmpty())
            //            {
            //                addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
            //            }
            //            return;
            //        }
            //    }
            //}
        }
    }
}
