using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using J2N;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>SpaceBetweenAlphabeticalWordのConfiguration</summary>
    public record SpaceBetweenAlphabeticalWordConfiguration : ValidatorConfiguration, INoSpaceConfigParameter
    {
        public bool NoSpace { get; init; }

        public SpaceBetweenAlphabeticalWordConfiguration(ValidationLevel level, bool noSpace) : base(level)
        {
            this.NoSpace = noSpace;
        }
    }

    /// <summary>SpaceBetweenAlphabeticalWordのValidator</summary>
    public class SpaceBetweenAlphabeticalWordValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public SpaceBetweenAlphabeticalWordConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" }; // JAVA版では"zh-CHS"も対象言語。

        // MEMO: 現在のValidation設定の左カッコ、右カッコ、カンマの文字。

        private char leftParenthesis = '(';

        private char rightParenthesis = ')';

        private char comma = ',';

        public SpaceBetweenAlphabeticalWordValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            SpaceBetweenAlphabeticalWordConfiguration config) :
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

            pat = new Regex($"{shard}\\s+({word})\\s+{shard}");
        }

        private readonly string shard = @"[^A-Za-z0-9 !@#$%^&*()_+=\[\]\\{}|=<>,.{};':"",./<>?（）［］｛｝-]";
        private readonly string word = @"[A-Za-z0-9 !@#$%^&*()_+=\[\]\\{}|=<>,.{};':"",./<>?（）｛｝［］-]+";

        private readonly Regex pat; // = new Regex($"{shard}\\s+({word})\\s+{shard}");

        //public SpaceBetweenAlphabeticalWordValidator() : base("forbidden", false, // Spaces are enforced (false) or forbidden (true)
        //                                                     "skip_before", "",
        //                                                     "skip_after", "")
        //{
        //}

        /// <summary>
        /// 2文字を確認し、前の文字が標準ラテン文字ではなく、かつSkipBeforeに指定された文字ではなく、かつ左カッコ、右カッコ、カンマではなく、
        /// かつ後の文字が標準ラテン文字であり、ユニコードレターである場合にtrueを返す関数。
        /// </summary>
        /// <param name="prevCharacter">The prev character.</param>
        /// <param name="character">The character.</param>
        /// <returns>A bool.</returns>
        private bool notHasWhiteSpaceBeforeLeftParenthesis(char prevCharacter, char character)
        {
            return !StringUtils.IsBasicLatin(prevCharacter)
                   //&& getString("skip_before").IndexOf(prevCharacter) == -1
                   && prevCharacter != leftParenthesis
                   && prevCharacter != rightParenthesis
                   && prevCharacter != comma
                   && StringUtils.IsBasicLatin(character)
                   && char.IsLetter(character);
        }

        /// <summary>
        /// 2文字を確認し、前の文字が標準ラテン文字であり、ユニコードレターであり、
        /// 後の文字が標準ラテン文字ではなく、かつSkipAfterに指定された文字ではなく、かつ左カッコ、右カッコ、カンマではない場合にtrueを返す関数。
        /// </summary>
        /// <param name="prevCharacter">The prev character.</param>
        /// <param name="character">The character.</param>
        /// <returns>A bool.</returns>
        private bool notHasWhiteSpaceAfterRightParenthesis(char prevCharacter, char character)
        {
            return !StringUtils.IsBasicLatin(character)
                   //&& getString("skip_after").IndexOf(character) == -1
                   && character != rightParenthesis
                   && character != leftParenthesis
                   && character != comma
                   && StringUtils.IsBasicLatin(prevCharacter)
                   && char.IsLetter(prevCharacter);
        }

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            if (Config.NoSpace)
            {
                // アルファベット単語の前後にスペースを許容しない場合。
                var m = pat.Matches(sentence.Content);
                foreach (Match match in m)
                {
                    string word = match.Groups[1].Value;
                    if (!word.Contains(" "))
                    {
                        result.Add(new ValidationError(
                            ValidationType.SpaceBetweenAlphabeticalWord,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(match.Groups[1].Index),
                            sentence.ConvertToLineOffset(match.Groups[1].Index + match.Groups[1].Length - 1),
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
                    if (notHasWhiteSpaceBeforeLeftParenthesis(prevCharacter, character))
                    {
                        result.Add(new ValidationError(
                            ValidationType.SpaceBetweenAlphabeticalWord,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(idx),
                            sentence.ConvertToLineOffset(idx),
                            MessageArgs: new object[] { character.ToString() },
                            MessageKey: "Before"
                            ));
                    }
                    else if (notHasWhiteSpaceAfterRightParenthesis(prevCharacter, character))
                    {
                        result.Add(new ValidationError(
                            ValidationType.SpaceBetweenAlphabeticalWord,
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
