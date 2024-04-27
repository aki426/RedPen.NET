using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    // MEMO: Configurationの定義は短いのでValidatorファイル内に併記する。

    /// <summary>JapaneseStyleのConfiguration</summary>
    public record JapaneseStyleConfiguration : ValidatorConfiguration, IJodoshiStyleConfigParameter
    {
        public JodoshiStyle JodoshiStyle { get; init; }

        public JapaneseStyleConfiguration(ValidationLevel level, JodoshiStyle jodoshiStyle) : base(level)
        {
            this.JodoshiStyle = jodoshiStyle;
        }
    }

    /// <summary>JapaneseStyleのValidator</summary>
    public class JapaneseStyleValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        public JapaneseStyleConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        public JapaneseStyleValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseStyleConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>だ・である調の助動詞のリスト</summary>
        private static List<string> DaDearuPattern = new List<string>()
        {
            "た", "だ", "だった", "だろう", "であった", "である", "ない", "ぬ"
        };

        /// <summary>です・ます調の助動詞のリスト</summary>
        private static List<string> DesuMasuPattern = new List<string>()
        {
            "でした", "でしょう", "です", "ないでしょう", "ないです", "ました", "ます", "ません"
        };

        public void PreValidate(Sentence sentence)
        {
            // nothing.
        }

        /// <summary>
        /// 助動詞のTokenのみを取得する。助動詞が連続する場合は、単体のものは捨てて連結したものを1Tokenとして返す。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of TokenElements.</returns>
        public static List<TokenElement> GetCompoundJodoshi(Sentence sentence)
        {
            // 助動詞が連続する場合は連結したものを1Tokenとして評価する。
            List<TokenElement> CompoundJodoshi = new List<TokenElement>();
            List<TokenElement> buffer = new List<TokenElement>();
            foreach (var token in sentence.Tokens)
            {
                if (token.Tags[0] == "助動詞")
                {
                    buffer.Add(token);
                }
                else
                {
                    if (buffer.Any())
                    {
                        // 連結した助動詞を1Tokenとして登録。
                        CompoundJodoshi.Add(new TokenElement(
                            string.Join("", buffer.Select(t => t.Surface)),
                            buffer.First().Tags, // Tagsは使わないので先頭のTokenのものを暫定的にセット。
                            string.Join("", buffer.Select(t => t.Reading)), // Readingも使わないが連結してセット。
                            buffer.SelectMany(t => t.OffsetMap).ToImmutableList()
                        ));

                        buffer.Clear();
                    }
                }
            }
            if (buffer.Any())
            {
                // 連結した助動詞を1Tokenとして登録。
                CompoundJodoshi.Add(new TokenElement(
                    string.Join("", buffer.Select(t => t.Surface)),
                    buffer.First().Tags, // Tagsは使わないので先頭のTokenのものを暫定的にセット。
                    string.Join("", buffer.Select(t => t.Reading)), // Readingも使わないが連結してセット。
                    buffer.SelectMany(t => t.OffsetMap).ToImmutableList()
                ));
            }

            return CompoundJodoshi;
        }

        /// <summary>
        /// Validation実行。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            var jodoshiList = GetCompoundJodoshi(sentence); // 助動詞のみ取得。
            List<TokenElement> errorJodoshi = new List<TokenElement>();

            switch (Config.JodoshiStyle)
            {
                case JodoshiStyle.DaDearu:
                    // だ・である調の場合、です・ます調の助動詞が含まれている場合はエラーとする。
                    errorJodoshi = jodoshiList.Where(jodoshi => DesuMasuPattern.Contains(jodoshi.Surface)).ToList();
                    break;

                case JodoshiStyle.DesuMasu:
                default:
                    // です・ます調の場合、だ・である調の助動詞が含まれている場合はエラーとする。
                    errorJodoshi = jodoshiList.Where(jodoshi => DaDearuPattern.Contains(jodoshi.Surface)).ToList();
                    break;
            }

            foreach (var token in errorJodoshi)
            {
                result.Add(new ValidationError(
                    ValidationType.JapaneseStyle,
                    this.Level,
                    sentence,
                    token.OffsetMap[0],
                    token.OffsetMap[^1],
                    MessageArgs: new object[] { token.Surface }
                ));
            }

            return result;
        }
    }
}
