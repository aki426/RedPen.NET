using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using RedPen.Net.Core.Model;
using System.Collections.Generic;
using System.IO;
using System;

using System.Collections.Generic;

using System.Linq;
using System.Collections.Immutable;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// The neologd japanese Tokenizer.
    /// </summary>
    public class NeologdJapaneseTokenizer : IRedPenTokenizer
    {
        private JapaneseTokenizer tokenizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NeologdJapaneseTokenizer"/> class.
        /// </summary>
        public NeologdJapaneseTokenizer()
        {
            // MEMO: 本来なら何らかのTextReaderが必要？
            this.tokenizer = new JapaneseTokenizer(
                new StringReader(string.Empty), // 空のTextReaderとして暫定的に与える。
                null, // TODO: Neologd辞書を使うための辞書の与え方を調べる。
                false,
                JapaneseTokenizerMode.NORMAL);
        }

        ///// <summary>
        ///// Tokenizes the.
        ///// </summary>
        ///// <param name="sentence">The sentence.</param>
        ///// <returns>A list of TokenElements.</returns>
        //public List<TokenElement> Tokenize(string sentence)
        //{
        //    List<TokenElement> tokens = new List<TokenElement>();
        //    try
        //    {
        //        foreach (TokenElement token in KuromojiNeologd(sentence))
        //        {
        //            tokens.Add(token);
        //        }
        //    }
        //    catch (IOException e)
        //    {
        //        // TODO: StackTraceを出力するように変更。
        //        throw;
        //    }

        //    return tokens;
        //}

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
                // TODO: 現状、Tokenに原文の位置を渡せていないので、
                // TokenizeしたSurfaceの文字数をカウントしてSentence.GetOffsetから位置情報を取得してTokenに渡す。
                foreach (TokenElement token in KuromojiNeologd(sentence))
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

        ///// <summary>
        ///// Kuromojis the neologd.
        ///// </summary>
        ///// <param name="src">The src.</param>
        ///// <returns>A list of TokenElements.</returns>
        //private List<TokenElement> KuromojiNeologd(string src)
        //{
        //    tokenizer.SetReader(new StringReader(src));
        //    List<TokenElement> tokens = new List<TokenElement>();
        //    IBaseFormAttribute baseAttr = tokenizer.AddAttribute<IBaseFormAttribute>();
        //    ICharTermAttribute charAttr = tokenizer.AddAttribute<ICharTermAttribute>();
        //    IPartOfSpeechAttribute posAttr = tokenizer.AddAttribute<IPartOfSpeechAttribute>();
        //    IReadingAttribute readAttr = tokenizer.AddAttribute<IReadingAttribute>();
        //    IOffsetAttribute offsetAttr = tokenizer.AddAttribute<IOffsetAttribute>();
        //    IInflectionAttribute inflectionAttr = tokenizer.AddAttribute<IInflectionAttribute>();

        //    tokenizer.Reset();
        //    while (tokenizer.IncrementToken())
        //    {
        //        string surface = charAttr.ToString();
        //        tokens.Add(new TokenElement(surface,
        //                GetTagList(posAttr, inflectionAttr),
        //                0,
        //                offsetAttr.StartOffset,
        //                readAttr.GetReading()

        //        ));
        //    }
        //    tokenizer.Dispose();

        //    return tokens;
        //}

        /// <summary>
        /// Kuromojis the neologd.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>A list of TokenElements.</returns>
        private List<TokenElement> KuromojiNeologd(Sentence src)
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
                    // surfaceに対応するOffsetMapをSentenceから取得する。
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
