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
    /// <summary>SuggestGrammarRuleのConfiguration</summary>
    public record SuggestGrammarRuleConfiguration : ValidatorConfiguration, IGrammarRuleMapConfigParameter
    {
        public Dictionary<string, string> GrammarRuleMap { get; init; }

        public SuggestGrammarRuleConfiguration(ValidationLevel level, Dictionary<string, string> grammarRuleMap) : base(level)
        {
            this.GrammarRuleMap = grammarRuleMap;
        }
    }

    // TODO: Validation対象に応じて、IDocumentValidatable, ISectionValidatable, ISentenceValidatableを実装する。
    /// <summary>SuggestGrammarRuleのValidator</summary>
    public class SuggestGrammarRuleValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        // TODO: 専用のValidatorConfigurationを別途定義する。
        /// <summary>ValidatorConfiguration</summary>
        public SuggestGrammarRuleConfiguration Config { get; init; }

        public ImmutableList<(GrammarRule rule, string suggest)> GrammarRules { get; init; }

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestGrammarRuleValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public SuggestGrammarRuleValidator(
                CultureInfo documentLangForTest,
                SymbolTable symbolTable,
                SuggestGrammarRuleConfiguration config) :
                base(
                    config.Level,
                    documentLangForTest,
                    symbolTable)
        {
            this.Config = config;

            // GrammarRuleと、検出された場合のメッセージを紐づける。
            List<(GrammarRule rule, string suggest)> temp = new List<(GrammarRule rule, string suggest)>();
            foreach (KeyValuePair<string, string> pair in config.GrammarRuleMap)
            {
                temp.Add((GrammarRuleExtractor.Run(pair.Key), pair.Value));
            }
            this.GrammarRules = temp.ToImmutableList();
        }

        /// <summary>
        /// Validate.
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>A list of ValidationErrors.</returns>
        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            foreach ((GrammarRule rule, string suggest) in this.GrammarRules)
            {
                List<ImmutableList<TokenElement>> list = rule.MatchExtend(sentence.Tokens);

                if (list.Any())
                {
                    foreach (ImmutableList<TokenElement> matchedTokens in list)
                    {
                        // 文法ルールにマッチした場合、マッチした文字列、文法ルール、提案の3つをメッセージとして出力する。
                        var surface = string.Join("", matchedTokens.Select(t => t.Surface));

                        result.Add(new ValidationError(
                            ValidationType.SuggestGrammarRule,
                            this.Level,
                            sentence,
                            matchedTokens.First().OffsetMap[0],
                            matchedTokens.Last().OffsetMap[^1],
                            MessageArgs: new object[] { surface, rule.ToString(), suggest }));
                    }
                }
            }

            return result;
        }
    }
}
