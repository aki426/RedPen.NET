using System;
using System.Collections.Generic;
using System.IO;

namespace RedPen.Net.Core.Parser
{
    /// <summary>
    /// The preprocessing reader.
    /// </summary>
    public class PreprocessingReader : IDisposable
    {
        // PreprocessingReaderがStreamを持つべきかStreamReaderを持つべきか、またBufferedStreamへの変換が必要か検討する。
        // https://stackoverflow.com/questions/492283/when-to-use-net-bufferedstream-class
        private BufferedStream bufferedStream;

        public HashSet<PreprocessorRule> PreprocessorRules { get; init; } = new HashSet<PreprocessorRule>();

        private int lineNumber = 0;
        private PreprocessorRule? lastRule = null;
        private IDocumentParser? parser = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreprocessingReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="parser">The parser.</param>
        public PreprocessingReader(Stream stream, IDocumentParser parser)
        {
            this.bufferedStream = new BufferedStream(stream);
            this.parser = parser;
        }

        // MEMO: JAVAのCloseはC#のDisposeに相当する
        /// <summary>
        /// Disposes the.
        /// </summary>
        public void Dispose()
        {
            // MEMO: BufferedStream.DisposeはCloseを再呼び出ししている
            bufferedStream.Dispose();
        }

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns>A string? .</returns>
        public string? ReadLine()
        {
            // TODO: using句を使う実装を検討する。
            // TODO: 文字コードをUTF8に固定する実装を検討する。
            StreamReader sr = new StreamReader(this.bufferedStream);
            var line = sr.ReadLine();
            lineNumber++;

            // line == nullではなくsr.Peek() == -1で判定すべき？
            if (line == null) { return line; }

            string ruleText = line;

            // TODO: Parserを実装したらコメントアウトを外す。
            // TODO: switch式を使う実装を検討する。
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
