using System;
using System.Collections.Generic;
using System.Globalization;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
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

        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            foreach (KeyValuePair<string, string> kvp in Config.WordMap)
            {
                int end = -1; // MEMO: 初期状態はIndexOfに渡す走査開始Indexを0にしたいので-1にしておく。

                // まだ未走査の文字列がある＝endが文字列の最後に達していない場合はキーワードの検出を続ける。
                while (end < sentence.Content.Length - 1)
                {
                    int start = sentence.Content.IndexOf(kvp.Key, end + 1);
                    if (start < 0) break; // not found.
                    end = start + kvp.Key.Length - 1; // LineOffsetイコール文字位置とみなす。startが開始文字、endが終了文字。

                    // 日本語の判定は入力ドキュメントの言語設定で行う。
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
                        ValidationType.SuggestExpression,
                        this.Level,
                        sentence,
                        sentence.ConvertToLineOffset(start),
                        sentence.ConvertToLineOffset(end),
                        MessageArgs: new object[] { kvp.Key, kvp.Value }));
                }
            }

            return result;
        }
    }
}
