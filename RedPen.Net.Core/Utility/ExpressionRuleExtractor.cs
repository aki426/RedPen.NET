using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        /// Create a rule from input sentence.
        /// </summary>
        /// <param name="line">ルール表記文字列は「Surface:タグ,タグ,タグ,...」という形式。</param>
        /// <returns>An ExpressionRule.</returns>
        public static ExpressionRule Run(string line)
        {
            string[] expressionSegments = Split(line);
            List<TokenElement> tokens = new List<TokenElement>();

            foreach (string segment in expressionSegments)
            {
                string[] wordSegments = segment.Split(':');
                string surface = wordSegments[0];
                string tagStr = wordSegments.Length > 1 ? wordSegments[1] : "";

                // MEMO: ExpressionRuleのTokenElementは特殊なので位置指定が実際の出現位置と結びついていない。
                tokens.Add(new TokenElement(surface, tagStr.Split(',').ToList(), 0, 0));
            }

            return new ExpressionRule()
            {
                Tokens = tokens.ToImmutableList()
            };
        }

        /// <summary>
        /// RedPenのルール表記文字列を分割する。
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>An array of string.</returns>
        public static string[] Split(string line)
        {
            var segments = Regex.Split(line, @" *[+] *");
            return segments;
        }
    }
}
