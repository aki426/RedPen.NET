using System;
using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    public sealed class InvalidExpressionValidator : Validator, ISentenceValidatable // DictionaryValidator
    {
        public InvalidExpressionConfiguration Config { get; init; }

        public InvalidExpressionValidator(
            ValidationLevel level,
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            InvalidExpressionConfiguration config) :
            base(
                level,
                documentLangForTest,
                //errorMessages,
                symbolTable)
        {
            this.Config = config;
        }

        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            //foreach (string value in StreamDictionary())
            foreach (string invalidWord in Config.WordList)
            {
                // Invalidな表現を1つのセンテンス内から複数探索する。
                int offset = 0;
                while (true)
                {
                    // MEMO: String.IndexOf(string, int, StringComparison)は.NET 2.0以降で使用可能。
                    // MEMO: StringComparison.Ordinalは.NET 1.1以降で使用可能。
                    // MEMO: StringComparison.Ordinalは大文字と小文字を区別する。
                    int matchStartPosition = sentence.Content.IndexOf(invalidWord, offset, StringComparison.Ordinal);
                    if (matchStartPosition <= -1)
                    {
                        // not found
                        break;
                    }

                    // マッチしたInvalid Expressionの全文字位置を登録する。
                    int matchEndPosition = matchStartPosition + invalidWord.Length - 1;
                    result.Add(
                        new ValidationError(
                            ValidationType.InvalidExpression,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(matchStartPosition),
                            //matchStartPosition,
                            sentence.ConvertToLineOffset(matchEndPosition),
                            //matchEndPosition,
                            MessageArgs: new object[] { invalidWord }));

                    // next loop. マッチしたinvalidWordの次の文字から再検索するため+1。
                    offset = matchEndPosition + 1;
                }
            }

            return result;
        }
    }
}
