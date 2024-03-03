using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Ja.TokenAttributes;
using Lucene.Net.Analysis.TokenAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace redpen_core.tokenizer
{
    /// <summary>
    /// The neologd japanese tokenizer.
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
                null,
                false,
                JapaneseTokenizerMode.NORMAL);
        }

        /// <summary>
        /// Tokenizes the.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of TokenElements.</returns>
        public List<TokenElement> Tokenize(string sentence)
        {
            List<TokenElement> tokens = new List<TokenElement>();
            try
            {
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

        /// <summary>
        /// Kuromojis the neologd.
        /// </summary>
        /// <param name="src">The src.</param>
        /// <returns>A list of TokenElements.</returns>
        private List<TokenElement> KuromojiNeologd(string src)
        {
            tokenizer.SetReader(new StringReader(src));
            List<TokenElement> tokens = new List<TokenElement>();
            IBaseFormAttribute baseAttr = tokenizer.AddAttribute<IBaseFormAttribute>();
            ICharTermAttribute charAttr = tokenizer.AddAttribute<ICharTermAttribute>();
            IPartOfSpeechAttribute posAttr = tokenizer.AddAttribute<IPartOfSpeechAttribute>();
            IReadingAttribute readAttr = tokenizer.AddAttribute<IReadingAttribute>();
            IOffsetAttribute offsetAttr = tokenizer.AddAttribute<IOffsetAttribute>();
            IInflectionAttribute inflectionAttr = tokenizer.AddAttribute<IInflectionAttribute>();

            tokenizer.Reset();
            while (tokenizer.IncrementToken())
            {
                string surface = charAttr.ToString();
                tokens.Add(new TokenElement(surface,
                        GetTagList(posAttr, inflectionAttr),
                        offsetAttr.StartOffset,
                        readAttr.GetReading()

                ));
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