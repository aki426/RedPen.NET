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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>SuggestExpressionのConfiguration</summary>
    public record SuggestExpressionConfiguration : ValidatorConfiguration, IExpressionMapConfigParameter
    {
        public Dictionary<string, string> ExpressionMap { get; init; }

        public SuggestExpressionConfiguration(ValidationLevel level, Dictionary<string, string> expressionMap) : base(level)
        {
            ExpressionMap = expressionMap;
        }
    }

    /// <summary>SuggestExpressionのValidator</summary>
    public class SuggestExpressionValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidationConfiguration</summary>
        public SuggestExpressionConfiguration Config { get; init; }

        public CultureInfo DocumentLang { get; init; }

        public SuggestExpressionValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SuggestExpressionConfiguration config) : base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
            this.DocumentLang = documentLangForTest;
        }

        /// <summary>
        /// Validate実行。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            foreach (KeyValuePair<string, string> kvp in Config.ExpressionMap)
            {
                string content = sentence.Content;
                // 日本語の場合、正表現の中に誤表現が含まれる場合があるので、先に正表現をマスキングした文字列で判定を行う。
                if (this.DocumentLang.Name == "ja-JP")
                {
                    content = GetNullMasked(content, kvp.Value);
                }

                int end = -1; // MEMO: 初期状態はIndexOfに渡す走査開始Indexを0にしたいので-1にしておく。

                // まだ未走査の文字列がある＝endが文字列の最後に達していない場合はキーワードの検出を続ける。
                while (end < sentence.Content.Length - 1)
                {
                    int start = content.IndexOf(kvp.Key, end + 1); // 誤表現の検出はマスク済みの文字列で行う。
                    if (start < 0) break; // not found.
                    end = start + kvp.Key.Length - 1; // LineOffsetイコール文字位置とみなす。startが開始文字、endが終了文字。

                    // 言語の判定は入力ドキュメントの言語設定で行う。
                    if (this.DocumentLang.Name == "ja-JP")
                    {
                        // go to resutl.Add( error )
                    }
                    else
                    {
                        // 単語が単語の境界にあるかどうかを判定する。
                        // MEMO: これはスペース区切りの英語などの言語で有効で、日本語の場合は単語の区切りが助詞や助動詞であるため、
                        // char.IsLetterがTrueになってしまい、期待した処理にならない。
                        bool hasWordBoundaries =
                            (start == 0 || !char.IsLetter(sentence.Content[start - 1]))
                            && (end == sentence.Content.Length - 1 || !char.IsLetter(sentence.Content[end + 1]));
                        if (hasWordBoundaries)
                        {
                            // go to resutl.Add( error )
                        }
                        else
                        {
                            // trap and next loop.
                            continue;
                        }
                    }

                    result.Add(new ValidationError(
                        ValidationName,
                        this.Level,
                        sentence,
                        sentence.ConvertToLineOffset(start),
                        sentence.ConvertToLineOffset(end),
                        MessageArgs: new object[] { kvp.Key, kvp.Value }));
                }
            }

            return result;
        }

        /// <summary>
        /// 文字列strに対して、マスキングすべきすべての文字列をNUL文字で置き換えた文字列を取得する。
        /// </summary>
        /// <param name="str">The str.</param>
        /// <param name="masking">The masking.</param>
        /// <returns>A string.</returns>
        public static string GetNullMasked(string str, string masking)
        {
            string mask = new string('\0', masking.Length);
            return Regex.Replace(str, Regex.Escape(masking), mask);
        }
    }
}
