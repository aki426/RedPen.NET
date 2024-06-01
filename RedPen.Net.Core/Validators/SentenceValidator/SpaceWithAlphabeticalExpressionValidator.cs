using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SpaceBetweenAlphabeticalWordのConfiguration</summary>
    public record SpaceWithAlphabeticalExpressionConfiguration : ValidatorConfiguration, IForbiddenConfigParameter, ISkipAfterConfigParameter, ISkipBeforeConfigParameter
    {
        public bool Forbidden { get; init; }

        public string SkipAfter { get; init; }

        public string SkipBefore { get; init; }

        public SpaceWithAlphabeticalExpressionConfiguration(
            ValidationLevel level,
            bool Forbidden = false,
            string SkipAfter = "",
            string SkipBefore = "") : base(level)
        {
            this.Forbidden = Forbidden;
            this.SkipAfter = SkipAfter;
            this.SkipBefore = SkipBefore;
        }
    }

    /// <summary>SpaceBetweenAlphabeticalWordのValidator</summary>
    public class SpaceWithAlphabeticalExpressionValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SpaceWithAlphabeticalExpressionConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" }; // JAVA版では"zh-CHS"も対象言語。

        // MEMO: 現在のValidation設定の左カッコ、右カッコ、カンマの文字。

        private char leftParenthesis = '(';

        private char rightParenthesis = ')';

        private char comma = ',';

        public SpaceWithAlphabeticalExpressionValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SpaceWithAlphabeticalExpressionConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;

            // 左カッコ、右カッコ、カンマの文字をSymbolTableからする取得。
            leftParenthesis = symbolTable.SymbolTypeDictionary[SymbolType.LEFT_PARENTHESIS].Value;
            rightParenthesis = symbolTable.SymbolTypeDictionary[SymbolType.RIGHT_PARENTHESIS].Value;
            comma = symbolTable.SymbolTypeDictionary[SymbolType.COMMA].Value;
        }

        /// <summary>
        /// 2文字を確認し、
        /// 前の文字が標準ラテン文字ではなく、かつSkipAfterに指定された文字ではなく、かつ左カッコ、右カッコ、カンマではなく、
        /// かつ後の文字が標準ラテン文字であり、ユニコードレターである場合にtrueを返す関数。
        /// </summary>
        /// <param name="first">The prev character.</param>
        /// <param name="second">The next character.</param>
        /// <returns>A bool.</returns>
        private bool NeedSpaceAsFirst(char first, char second)
        {
            return !UnicodeUtility.IsBasicLatin(first)
                   && Config.SkipAfter.IndexOf(first) == -1 // 特定文字の直後であればスペースをスキップして良い。
                   && first != leftParenthesis
                   && first != rightParenthesis
                   && first != comma
                   && UnicodeUtility.IsBasicLatin(second)
                   && char.IsLetter(second);
        }

        /// <summary>
        /// 2文字を確認し、
        /// 前の文字が標準ラテン文字であり、ユニコードレターであり、
        /// 後の文字が標準ラテン文字ではなく、かつSkipBeforeに指定された文字ではなく、かつ左カッコ、右カッコ、カンマではない場合にtrueを返す関数。
        /// </summary>
        /// <param name="first">The prev character.</param>
        /// <param name="second">The next character.</param>
        /// <returns>A bool.</returns>
        private bool NeedSpaceAsSecond(char first, char second)
        {
            return !UnicodeUtility.IsBasicLatin(second)
                   && Config.SkipBefore.IndexOf(second) == -1 // 特定文字の直前であればスペースをスキップして良い。
                   && second != rightParenthesis
                   && second != leftParenthesis
                   && second != comma
                   && UnicodeUtility.IsBasicLatin(first)
                   && char.IsLetter(first);
        }

        // 半角英語表現をキャプチャするための正規表現。
        // 半角スペース以外で開始、終了し、内部で半角スペースを含んでもよいものとする。
        // つまり「きょうは Pepsi cola を飲みたい。」のような表現で「Pepsi cola」全体をキャプチャしたい。

        /// <summary>半角スペース以外の英字数字記号以外で始まり、かつ内部に半角スペース意外を含み、
        /// 末尾が半角スペース以外の英二数字記号以外で終わる、全体が半角の表現</summary>
        private static readonly Regex spaceWithinAlphabeticalExpression =
            new Regex(@"[!-~]+( +[!-~]+)*");

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            if (Config.Forbidden)
            {
                // アルファベット単語の前後にスペースを許容しない場合。
                var m = spaceWithinAlphabeticalExpression.Matches(sentence.Content);
                foreach (Match match in m)
                {
                    string word = match.Value;
                    int start = match.Index;
                    int end = match.Index + match.Length - 1;

                    if ((start == 0 || sentence.Content[start - 1] != ' ')
                        && (end == sentence.Content.Length - 1 || sentence.Content[end + 1] != ' '))
                    {
                        // 英語表現の前後にスペースが無い、または無くてもよいケースはエラーとしない。
                        // nothing.
                    }
                    else
                    {
                        result.Add(new ValidationError(
                            ValidationName,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(match.Index),
                            sentence.ConvertToLineOffset(match.Index + match.Length - 1),
                            MessageArgs: new object[] { word },
                            MessageKey: "Forbidden"
                        ));
                    }
                }
            }
            else
            {
                // アルファベット単語の前後にスペースを入れる場合。

                char prevCharacter = ' ';
                int idx = 0;
                foreach (char character in sentence.Content.ToCharArray())
                {
                    if (NeedSpaceAsFirst(prevCharacter, character))
                    {
                        // 1文字目がスペースであるべき場合、エラーとして報告する。
                        result.Add(new ValidationError(
                            ValidationName,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(idx),
                            sentence.ConvertToLineOffset(idx),
                            MessageArgs: new object[] { character.ToString() },
                            MessageKey: "Before"
                            ));
                    }
                    else if (NeedSpaceAsSecond(prevCharacter, character))
                    {
                        // 2文字目がスペースであるべき場合、エラーとして報告する。
                        result.Add(new ValidationError(
                            ValidationName,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(idx),
                            sentence.ConvertToLineOffset(idx),
                            MessageArgs: new object[] { character.ToString() },
                            MessageKey: "After"
                            ));
                    }

                    prevCharacter = character;
                    idx++;
                }
            }

            return result;
        }
    }
}
