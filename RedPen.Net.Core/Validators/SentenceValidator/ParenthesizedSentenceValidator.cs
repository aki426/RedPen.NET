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
        IMaxLengthConfigParameter, IMaxCountConfigParameter, IMaxLevelConfigParameter
    {
        /// <summary>括弧内に存在してもよい単語数の上限</summary>
        public int MaxLength { get; init; }

        /// <summary>一文内に存在してよい括弧の上限数</summary>
        public int MaxCount { get; init; }

        /// <summary>一文に存在してよい括弧のネスト数</summary>
        public int MaxLevel { get; init; }

        public ParenthesizedSentenceConfiguration(ValidationLevel level, int maxLength, int maxCount, int maxLevel) : base(level)
        {
            this.MaxLength = maxLength;
            this.MaxCount = maxCount;
            this.MaxLevel = maxLevel;
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>ParenthesizedSentenceのValidator</summary>
    public class ParenthesizedSentenceValidator : Validator, IDocumentValidatable
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
        /// ドキュメント構造上ひとまとまりと判定されるSentenceのリスト全体に対して、Validationを実行する。
        /// </summary>
        /// <param name="sentences">The sentences.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public static List<ValidationError> ValidateSentencesByStructure(
            List<Sentence> sentences,
            ParenthesizedSentenceConfiguration config,
            SymbolTable symbolTable
            )
        {
            List<ValidationError> result = new List<ValidationError>();

            // MEMO: 空のリストが渡された場合は何もしない。
            if (sentences.Count == 0)
            {
                return result;
            }

            // 文の区切りとみなす文字を定義する。
            char[] periods = new char[]{
                // MEMO: 日本語では「。」「？」「！」の3種類が該当。
                symbolTable.GetValueOrFallbackToDefault(SymbolType.FULL_STOP),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.QUESTION_MARK),
                symbolTable.GetValueOrFallbackToDefault(SymbolType.EXCLAMATION_MARK)
            };

            // 文章構造単位で必要な情報を付加しつつ1文字単位で分解する。
            List<(char c, int indexInSentence, int indexInStructure, Sentence sentence, LineOffset lineOffset)> charStack =
                    new List<(char c, int indexInSentence, int indexInStructure, Sentence sentence, LineOffset lineOffset)>();
            int sentenceStartPosCounter = 0;
            foreach (Sentence sentence in sentences)
            {
                if (sentence.Content.Length == 0)
                {
                    // 空のセンテンスはスキップする。
                    continue;
                }

                charStack.AddRange(
                    sentence.Content.ToCharArray().Select((c, index) =>
                        (c, index, sentenceStartPosCounter + index, sentence, sentence.ConvertToLineOffset(index)))
                );
                sentenceStartPosCounter += sentence.Content.Length;
            }

            // validation
            List<(char c, int indexInSentence, int indexInStructure, Sentence sentence, LineOffset lineOffset)> parenLevel =
                new List<(char c, int indexInSentence, int indexInStructure, Sentence sentence, LineOffset lineOffset)>();

            int subsentenceCount = 0;

            foreach (var currentChar in charStack)
            {
                if (currentChar.c == L_PAREN || currentChar.c == L_PAREN_FULLWIDTH)
                {
                    // 左カッコ開始
                    parenLevel.Add(currentChar);

                    if (parenLevel.Count > config.MaxLevel)
                    {
                        // 括弧のネストレベルが規定値を超えた。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            config.Level,
                            currentChar.sentence,
                            currentChar.lineOffset,
                            currentChar.lineOffset,
                            MessageArgs: new object[] { parenLevel.Count, config.MaxLevel },
                            MessageKey: "NestingLevelTooDeep"
                        ));
                        // Validationロジックは破綻しないので処理続行。
                    }
                }
                else if (currentChar.c == R_PAREN || currentChar.c == R_PAREN_FULLWIDTH)
                {
                    char lParen = currentChar.c == R_PAREN ? L_PAREN : L_PAREN_FULLWIDTH;

                    if (parenLevel.Count == 0)
                    {
                        // 対応する左カッコが存在しないのに右カッコが出現した。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            config.Level,
                            currentChar.sentence,
                            currentChar.lineOffset,
                            currentChar.lineOffset,
                            MessageArgs: new object[] { currentChar.c.ToString() },
                            MessageKey: "MismatchedParentheses"
                        ));

                        // Validationロジックは破綻するのでReturnする。
                        return result;
                    }
                    else if (parenLevel.Last().c != lParen)
                    {
                        // 対応する左カッコの文字種が右カッコと一致しない。
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            config.Level,
                            currentChar.sentence,
                            currentChar.lineOffset,
                            currentChar.lineOffset,
                            MessageArgs: new object[] { currentChar.c.ToString() },
                            MessageKey: "MismatchedParentheses"
                        ));

                        // Validationロジックは破綻するのでReturnする。
                        return result;
                    }
                    else
                    {
                        // 対応する正しい左カッコが存在し、右カッコが出現して閉じるケース。
                        // NOTE: センテンスをまたぐ場合があるので、構造上の文字数カウントで判定する。
                        var subSentenceLength = currentChar.indexInStructure - parenLevel.Last().indexInStructure - 1;

                        if (subSentenceLength > config.MaxLength)
                        {
                            // 括弧内センテンスの文字数が規定値を超えた。
                            result.Add(new ValidationError(
                                ValidationType.ParenthesizedSentence,
                                config.Level,
                                currentChar.sentence,
                                parenLevel.Last().lineOffset,
                                currentChar.lineOffset,
                                MessageArgs: new object[] { subSentenceLength, config.MaxLength },
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
                else if (periods.Contains(currentChar.c) && parenLevel.Count == 0)
                {
                    // 文の区切りに到達したとみなせる場合、かつ括弧が閉じ切っていた場合（ゼロレベルに戻った状態）のとき、
                    // その文の中に存在する括弧の数が規定値を超えていた場合エラーとする。

                    if (subsentenceCount > config.MaxCount)
                    {
                        result.Add(new ValidationError(
                            ValidationType.ParenthesizedSentence,
                            config.Level,
                            currentChar.sentence,
                            currentChar.sentence.OffsetMap[0],
                            currentChar.sentence.OffsetMap[^1],
                            MessageArgs: new object[] { subsentenceCount, config.MaxCount },
                            MessageKey: "SubsentenceTooFrequent"));
                    }

                    // カッコの数カウンターは1つの文を抜けたのでゼロに戻しておく。
                    subsentenceCount = 0;
                }
            }

            if (parenLevel.Count > 0)
            {
                // 対応する右カッコで閉じられていない左カッコが存在する。
                foreach (var lparen in parenLevel)
                {
                    result.Add(new ValidationError(
                        ValidationType.ParenthesizedSentence,
                        config.Level,
                        lparen.sentence,
                        lparen.lineOffset,
                        lparen.lineOffset,
                        MessageArgs: new object[] { lparen.c.ToString() },
                        MessageKey: "MismatchedParentheses"
                    ));
                }
            }
            else
            {
                // ドキュメント構造の最後でピリオド類文字が現れなかった場合に括弧の使いすぎエラーを出す。
                if (subsentenceCount != 0 && subsentenceCount > config.MaxCount)
                {
                    result.Add(new ValidationError(
                        ValidationType.ParenthesizedSentence,
                        config.Level,
                        sentences.Last(),
                        sentences.Last().OffsetMap[0],
                        sentences.Last().OffsetMap[^1],
                        MessageArgs: new object[] { subsentenceCount, config.MaxCount },
                        MessageKey: "SubsentenceTooFrequent"));
                }
            }

            return result;
        }

        /// <summary>
        /// 括弧に関するValidationを実行します。次のケースに対応しています。
        /// １．左右の括弧の対応関係が正しいかどうか。（左カッコが無いのに右カッコがある、もしくは右カッコで閉じられていない左カッコがある。）
        /// ２．括弧内の文章の文字数が規定値を超えていないかどうか。（「これ（～～～長すぎる文～～～）はあれと異なります。」などという長い括弧は不正。）
        /// ３．一文内に存在してもよい括弧の数が規定値を超えていないかどうか。（「あ（）、い（）、う（）です」などという括弧の多用は不正。）
        /// ４．括弧のネストレベルが規定値を超えていないかどうか。（「（（（（））））」などという括弧の使い方は不正。）
        /// NOTE: Sentence単位でのValidationは文の区切り方によって括弧の対応関係が破綻する可能性があるため、Documentを受け取り、Paragraph単位でValidationを実行します。
        /// </summary>
        /// <param name="document">The Document.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Document document)
        {
            List<ValidationError> result = new List<ValidationError>();

            foreach (List<Sentence> sentences in document.GetAllSentencesByDocumentStructure())
            {
                result.AddRange(ValidateSentencesByStructure(sentences, Config, SymbolTable));
            }

            return result;
        }
    }
}
