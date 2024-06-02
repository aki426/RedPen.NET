//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Globals;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core
{
    public record RedPen
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        ImmutableList<ValidationDefinition> ValidationDefinitions { get; init; }
        ImmutableList<ErrorMessageDefinition> ErrorMessageDefinitions { get; init; }

        public Configuration Configuration { get; init; }

        /// <summary>
        /// RedPen.Net.Coreの利用者側で、アドインValidatiorを追加する場合のコンストラクタ。
        /// Initializes a new instance of the <see cref="RedPen"/> class.
        /// </summary>
        /// <param name="validationDefinitions">The validation definitions.</param>
        /// <param name="errorMessageDefinitions">The error message definitions.</param>
        public RedPen(
            ImmutableList<ValidationDefinition> validationDefinitions,
            ImmutableList<ErrorMessageDefinition> errorMessageDefinitions)

        {
            ValidationDefinitions = validationDefinitions;
            ErrorMessageDefinitions = errorMessageDefinitions;
        }

        /// <summary>
        /// RedPen.Net.CoreのデフォルトValidatiorしか使用しない場合のコンストラクタ。
        /// Initializes a new instance of the <see cref="RedPen"/> class.
        /// </summary>
        public RedPen() : this(ImmutableList<ValidationDefinition>.Empty, ImmutableList<ErrorMessageDefinition>.Empty) { }

        /// <summary>
        /// 現在のRedPenの設定を基に、JsonテキストからConfigurationをロードする。
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>A Configuration.</returns>
        public Configuration LoadConfiguration(string jsonString)
        {
            // Jsonテキストから未定義のValidatorConfigurationを読み込まないためにValidatorConfigurationの型定義を渡す。
            var jsonLoader = new ConfigurationLoader(
                DefaultValidationDefinition.ValidationNameToValidatorConfigurationTypeMap.AddRange(
                    ValidationDefinitions.Select(i => new KeyValuePair<string, Type>(i.ValidationName, i.ValidatorConfigurationType))
                ));

            return jsonLoader.Load(jsonString);
        }

        /// <summary>
        /// Configurationで定義されたValidatorを生成する。
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>A list of Validators.</returns>
        public List<Validator> GetValidators(Configuration config)
        {
            // 応用アプリケーション側で追加したValidationの情報を元にValidatorを生成する。
            // 生成されるValidatorはどのみちConfigurationの定義で制限されているので、ValidatorFactoryは追加の情報を与えるだけで良い。
            var validatorFactory = new ValidatorFactory(
                ValidationDefinitions.Select(i => new KeyValuePair<string, Type>(i.ValidationName, i.ValidatorType))
                .ToImmutableDictionary());

            List<Validator> validators = new List<Validator>();
            foreach (ValidatorConfiguration conf in config.ValidatorConfigurations.Where(i => i.Level != ValidationLevel.OFF))
            {
                Validator? validator = validatorFactory.GetValidator(config.DocumentCultureInfo, config.SymbolTable, conf);
                if (validator != null)
                {
                    validators.Add(validator);
                }
                else
                {
                    LOG.Warn("Failed to create validator: {0}", conf.ValidationName);
                    throw new ArgumentException("Failed to create validator: " + conf.ValidationName);
                }
            }

            return validators;
        }

        /// <summary>
        /// ドキュメントのParse。Parserを選択することでファイルフォーマットに対応する。
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="config">The config.</param>
        /// <param name="documentText">The document text.</param>
        /// <returns>A Document.</returns>
        public static Document Parse(IDocumentParser parser, Configuration config, string documentText)
        {
            Document document;
            try
            {
                // NOTE: ドキュメントのフォーマットは入力ファイルの拡張子などで判別するのが一般的だが、
                // それはCoreライブラリの領域ではないのでParserの指定は応用アプリケーションで行い、選択はCoreで行う。
                document = parser.Parse(
                    documentText,
                    new SentenceExtractor(config.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(config.DocumentCultureInfo));
            }
            catch (Exception e)
            {
                throw;
            }

            return document;
        }

        public static List<ValidationError> Validate(Document document, List<Validator> validators)
        {
            List<ValidationError> errors = new List<ValidationError>();
            // Validate Document
            foreach (IDocumentValidatable validator in validators.Where(v => v is IDocumentValidatable)
                .Select(v => v as IDocumentValidatable).Where(v => v != null))
            {
                errors.AddRange(validator.Validate(document));
            }
            // Validate Sentences
            foreach (ISentenceValidatable validator in validators.Where(v => v is ISentenceValidatable)
                .Select(v => v as ISentenceValidatable).Where(v => v != null))
            {
                foreach (var sentence in document.GetAllSentences().Where(s => s.Content != ""))
                {
                    errors.AddRange(validator.Validate(sentence));
                }
            }

            return errors;
        }
    }
}

// NOTE: PreprocessorRule関係のコードは参考までに残しておく。

///// <summary>
///// Apply the preprocessor rules in the document to the valudation errors
///// </summary>
///// <param name="document"></param>
///// <param name="errors"></param>
//private void ApplyPreprocessorRules(Document document, List<ValidationError> errors)
//{
//    HashSet<ValidationError> suppressedErrors = new HashSet<ValidationError>();
//    if (document.PreprocessorRules != null)
//    {
//        foreach (ValidationError error in errors)
//        {
//            foreach (PreprocessorRule rule in document.PreprocessorRules)
//            {
//                switch (rule.Type)
//                {
//                    case PreprocessorRule.RuleType.SUPPRESS:
//                        if (rule.IsTriggeredBy(document, error.LineNumber, error.ValidationName))
//                        {
//                            suppressedErrors.Add(error);
//                        }
//                        break;
//                }
//            }
//        }
//    }
//    errors.RemoveAll(suppressedErrors.Contains);
//}

///// <summary>
///// Apply the preprocessor rules in each document to its relevent validation errors
///// </summary>
///// <param name="documents"></param>
///// <param name="docErrorsMap"></param>
//private void ApplyPreprocessorRules(List<Document> documents, Dictionary<Document, List<ValidationError>> docErrorsMap)
//{
//    foreach (Document document in documents)
//    {
//        ApplyPreprocessorRules(document, docErrorsMap[document]);
//    }
//}
