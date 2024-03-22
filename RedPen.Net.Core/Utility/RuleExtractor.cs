using System.Linq;
using System.Text.RegularExpressions;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Validator;

namespace RedPen.Net.Core.Utility
{
    /// <summary>
    /// RedPenのルール表記文字列からルールを抽出するクラス。
    /// </summary>
    public static class RuleExtractor
    {
        /// <summary>
        /// Create a rule from input sentence.
        /// </summary>
        /// <param name="line">line input rule</param>
        /// <returns>An ExpressionRule.</returns>
        public static ExpressionRule Run(string line)
        {
            string[] expressionSegments = Split(line);
            ExpressionRule rule = new ExpressionRule();
            foreach (string segment in expressionSegments)
            {
                string[] wordSegments = segment.Split(':');
                string surface = wordSegments[0];
                string tagStr = "";
                if (wordSegments.Length > 1)
                {
                    tagStr = wordSegments[1];
                }

                rule.Add(new TokenElement(surface, tagStr.Split(',').ToList(), 0));
            }
            return rule;
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
