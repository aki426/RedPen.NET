using System.Collections.Generic;
using System.IO;
using System.Text;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Parser
{
    // MEMO: Parserは文章構造を解析しDocumentへ変換するクラス。
    // 一方、TokenizerはSentenceオブジェクトを取り文章を単語や句読点などのTokenElementに分割するクラス。
    // Document >>> Sentence > TokenElementという構造になる。

    /// <summary>The plain text parser.</summary>
    public sealed class PlainTextParser : BaseDocumentParser
    {
        /// <summary>
        /// Parse input stream as document.
        /// NOTE: 改行コードはLFのみを想定。
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

            // MEMO: 改行コードと空文字列を区切りとして、パラグラフ全体の文字列とパラグラフ開始行を取得する。
            PreprocessingReader br = CreateReader(inputStream);
            string? line;
            int linesRead = 0;
            int paragraphStartLine = 1;
            string paragraphText = "";
            try
            {
                while ((line = br.ReadLine()) != null)
                {
                    linesRead++; // 原文の行カウンター。1行目開始。
                    if (line.Equals(""))
                    {
                        if (paragraphText != string.Empty)
                        {
                            // パラグラフに対してSentenceのListを生成する。
                            this.ExtractSentences(paragraphStartLine, paragraphText, sentenceExtractor, documentBuilder);
                        }

                        paragraphStartLine = linesRead + 1; // 空行が来たので、次のパラグラフ開始行は現在業の次の行とみなす。
                        documentBuilder.AddParagraph(); // TODO: 空文字列が連続すると、空のパラグラフが追加され続ける？
                        paragraphText = "";
                    }
                    else
                    {
                        // MEMO: 改行コードをデリミタとして、文字列を詰め込んでいき、パラグラフ全体の文字列を作成する。
                        paragraphText += (string.IsNullOrEmpty(paragraphText) ? "" : "\n") + line;
                    }
                }
            }
            catch (IOException e)
            {
                throw new RedPenException(e);
            }

            if (!string.IsNullOrEmpty(paragraphText))
            {
                this.ExtractSentences(paragraphStartLine, paragraphText, sentenceExtractor, documentBuilder);
            }

            documentBuilder.SetPreprocessorRules(br.PreprocessorRules);

            return documentBuilder.Build();
        }

        /// <summary>
        /// Extracts the sentences.
        /// </summary>
        /// <param name="paragraphStartLineNum">パラグラフの開始行</param>
        /// <param name="paragraphText">パラグラフ全体のテキスト。改行コードは"\n"へ変換されている。</param>
        /// <param name="sentenceExtractor">The sentence extractor.</param>
        /// <param name="builder">The builder.</param>
        private void ExtractSentences(
            int paragraphStartLineNum,
            string paragraphText,
            SentenceExtractor sentenceExtractor,
            DocumentBuilder builder)
        {
            LineOffset lineOffset = new LineOffset(paragraphStartLineNum, 0);

            // paragraphTextの中の終了位置を取得する。
            int periodPosition = sentenceExtractor.GetSentenceEndPosition(paragraphText);
            if (periodPosition == -1)
            {
                //addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
                var value = CreateSentence(lineOffset, paragraphText, sentenceExtractor.getBrokenLineSeparator());
                builder.AddSentence(value.sentence);
            }
            else
            {
                // paragraphTextイコール１センテンスではない場合、句点で区切らなければならない。
                while (true)
                {
                    // MEMO: periodPositionが文字列の長さを超える値だった場合、Java ではStringIndexOutOfBoundsExceptionが発生しますが、
                    // C#では単に文字列の終端までの部分文字列が返されます。
                    //lineOffset = addSentence(lineOffset, paragraphText.Substring(0, periodPosition + 1), sentenceExtractor, builder);
                    var value = CreateSentence(lineOffset, paragraphText.Substring(0, periodPosition + 1), sentenceExtractor.getBrokenLineSeparator());
                    builder.AddSentence(value.sentence);
                    lineOffset = value.nextStartOffset;

                    paragraphText = paragraphText.Substring(periodPosition + 1, paragraphText.Length - (periodPosition + 1));

                    periodPosition = sentenceExtractor.GetSentenceEndPosition(paragraphText);
                    if (periodPosition == -1)
                    {
                        // paragraphTextの次のセンテンスの終了位置がない＝残りは1センテンスの可能性。
                        if (paragraphText != string.Empty)
                        {
                            //addSentence(lineOffset, paragraphText, sentenceExtractor, builder);
                            var value2 = CreateSentence(lineOffset, paragraphText, sentenceExtractor.getBrokenLineSeparator());
                            builder.AddSentence(value2.sentence);
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Sentenceインスタンスを生成し、DocumentBuilderに追加する。
        /// </summary>
        /// <param name="lineOffset">rawSentenceTextの先頭位置の行＋オフセット位置</param>
        /// <param name="rawSentenceText">改行を含む可能性がある、句点で区切られた１センテンスのテキスト</param>
        /// <param name="sentenceExtractor">The sentence extractor.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>終了位置＝次のセンテンスの開始位置</returns>
        public static (Sentence sentence, LineOffset nextStartOffset) CreateSentence(
            LineOffset lineOffset,
            string rawSentenceText,
            string brokenLineSeparator // 改行によってブレークされたSentenceを連結する際の文字列。
        )
        {
            int lineNum = lineOffset.LineNum; // 現在処理中の元テキストの行番号。
            int offset = lineOffset.Offset;

            //int sentenceStartLineNum = lineNum;
            //int sentenceStartLineOffset = offset;

            List<LineOffset> offsetMap = new List<LineOffset>();
            string normalizedSentence = ""; // 改行などを除去したSentence.Contentに格納される文字列。
            StringBuilder sb = new StringBuilder();
            int i; // MEMO: 連続する2つのfor文用。

            // skip leading line breaks to find the start line of the sentence
            // MEMO: ムダなように見えるが、冒頭の改行スキップのため必要。
            for (i = 0; i < rawSentenceText.Length; i++)
            {
                char ch = rawSentenceText[i];
                // 改行コードはLF。
                if (ch == '\n')
                {
                    //sentenceStartLineNum++; // 改行により行数をカウントアップ。
                    lineNum++; // 改行により行数をカウントアップ。
                    //sentenceStartLineOffset = 0; // 改行によりオフセット位置を初期化。
                    offset = 0; // 改行によりオフセット位置を初期化。
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
                    // 改行されたセンテンスを再結合するための文字列を取得。
                    if (brokenLineSeparator != string.Empty)
                    {
                        // 通常brokenLineSeparatorは半角スペースか空文字列だが、念のためリストを生成する。
                        offsetMap.AddRange(LineOffset.MakeOffsetList(lineNum, offset, brokenLineSeparator));
                        normalizedSentence += brokenLineSeparator;
                        sb.Append(brokenLineSeparator);
                    }
                    lineNum++; // 改行により行数をカウントアップ。
                    offset = 0;
                }
                else
                {
                    // 1文字ずつ元テキストでの位置を記録。
                    normalizedSentence += ch;
                    sb.Append(ch);
                    offsetMap.Add(new LineOffset(lineNum, offset));
                    offset++; // 次の文字のオフセット位置へカウントアップ。※このまま次のセンテンスのオフセット位置を指すこともできる。
                }
            }

            // Sentence生成。
            Sentence sentence = new Sentence(normalizedSentence, offsetMap, new List<string>());
            //sentence = sentence with { OffsetMap = offsetMap };
            //builder.AddSentence(sentence);

            return (sentence, new LineOffset(lineNum, offset)); // 終了位置＝次のセンテンスの開始位置を返す。
        }
    }
}
