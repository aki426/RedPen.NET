using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>JapaneseAmbiguousNounConjunctionのConfiguration</summary>
    public record JapaneseAmbiguousNounConjunctionConfiguration : ValidatorConfiguration, IWordSetConfigParameter
    {
        public HashSet<string> WordSet { get; init; }

        public JapaneseAmbiguousNounConjunctionConfiguration(ValidationLevel level, HashSet<string> wordSet) : base(level)
        {
            this.WordSet = wordSet;
        }
    }

    /// <summary>JapaneseAmbiguousNounConjunctionのValidator</summary>
    public class JapaneseAmbiguousNounConjunctionValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public JapaneseAmbiguousNounConjunctionConfiguration Config { get; init; }

        /// <summary></summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="JapaneseAmbiguousNounConjunctionValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public JapaneseAmbiguousNounConjunctionValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            JapaneseAmbiguousNounConjunctionConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;
        }

        /// <summary>あいまいな名詞接続のパターン「格助詞の "の" + 名詞連続 + 各助詞の "の"」のExpressionRule。</summary>
        private static ExpressionRule nounConjunction =
            ExpressionRuleExtractor.Run("*:名詞 + の:助詞 + *:名詞 + の:助詞 + *:名詞");

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // 名詞の連続を連結済みのTokenElementリストに変換して、ExpressionRuleにマッチさせる。
            // これにより名詞連続をバラバラのTokenではなく1つの名詞Tokenとして扱うことでRuleにマッチ可能にする。
            (bool isMatch, List<ImmutableList<TokenElement>> tokens) value =
                nounConjunction.MatchSurfacesAndTags(
                    TokenElement.ConvertToConcatedNouns(sentence.Tokens));

            if (value.isMatch)
            {
                foreach (ImmutableList<TokenElement> matchedTokens in value.tokens)
                {
                    var surface = string.Join("", matchedTokens.Select(t => t.Surface));
                    // Ignoreリストに入っていない表現であればエラーとして出力する。
                    if (!Config.WordSet.Contains(surface))
                    {
                        result.Add(new ValidationError(
                            ValidationType.JapaneseAmbiguousNounConjunction,
                            this.Level,
                            sentence,
                            matchedTokens.First().OffsetMap[0],
                            matchedTokens.Last().OffsetMap[^1],
                            MessageArgs: new object[] { surface }));
                    }
                }
            }

            return result;
        }
    }
}
