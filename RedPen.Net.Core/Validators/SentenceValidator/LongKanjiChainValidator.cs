using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>LongKanjiChainのConfiguration</summary>
    public record LongKanjiChainConfiguration : ValidatorConfiguration, IMinLengthConfigParameter, IExpressionSetConfigParameter
    {
        public int MinLength { get; init; }
        public HashSet<string> ExpressionSet { get; init; }

        public LongKanjiChainConfiguration(ValidationLevel level, int minLength, HashSet<string> expressionSet) : base(level)
        {
            this.MinLength = minLength;
            this.ExpressionSet = expressionSet;
        }
    }

    /// <summary>LongKanjiChainのValidator</summary>
    public class LongKanjiChainValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ValidatorConfiguration</summary>
        public LongKanjiChainConfiguration Config { get; init; }

        /// <summary>日本語のみ許容</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        /// <summary>漢字の連続を表す正規表現</summary>
        private Regex pat;

        // TODO: コンストラクタの引数定義は共通にすること。
        /// <summary>
        /// Initializes a new instance of the <see cref="LongKanjiChainValidator"/> class.
        /// </summary>
        /// <param name="documentLangForTest">The document lang for test.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="config">The config.</param>
        public LongKanjiChainValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            LongKanjiChainConfiguration config) :
            base(
                config.Level,
                documentLangForTest,
                symbolTable)
        {
            this.Config = config;

            // 出現回数を考慮した正規表現を生成。
            pat = new Regex($"[\\u4e00-\\u9faf]{{{Config.MinLength},}}");
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
            MatchCollection matches = pat.Matches(sentence.Content);

            foreach (Match match in matches)
            {
                string word = match.Value;
                if (!Config.ExpressionSet.Contains(word))
                {
                    result.Add(new ValidationError(
                        ValidationName,
                        this.Level,
                        sentence,
                        sentence.ConvertToLineOffset(match.Index),
                        sentence.ConvertToLineOffset(match.Index + match.Length - 1),
                        MessageArgs: new object[] { word, word.Length, Config.MinLength }));
                }
            }

            return result;
        }
    }
}
