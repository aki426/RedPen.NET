using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Parser
{
    /// <summary>
    /// This class wraps a Stream as StreamReader.
    /// It looks for preprocessor instructions in the input text and converts this to a list of PreprocessorRules.
    /// </summary>
    public class PreprocessingReader : IDisposable
    {
        // MEMO: JAVA版ではBufferedStreamを使っているが、C#版では必要なさそうなのでStreamReaderで代用する。
        // PreprocessingReaderがStreamからのBuffering機能を持つ必要はなく、どのようなStreamを与えるかで利用側がコントロールすればよい。

        private StreamReader streamReader;
        private int lineNumber = 0;

        private IDocumentParser? parser = null;

        public HashSet<PreprocessorRule> PreprocessorRules { get; init; } = new HashSet<PreprocessorRule>();
        private PreprocessorRule? lastRule = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessingReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="parser">The parser.</param>
        public PreprocessingReader(Stream stream, IDocumentParser parser)
        {
            this.streamReader = new StreamReader(stream);
            this.parser = parser;
        }

        // MEMO: JAVAのCloseはC#のDisposeに相当する

        /// <summary>Dispose this.</summary>
        public void Dispose()
        {
            streamReader.Dispose();
        }

        /// <summary>
        /// A wrapper method of StreamReader.ReadLine().
        /// </summary>
        /// <returns>a line string.</returns>
        public string? ReadLine()
        {
            // PreprocessingReaderは各種フォーマットの特殊なルールを読み取る機能をReaderに持たせるためのものなので、
            // 仮にこの処理を個別のフォーマット用Parserに実装したらこの関数は不要になる？

            // MEMO: クラス内に状態としてStreamReaderを持つ設計なので、Streamは開きっぱなしである。
            // TODO: using句を使う実装を検討する。

            // TODO: 文字コードをUTF8に固定した方が良いか検討する。……利用側でコントロールすればよいか？

            var line = this.streamReader.ReadLine();
            lineNumber++;

            // line == nullではなくsr.Peek() == -1で判定すべき？
            if (line == null) { return line; }

            // TODO: Parserを実装したらコメントアウトを外す。
            // TODO: switch式を使う実装を検討する。
            //string ruleText = line;
            //if (parser is AsciiDocParser)
            //{
            //    if (ruleText.ToLower().StartsWith("[suppress"))
            //    {
            //        addAsciiDocAttributeSuppressRule(ruleText);
            //        return "";
            //    }
            //}
            //else if (parser is MarkdownParser)
            //{
            //    if (Regex.IsMatch(ruleText, "^ *<!-- *@suppress (.*)-->"))
            //    {
            //        ruleText = line.Replace("^ *<!--", "").Replace("-->", "").Trim();
            //        addCommentSuppressRule(ruleText);
            //    }
            //}
            //else if (parser is ReVIEWParser)
            //{
            //    if (Regex.IsMatch(ruleText, "^#@# *@suppress(.*)"))
            //    {
            //        ruleText = line.Replace("^#@#", "").Trim();
            //        addCommentSuppressRule(ruleText);
            //    }
            //}
            //else if (parser is LaTeXParser)
            //{
            //    if (Regex.IsMatch(ruleText, "^% *@suppress(.*)"))
            //    {
            //        ruleText = line.Replace("^%", "").Trim();
            //        addCommentSuppressRule(ruleText);
            //    }
            //}
            //else if (parser is ReSTParser)
            //{
            //    if (Regex.IsMatch(ruleText, "[.][.] *@suppress(.*)"))
            //    {
            //        ruleText = line.Replace("[.][.]", "").Trim();
            //        addCommentSuppressRule(ruleText, 3);
            //    }
            //}

            return line;
        }

        /// <summary>
        /// Adds the ascii doc attribute suppress rule.
        /// </summary>
        /// <param name="ruleString">The rule string.</param>
        private void AddAsciiDocAttributeSuppressRule(string ruleString)
        {
            PreprocessorRule rule = new PreprocessorRule(PreprocessorRule.RuleType.SUPPRESS, lineNumber);
            if (lastRule != null) { lastRule.LineNumberLimit = lineNumber; }
            lastRule = rule;

            string[] parts = ruleString.Split('=');
            if (parts.Length > 1)
            {
                string[] parameters = parts[1].Replace("[\"'\\]]", "").Split(' ');
                foreach (string parameter in parameters)
                {
                    if (parameter != string.Empty)
                    {
                        rule.AddParameter(parameter);
                    }
                }
            }
            PreprocessorRules.Add(rule);
        }

        /// <summary>
        /// Adds the comment suppress rule.
        /// </summary>
        /// <param name="ruleString">The rule string.</param>
        private void AddCommentSuppressRule(string ruleString)
        {
            AddCommentSuppressRule(ruleString, 0);
        }

        /// <summary>
        /// Adds the comment suppress rule.
        /// </summary>
        /// <param name="ruleString">The rule string.</param>
        /// <param name="gap">The gap.</param>
        private void AddCommentSuppressRule(string ruleString, int gap)
        {
            PreprocessorRule rule = new PreprocessorRule(PreprocessorRule.RuleType.SUPPRESS, lineNumber + gap);
            if (lastRule != null) { lastRule.LineNumberLimit = lineNumber; }
            lastRule = rule;

            string[] parts = ruleString.Split(' ');
            if (parts.Length > 1)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i] != string.Empty)
                    {
                        rule.AddParameter(parts[i]);
                    }
                }
            }

            PreprocessorRules.Add(rule);
        }
    }
}
