using System;
using System.Collections.Generic;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public sealed class InvalidExpressionValidator : DictionaryValidator
    {
        private List<string> invalidWords;

        public InvalidExpressionValidator() : base("InvalidExpression")
        {
            // MEMO: DictionaryValidatorの仕組みをバイパスするための処理。
            invalidWords = new List<string>();

            // DefaultResourceの読み込み。
            string v = DefaultResources.ResourceManager.GetString($"InvalidExpression_ja");
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                invalidWords.Add(line.Trim());
                //log.Error("Skip to load line... Invalid line: " + line);
            }

            // TODO: 本来はデフォルトリソースだけでなくConfファイルからも読み込めるようにする。
        }

        public override void Validate(Sentence sentence)
        {
            //foreach (string value in StreamDictionary())
            foreach (string value in invalidWords)
            {
                // Invalidな表現を1つのセンテンス内から複数探索する。
                int offset = 0;
                while (true)
                {
                    // MEMO: String.IndexOf(string, int, StringComparison)は.NET 2.0以降で使用可能。
                    // MEMO: StringComparison.Ordinalは.NET 1.1以降で使用可能。
                    // MEMO: StringComparison.Ordinalは大文字と小文字を区別する。
                    int matchStartPosition = sentence.Content.IndexOf(value, offset, StringComparison.Ordinal);
                    if (matchStartPosition <= -1)
                    {
                        // not found
                        break;
                    }

                    // マッチしたInvalid Expressionの全文字位置を登録する。
                    int matchEndPosition = matchStartPosition + value.Length;
                    addLocalizedErrorWithPosition(sentence, matchStartPosition, matchEndPosition, value);

                    // next loop
                    offset = matchEndPosition;
                }
            }
        }
    }
}
