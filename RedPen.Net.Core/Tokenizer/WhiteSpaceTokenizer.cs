using System.Collections.Generic;
using System.Text.RegularExpressions;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Tokenizer
{
    /// <summary>
    /// The white space Tokenizer.
    /// MEMO: 半角空白をデリミタとする英語系の言語のためのトークナイザと思われる。
    /// </summary>
    public class WhiteSpaceTokenizer : IRedPenTokenizer
    {
        private static readonly List<Regex> DENYLIST_TOKEN_PATTERNS = new List<Regex>()
        {
            new Regex(@"^[-+#$€£¥]?\d+(\.\d+)?[%€¥¢₽]?$")
        };

        private static readonly string DELIMITERS = " \u00A0\t\n\r?!,:;.()\u2014\"";
        private static readonly string WRAPPED_DELIMITERS = "\'"; // delimiters need to wrapped with white spaces

        /// <summary>
        /// Initializes a new instance of the <see cref="WhiteSpaceTokenizer"/> class.
        /// </summary>
        public WhiteSpaceTokenizer()
        {
        }

        ///// <summary>
        ///// Tokenizes the.
        ///// </summary>
        ///// <param name="sentence">The sentence.</param>
        ///// <returns>A list of TokenElements.</returns>
        //public List<TokenElement> Tokenize(string sentence)
        //{
        //    List<TokenElement> tokens = new List<TokenElement>();

        //    string surface = "";
        //    int offset = 0;
        //    List<string> tags = new List<string>();

        //    for (int i = 0; i < sentence.Length; i++)
        //    {
        //        char ch = sentence[i];
        //        if ((DELIMITERS.IndexOf(ch) != -1) || IsWrappedDelimiters(sentence, i))
        //        {
        //            if (IsSuitableToken(surface))
        //            {
        //                tokens.Add(new TokenElement(surface, tags, 0, offset));
        //            }
        //            if (!char.IsWhiteSpace(ch) && ch != '\u00A0')
        //            {
        //                tokens.Add(new TokenElement(ch.ToString(), tags, 0, i));
        //            }
        //            surface = "";
        //            offset = -1;
        //        }
        //        else
        //        {
        //            if (offset < 0)
        //            {
        //                offset = i;
        //            }
        //            surface += ch;
        //        }
        //    }

        //    if (IsSuitableToken(surface))
        //    {
        //        tokens.Add(new TokenElement(surface, tags, 0, offset));
        //    }

        //    return tokens;
        //}

        public List<TokenElement> Tokenize(Sentence sentence)
        {
            List<TokenElement> tokens = new List<TokenElement>();

            string surface = "";
            int offset = 0;
            List<string> tags = new List<string>();

            // TODO: 現状、Tokenに原文の位置を渡せていないので、
            // TokenizeしたSurfaceの文字数をカウントしてSentence.GetOffsetから位置情報を取得してTokenに渡す。

            for (int i = 0; i < sentence.Content.Length; i++)
            {
                char ch = sentence.Content[i];
                if ((DELIMITERS.IndexOf(ch) != -1) || IsWrappedDelimiters(sentence.Content, i))
                {
                    if (IsSuitableToken(surface))
                    {
                        tokens.Add(new TokenElement(surface, tags, sentence.LineNumber, offset));
                    }
                    if (!char.IsWhiteSpace(ch) && ch != '\u00A0')
                    {
                        tokens.Add(new TokenElement(ch.ToString(), tags, sentence.LineNumber, i));
                    }
                    surface = "";
                    offset = -1;
                }
                else
                {
                    if (offset < 0)
                    {
                        offset = i;
                    }
                    surface += ch;
                }
            }

            if (IsSuitableToken(surface))
            {
                tokens.Add(new TokenElement(surface, tags, sentence.LineNumber, offset));
            }

            return tokens;
        }

        /// <summary>
        /// Are the wrapped delimiters.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="i">The i.</param>
        /// <returns>A bool.</returns>
        private bool IsWrappedDelimiters(string content, int i)
        {
            char ch = content[i];
            return WRAPPED_DELIMITERS.IndexOf(ch) != -1
                && ((i > 0 && content[i - 1] == ' ') || (i < content.Length - 1 && content[i + 1] == ' '));
        }

        /// <summary>
        /// Are the suitable token.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <returns>A bool.</returns>
        private bool IsSuitableToken(string surface)
        {
            if (surface == string.Empty)
            {
                return false;
            }

            foreach (Regex pattern in DENYLIST_TOKEN_PATTERNS)
            {
                if (pattern.IsMatch(surface))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
