using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

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

        public List<ValidationError> Validate(Sentence sentence)
        {
            List<ValidationError> result = new List<ValidationError>();

            // validation
            int stackSize = 0;
            var surfaces = new List<TokenElement>();
            foreach (TokenElement tokenElement in sentence.Tokens)
            {
                var tags = tokenElement.Tags;
                switch (stackSize)
                {
                    case 0:
                        if (tags[0].Equals("名詞"))
                        {
                            surfaces.Add(tokenElement);
                            stackSize = 1;
                        }
                        break;

                    case 1:
                        if (tags[0].Equals("助詞") && tokenElement.Surface.Equals("の"))
                        {
                            surfaces.Add(tokenElement);
                            stackSize = 2;
                        }
                        break;

                    case 2:
                        if (tags[0].Equals("名詞"))
                        {
                            surfaces.Add(tokenElement);
                        }
                        else
                        {
                            if (tags[0].Equals("助詞") && tokenElement.Surface.Equals("の"))
                            {
                                surfaces.Add(tokenElement);
                                stackSize = 3;
                            }
                            else
                            {
                                surfaces.Clear();
                                stackSize = 0;
                            }
                        }
                        break;

                    case 3:
                        if (tags[0].Equals("名詞"))
                        {
                            surfaces.Add(tokenElement);
                        }
                        else
                        {
                            var surface = string.Join("", surfaces.Select(t => t.Surface));
                            if (!Config.WordSet.Contains(surface))
                            {
                                result.Add(new ValidationError(
                                    ValidationType.JapaneseAmbiguousNounConjunction,
                                    this.Level,
                                    sentence,
                                    surfaces.First().OffsetMap[0],
                                    surfaces.Last().OffsetMap[^1],
                                    MessageArgs: new object[] { surface }));
                            }
                            stackSize = 0;
                        }
                        break;
                }
            }

            // TODO: MessageKey引数はErrorMessageにバリエーションがある場合にValidator内で条件判定して引数として与える。

            return result;
        }
    }
}
