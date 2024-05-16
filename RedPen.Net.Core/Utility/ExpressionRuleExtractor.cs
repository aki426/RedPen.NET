using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// RedPenのルール表記文字列からルールを抽出するクラス。
    /// </summary>
    public static class ExpressionRuleExtractor
    {
        /// <summary>
        /// RuleExpressionを定義された複数行テキストからExpressionRuleのリストを読み込む。
        /// MEMO: 冒頭に#記号があるばあいはコメントとして無視される。
        /// </summary>
        /// <param name="ruleDefinition">テキストファイルの中身などの複数行テキスト</param>
        /// <returns>A list of ExpressionRules.</returns>
        public static List<ExpressionRule> LoadExpressionRules(string ruleDefinition)
        {
            return ruleDefinition.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line != "" && line.Trim()[0] != '#')
                .Select(line => Run(line)).ToList();
        }

        /// <summary>
        /// Converts string to token.
        /// </summary>
        /// <param name="str">ルール表記におけるToken1つ分の文字列「Surface:タグ,タグ,タグ,...:Reading」</param>
        /// <returns>A TokenElement.</returns>
        public static TokenElement ConvertToToken(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            else if (str.Trim() == "")
            {
                throw new ArgumentException("Expected TokenElement's text expression. But str is empty.", nameof(str));
            }

            // Surface:タグ,タグ,タグ,...:Readingを要素ごとに分割。
            string[] wordSegments = str.Split(':');

            string surface = wordSegments[0].Trim().ToLower();
            string tagStr = wordSegments.Length > 1 ? wordSegments[1] : "";
            string readingStr = wordSegments.Length > 2 ? wordSegments[2].Trim() : "";

            return new TokenElement(surface, tagStr.Split(',').Select(t => t.Trim()).ToList(), 0, 0, readingStr);
        }

        /// <summary>
        /// Create a rule from input sentence.
        /// </summary>
        /// <param name="line">ルール表記文字列は「Surface:タグ,タグ,タグ,...:Reading + Surface:タグ,...:Reading + ...」という形式。</param>
        /// <returns>An ExpressionRule.</returns>
        public static ExpressionRule Run(string line)
        {
            if (line.Trim() == "")
            {
                throw new ArgumentException("Invalid rule format. Rule expression is empty.", nameof(line));
            }

            List<(bool direct, TokenElement token)> result = new List<(bool direct, TokenElement token)>();
            StringBuilder sb = new StringBuilder();
            bool tokenIsDirect = false; // 最初のトークンの直接接続フラグは何でもよいが、処理の一貫性からFalseにしておく。

            foreach (char c in line)
            {
                if (c == '=')
                {
                    if (sb.Length == 0)
                    {
                        throw new ArgumentException("Invalid rule format. One token must exist before '='.", nameof(line));
                    }

                    result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

                    // iteration
                    // NOTE: '='の次のトークンは、離れた場所に出現しても良いルール。
                    tokenIsDirect = false;
                    sb.Clear();
                }
                else if (c == '+')
                {
                    if (sb.Length == 0)
                    {
                        throw new ArgumentException("Invalid rule format. One token must exist before '+'.", nameof(line));
                    }

                    result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

                    // iteration
                    // NOTE: '+'の次のトークンは、直前のトークンに続いて出現する必要があるルール。
                    tokenIsDirect = true;
                    sb.Clear();
                }
                else if (c == ' ')
                {
                    // MEMO: 空白文字は無視する。
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length == 0)
            {
                throw new ArgumentException("Invalid rule format. One token must exist after the last '+' or '='.", nameof(line));
            }

            result.Add((tokenIsDirect, ConvertToToken(sb.ToString())));

            return new ExpressionRule()
            {
                TokenPattern = result.ToImmutableList()
            };
        }

        /// <summary>
        /// RedPenのルール表記文字列を分割する。
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>An array of string.</returns>
        public static string[] SplitToRuleElements(string line)
        {
            var segments = Regex.Split(line, @" *[+] *");
            return segments;
        }
    }
}
