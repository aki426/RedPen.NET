using System;
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
        /// Create a rule from input sentence.
        /// </summary>
        /// <param name="line">ルール表記文字列は「Surface:タグ,タグ,タグ,...:Reading + Surface:タグ,...:Reading + ...」という形式。</param>
        /// <returns>An ExpressionRule.</returns>
        public static ExpressionRule Run(string line)
        {
            string[] expressionSegments = SplitToRuleElements(line);
            List<TokenElement> tokens = new List<TokenElement>();

            foreach (string segment in expressionSegments)
            {
                string[] wordSegments = segment.Split(':');
                string surface = wordSegments[0].Trim().ToLower();
                string tagStr = wordSegments.Length > 1 ? wordSegments[1] : "";
                string readingStr = wordSegments.Length > 2 ? wordSegments[2].Trim() : "";

                // MEMO: ExpressionRuleのTokenElementは特殊なので位置指定が実際の出現位置と結びついていない。
                tokens.Add(new TokenElement(surface, tagStr.Split(',').Select(t => t.Trim()).ToList(), 0, 0, readingStr));
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
        public static string[] SplitToRuleElements(string line)
        {
            var segments = Regex.Split(line, @" *[+] *");
            return segments;
        }
    }
}
