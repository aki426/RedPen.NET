﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using RedPen.Net.Core.Model;
using System.Collections.Generic;
using System.Linq;

namespace RedPen.Net.Core.Parser
{
    public class PreprocessorRule
    {
        public int LineNumber { get; init; } = 0;
        public int LineNumberLimit { get; set; } = 0;

        public List<string> Parameters { get; init; } = new List<string>();

        public enum RuleType
        {
            SUPPRESS
        }

        public RuleType Type { get; init; }

        public PreprocessorRule(RuleType type, int lineNumber)
        {
            LineNumber = lineNumber;
            Type = type;
        }

        /// <summary>
        /// パラメータを追加する関数。パラメータ名は常に小文字に変換される。
        /// </summary>
        /// <param name="parameter">パラメータ名</param>
        public void AddParameter(string parameter)
        {
            this.Parameters.Add(parameter.ToLower());
        }

        /// <summary>
        /// Return true if the rule is triggered by the given line and name from the structure of the supplied document
        /// 行番号とバリデータ名から、与えられたドキュメントの構造に対してルールがトリガーされる場合はTrueを返す。
        /// </summary>
        /// <param name="document"></param>
        /// <param name="errorLineNumber"></param>
        /// <param name="validatorName"></param>
        /// <returns>true if the error is triggered by the specified validator</returns>
        public bool IsTriggeredBy(Document document, int errorLineNumber, string validatorName)
        {
            if ((LineNumberLimit < LineNumber) || (errorLineNumber < LineNumberLimit))
            {
                if (Parameters.Count == 0 || Parameters.Contains(validatorName.ToLower().Replace("\\.js$", "")))
                {
                    // find out if the rule line number is in the same section as the other line number
                    foreach (Section section in document.Sections)
                    {
                        // セクション全体のSentenceを取得し、それらの中にerrorLineNumberが含まれるかを確認する。

                        List<Sentence> allBlockSentences = new List<Sentence>();
                        allBlockSentences.AddRange(section.HeaderSentences);

                        foreach (Paragraph paragraph in section.Paragraphs)
                        {
                            allBlockSentences.AddRange(paragraph.Sentences);
                        }

                        foreach (ListBlock listBlock in section.ListBlocks)
                        {
                            foreach (ListElement element in listBlock.ListElements)
                            {
                                allBlockSentences.AddRange(element.Sentences);
                            }
                        }

                        if (IsInsideSentences(allBlockSentences, LineNumber, errorLineNumber))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// sentencesの行の範囲の中にruleLineNumberとerrorLineNumberが含まれる場合True。
        /// </summary>
        /// <param name="sentences"></param>
        /// <param name="ruleLineNumber"></param>
        /// <param name="errorLineNumber"></param>
        /// <returns></returns>
        private bool IsInsideSentences(List<Sentence> sentences, int ruleLineNumber, int errorLineNumber)
        {
            if (ruleLineNumber > errorLineNumber) { return false; }

            var sorted = sentences.OrderBy(s => s.LineNumber);
            int startLine = sorted.First().LineNumber;
            int endLine = sorted.Last().LineNumber;

            // if we are in this paragraph, and the other line number is
            if ((startLine <= ruleLineNumber) && (ruleLineNumber <= endLine)
                && (startLine <= errorLineNumber) && (errorLineNumber <= endLine))
            {
                return true;
            }

            return false;
        }
    }
}
