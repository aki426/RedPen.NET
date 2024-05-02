using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;

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

        public List<TokenElement> Tokenize(Sentence sentence)
        {
            List<TokenElement> tokens = new List<TokenElement>();

            string surface = "";
            int offset = 0;
            List<string> blankTag = new List<string>(); // このTokenizerではタグは使わないので常に空。

            // TODO: 現状、Tokenに原文の位置を渡せていないので、
            // TokenizeしたSurfaceの文字数をカウントしてSentence.GetOffsetから位置情報を取得してTokenに渡す。

            for (int i = 0; i < sentence.Content.Length; i++)
            {
                // 1文字ずつ処理。
                char ch = sentence.Content[i];
                if ((DELIMITERS.IndexOf(ch) != -1) || IsWrappedDelimiters(sentence.Content, i))
                {
                    // デリミタが出現したら直前までの文字列surfaceをTokenとして登録する。
                    if (IsSuitableToken(surface))
                    {
                        // surfaceはTokenのSurfaceとして適格。
                        tokens.Add(new TokenElement(
                            surface,
                            blankTag.ToImmutableList(),
                            surface,
                            // surfaceに対応するOffsetMapをSentenceから取得する。
                            Enumerable.Range(offset, surface.Length).Select(index => sentence.ConvertToLineOffset(index)).ToImmutableList()
                        ));
                    }
                    if (!char.IsWhiteSpace(ch) && ch != '\u00A0')
                    {
                        // デリミタとしての文字もTokenとして登録。
                        tokens.Add(new TokenElement(
                            ch.ToString(),
                            blankTag.ToImmutableList(),
                            ch.ToString(),
                            // surfaceに対応するOffsetMapをSentenceから取得する。
                            ImmutableList.Create(sentence.ConvertToLineOffset(i))
                        ));
                    }

                    surface = ""; // flush
                    offset = -1; // reset
                }
                else
                {
                    // MEMO: デリミタ関係の文字ではなかった場合はSurfaceとして積む。
                    if (offset < 0)
                    {
                        offset = i; // Surfaceの最初の位置を記録。
                    }
                    surface += ch;
                }
            }

            // 最後のSurfaceをTokenとして登録。
            if (IsSuitableToken(surface))
            {
                tokens.Add(new TokenElement(
                    surface,
                    blankTag.ToImmutableList(),
                    surface,
                    // surfaceに対応するOffsetMapをSentenceから取得する。
                    Enumerable.Range(offset, surface.Length).Select(index => sentence.ConvertToLineOffset(index)).ToImmutableList()
                ));
            }

            return tokens;
        }

        /// <summary>
        /// WRAPPED_DELIMITERSが半角スペースで囲まれていることを検証する関数。
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">対象文字のcontent内index</param>
        /// <returns>正しくラップされていればTrue</returns>
        private bool IsWrappedDelimiters(string content, int index)
        {
            char ch = content[index];
            return WRAPPED_DELIMITERS.IndexOf(ch) != -1
                && ((index > 0 && content[index - 1] == ' ') || (index < content.Length - 1 && content[index + 1] == ' '));
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
