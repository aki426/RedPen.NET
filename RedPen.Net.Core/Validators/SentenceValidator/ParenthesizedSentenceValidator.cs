using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>ParenthesizedSentenceのConfiguration</summary>
    public record ParenthesizedSentenceConfiguration : ValidatorConfiguration,
        IMaxLengthConfigParameter, IMaxNumberConfigParameter, IMaxLevelConfigParameter
    {
        /// <summary>括弧内に存在してもよい単語数の上限</summary>
        public int MaxLength { get; init; }

        /// <summary>一文内に存在してよい括弧の上限数</summary>
        public int MaxNumber { get; init; }

        /// <summary>一文に存在してよい括弧のネスト数</summary>
        public int MaxLevel { get; init; }

        public ParenthesizedSentenceConfiguration(ValidationLevel level, int maxLength, int maxNumber, int maxLevel) : base(level)
        {
            this.MaxLength = maxLength;
            this.MaxNumber = maxNumber;
            this.MaxLevel = maxLevel;
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>ParenthesizedSentenceのValidator</summary>
    public class ParenthesizedSentenceValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public ParenthesizedSentenceConfiguration Config { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="ParenthesizedSentenceValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public ParenthesizedSentenceValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            ParenthesizedSentenceConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        // カッコの文字種定義

        /// <summary>半角左カッコ</summary>
        private static readonly char L_PAREN = '(';

        /// <summary>半角右カッコ</summary>
        private static readonly char R_PAREN = ')';

        /// <summary>全角左カッコ</summary>
        private static readonly char L_PAREN_FULLWIDTH = '（';

        /// <summary>全角右カッコ</summary>
        private static readonly char R_PAREN_FULLWIDTH = '）';

        /// <summary>
        /// 括弧に関するValidationを実行します。次のケースに対応しています。
        /// １．左右の括弧の対応関係が正しいかどうか。（左カッコが無いのに右カッコがある、もしくは右カッコで閉じられていない左カッコがある。）
        /// ２．括弧内の文章の文字数が規定値を超えていないかどうか。（「これ（～～～長すぎる文～～～）はあれと異なります。」などという長い括弧は不正。）
        /// ３．一文内に存在してもよい括弧の数が規定値を超えていないかどうか。（「あ（）、い（）、う（）です」などという括弧の多用は不正。）
        /// ４．括弧のネストレベルが規定値を超えていないかどうか。（「（（（（））））」などという括弧の使い方は不正。）
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            List<(char paren, int offset)> parenLevel = new List<(char paren, int offset)>();
            int subsentenceCount = 0;

            for (int i = 0; i < sentence.Content.Length; i++)
            {
                char c = sentence.Content[i];

                if (c == L_PAREN || c == L_PAREN_FULLWIDTH)
                {
                    // 左カッコ開始
                    parenLevel.Add((c, i));

                    if (parenLevel.Count > Config.MaxLevel)
                    {
                        // 括弧のネストレベルが規定値を超えた。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(i),
                            sentence.ConvertToLineOffset(i),
                            MessageArgs: new object[] { parenLevel.Count, Config.MaxLevel },
                            MessageKey: "NestingLevelTooDeep"
                        ));
                        // Validationロジックは破綻しないので処理続行。
                    }
                }
                else if (c == R_PAREN || c == R_PAREN_FULLWIDTH)
                {
                    char lParen = c == R_PAREN ? L_PAREN : L_PAREN_FULLWIDTH;

                    if (parenLevel.Count == 0)
                    {
                        // 対応する左カッコが存在しないのに右カッコが出現した。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(i),
                            sentence.ConvertToLineOffset(i),
                            MessageArgs: new object[] { c.ToString() },
                            MessageKey: "MismatchedParentheses"
                        ));

                        // Validationロジックは破綻するのでReturnする。
                        return result;
                    }
                    else if (parenLevel.Last().paren != lParen)
                    {
                        // 対応する左カッコの文字種が右カッコと一致しない。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            this.Level,
                            sentence,
                            sentence.ConvertToLineOffset(i),
                            sentence.ConvertToLineOffset(i),
                            MessageArgs: new object[] { c.ToString() },
                            MessageKey: "MismatchedParentheses"
                        ));

                        // Validationロジックは破綻するのでReturnする。
                        return result;
                    }
                    else
                    {
                        // 対応する正しい左カッコが存在し、右カッコが出現して閉じるケース。
                        var subSentenceLength = i - parenLevel.Last().offset - 1;

                        if (subSentenceLength > Config.MaxLength)
                        {
                            // 括弧内センテンスの文字数が規定値を超えた。
                            result.Add(new ValidationError(
                                ValidationType.ParenthesizedSentence,
                                this.Level,
                                sentence,
                                sentence.ConvertToLineOffset(parenLevel.Last().offset),
                                sentence.ConvertToLineOffset(i),
                                MessageArgs: new object[] { subSentenceLength, Config.MaxLength },
                                MessageKey: "SubsentenceTooLong"));
                        }

                        // 右カッコで閉じられたので、現在の括弧ネストレベルを1つ下げる。
                        parenLevel.RemoveAt(parenLevel.Count - 1);

                        if (parenLevel.Count == 0)
                        {
                            subsentenceCount++;
                        }
                    }
                }
            }

            if (parenLevel.Count > 0)
            {
                // 対応する右カッコで閉じられていない右カッコが存在する。
                result.Add(new ValidationError(
                    ValidationType.ParenthesizedSentence,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(parenLevel.Last().offset),
                    sentence.ConvertToLineOffset(parenLevel.Last().offset),
                    MessageArgs: new object[] { parenLevel.Last().paren.ToString() },
                    MessageKey: "MismatchedParentheses"
                ));
            }

            if (subsentenceCount > Config.MaxNumber)
            {
                // 平の文章から1レベル括弧で囲まれた文章の数が規定値を超えた。
                result.Add(new ValidationError(
                    ValidationType.ParenthesizedSentence,
                    this.Level,
                    sentence,
                    sentence.ConvertToLineOffset(0),
                    sentence.ConvertToLineOffset(sentence.Content.Length - 1),
                    MessageArgs: new object[] { subsentenceCount, Config.MaxNumber },
                    MessageKey: "SubsentenceTooFrequent"
                ));
            }

            return result;
        }
    }
}
