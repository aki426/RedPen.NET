using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Validators;

namespace RedPen.Net.Core
{
    public class RedPen : IEquatable<RedPen>
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        public static readonly string VERSION = "1.10.4";

        private readonly Configuration configuration;
        private readonly SentenceExtractor sentenceExtractor;
        private readonly List<Validator> validators;

        ///// <summary>
        ///// constructs RedPen with specified config file.
        ///// </summary>
        ///// <param name="configFile">config file</param>
        ///// <exception cref="RedPenException">when failed to construct RedPen</exception>
        //public RedPen(FileInfo configFile)
        //{
        //    this.configuration = new ConfigurationLoader().Load(configFile);
        //    this.sentenceExtractor = new SentenceExtractor(configuration.SymbolTable);
        //    this.validators = new List<Validator>();
        //}

        ///// <summary>
        ///// constructs RedPen with specified config file path.
        ///// </summary>
        ///// <param name="configPath">config file path</param>
        ///// <exception cref="RedPenException">when failed to construct RedPen</exception>
        //public RedPen(string configPath)
        //{
        //    this.configuration = new ConfigurationLoader().LoadFromResource(configPath);
        //    this.sentenceExtractor = new SentenceExtractor(configuration.SymbolTable);
        //    this.validators = new List<Validator>();
        //}

        /// <summary>
        /// constructs RedPen with specified configuration.
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <exception cref="RedPenException">when failed to construct RedPen</exception>
        public RedPen(Configuration configuration)
        {
            this.configuration = configuration;
            this.sentenceExtractor = new SentenceExtractor(configuration.SymbolTable);
            this.validators = new List<Validator>();
        }

        /// <summary>
        /// Initializes the validators.
        /// </summary>
        /// <param name="thresholdStr">The threshold str.</param>
        private void InitializeValidators(string thresholdStr)
        {
            ValidationLevel threshold = (ValidationLevel)Enum.Parse(typeof(ValidationLevel), thresholdStr.ToUpper());
            validators.Clear();
            foreach (ValidatorConfiguration config in configuration.ValidatorConfigurations)
            {
                try
                {
                    if (config.Level.IsWorseThan(threshold))
                    {
                        // TODO: ValidatorFactoryをちゃんと実装すること。
                        // validators.Add(ValidatorFactory.GetInstance(config, configuration));
                    }
                }
                catch (RedPenException e)
                {
                    throw new InvalidOperationException("Failed to initialize validators.", e);
                }
            }
        }

        ///// <summary>
        ///// parse given input stream.
        ///// </summary>
        ///// <param name="parser">DocumentParser parser</param>
        ///// <param name="inputStream">content to parse</param>
        ///// <returns>parsed document</returns>
        ///// <exception cref="RedPenException">when failed to parse input stream</exception>
        //public Document Parse(IDocumentParser parser, Stream inputStream)
        //{
        //    return parser.Parse(inputStream, sentenceExtractor, configuration.Tokenizer);
        //}

        ///// <summary>
        ///// parse given content.
        ///// </summary>
        ///// <param name="parser">DocumentParser parser</param>
        ///// <param name="content">content to parse</param>
        ///// <returns>parsed document</returns>
        ///// <exception cref="RedPenException">when failed to parse input stream</exception>
        //public Document Parse(IDocumentParser parser, string content)
        //{
        //    return parser.Parse(content, sentenceExtractor, configuration.Tokenizer);
        //}

        ///// <summary>
        ///// parse given files.
        ///// </summary>
        ///// <param name="parser">DocumentParser parser</param>
        ///// <param name="files">files to parse</param>
        ///// <returns>parsed documents</returns>
        ///// <exception cref="RedPenException">when failed to parse input stream</exception>
        //public List<Document> Parse(IDocumentParser parser, FileInfo[] files)
        //{
        //    List<Document> documents = new List<Document>();
        //    foreach (FileInfo file in files)
        //    {
        //        documents.Add(parser.Parse(file, sentenceExtractor, configuration.Tokenizer));
        //    }
        //    return documents;
        //}

        /// <summary>
        /// validate the input document collection. Note that this method call is NOT thread safe. RedPen instances need to be crated for each thread.
        /// </summary>
        /// <param name="documents">input document collection generated by Parser</param>
        /// <returns>list of validation errors</returns>
        public Dictionary<Document, List<ValidationError>> Validate(List<Document> documents)
        {
            return Validate(documents, "error");
        }

        /// <summary>
        /// validate the input document collection. Note that this method call is NOT thread safe. RedPen instances need to be crated for each thread.
        /// </summary>
        /// <param name="documents">input document collection generated by Parser</param>
        /// <param name="threshold">threshold of error level</param>
        /// <returns>list of validation errors</returns>
        public Dictionary<Document, List<ValidationError>> Validate(List<Document> documents, string threshold)
        {
            Dictionary<Document, List<ValidationError>> docErrorsMap = new Dictionary<Document, List<ValidationError>>();
            foreach (Document e in documents)
            {
                docErrorsMap[e] = new List<ValidationError>();
            }
            InitializeValidators(threshold);
            RunDocumentValidators(documents, docErrorsMap);
            RunSectionValidators(documents, docErrorsMap);
            RunSentenceValidators(documents, docErrorsMap);
            ApplyPreprocessorRules(documents, docErrorsMap);
            return docErrorsMap;
        }

        /// <summary>
        /// validate the input document. Note that this method call is NOT thread safe. RedPen instances need to be crated for each thread.
        /// </summary>
        /// <param name="document">document to be validated</param>
        /// <returns>list of validation errors</returns>
        public List<ValidationError> Validate(Document document)
        {
            return Validate(document, "error");
        }

        /// <summary>
        /// validate the input document. Note that this method call is NOT thread safe. RedPen instances need to be crated for each thread.
        /// </summary>
        /// <param name="document">document to be validated</param>
        /// <param name="threshold">threshold of error level</param>
        /// <returns>list of validation errors</returns>
        public List<ValidationError> Validate(Document document, string threshold)
        {
            List<Document> documents = new List<Document> { document };
            Dictionary<Document, List<ValidationError>> documentListMap = Validate(documents, threshold);
            return documentListMap[document];
        }

        /// <summary>
        /// Get validators associated with this RedPen instance
        /// </summary>
        /// <returns>validators</returns>
        public ImmutableList<Validator> GetValidators()
        {
            // TODO: 暫定的にImmutableListを採用したが、実際の使用ケースを確認すること。
            return validators.ToImmutableList();
        }

        /// <summary>
        /// Get the configuration object for this RedPen
        /// </summary>
        /// <returns>The configuration object for this RedPen</returns>
        public Configuration GetConfiguration()
        {
            return configuration;
        }

        /// <summary>
        /// Apply the preprocessor rules in the document to the valudation errors
        /// </summary>
        /// <param name="document"></param>
        /// <param name="errors"></param>
        private void ApplyPreprocessorRules(Document document, List<ValidationError> errors)
        {
            HashSet<ValidationError> suppressedErrors = new HashSet<ValidationError>();
            if (document.PreprocessorRules != null)
            {
                foreach (ValidationError error in errors)
                {
                    foreach (PreprocessorRule rule in document.PreprocessorRules)
                    {
                        switch (rule.Type)
                        {
                            case PreprocessorRule.RuleType.SUPPRESS:
                                if (rule.IsTriggeredBy(document, error.LineNumber, error.Type.ValidationName()))
                                {
                                    suppressedErrors.Add(error);
                                }
                                break;
                        }
                    }
                }
            }
            errors.RemoveAll(suppressedErrors.Contains);
        }

        /// <summary>
        /// Apply the preprocessor rules in each document to its relevent validation errors
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="docErrorsMap"></param>
        private void ApplyPreprocessorRules(List<Document> documents, Dictionary<Document, List<ValidationError>> docErrorsMap)
        {
            foreach (Document document in documents)
            {
                ApplyPreprocessorRules(document, docErrorsMap[document]);
            }
        }

        /// <summary>
        /// Runs the document validators.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="docErrorsMap">The doc errors map.</param>
        private void RunDocumentValidators(List<Document> documents, Dictionary<Document, List<ValidationError>> docErrorsMap)
        {
            // run Section validator to documents
            foreach (Document document in documents)
            {
                List<ValidationError> errors = new List<ValidationError>();
                foreach (Validator e in validators)
                {
                    if (e is IDocumentValidatable)
                    {
                        errors.AddRange(((IDocumentValidatable)e).Validate(document));
                    }
                }
                docErrorsMap[document] = errors;
            }
        }

        /// <summary>
        /// Runs the section validators.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="docErrorsMap">The doc errors map.</param>
        private void RunSectionValidators(List<Document> documents, Dictionary<Document, List<ValidationError>> docErrorsMap)
        {
            foreach (Document document in documents)
            {
                foreach (Section section in document.Sections)
                {
                    List<ValidationError> errors = docErrorsMap[document];
                    foreach (Validator e in validators)
                    {
                        if (e is ISectionValidatable)
                        {
                            errors.AddRange(((ISectionValidatable)e).Validate(section));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Runs the sentence validators.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="docErrorsMap">The doc errors map.</param>
        private void RunSentenceValidators(List<Document> documents, Dictionary<Document, List<ValidationError>> docErrorsMap)
        {
            // run Sentence Validators to documents
            foreach (Document document in documents)
            {
                foreach (Section section in document.Sections)
                {
                    List<ValidationError> errors = docErrorsMap[document];

                    // apply SentenceValidations to section
                    // apply paragraphs
                    foreach (Paragraph paragraph in section.Paragraphs)
                    {
                        foreach (Validator e in validators)
                        {
                            if (e is ISentenceValidatable)
                            {
                                foreach (Sentence sentence in paragraph.Sentences)
                                {
                                    errors.AddRange(((ISentenceValidatable)e).Validate(sentence));
                                }
                            }
                        }
                    }
                    // apply to section header
                    foreach (Validator e in validators)
                    {
                        if (e is ISentenceValidatable)
                        {
                            foreach (Sentence sentence in section.HeaderSentences)
                            {
                                errors.AddRange(((ISentenceValidatable)e).Validate(sentence));
                            }
                        }
                    }
                    // apply to lists
                    foreach (ListBlock listBlock in section.ListBlocks)
                    {
                        foreach (ListElement listElement in listBlock.ListElements)
                        {
                            foreach (Validator e in validators)
                            {
                                if (e is ISentenceValidatable)
                                {
                                    foreach (Sentence sentence in listElement.Sentences)
                                    {
                                        errors.AddRange(((ISentenceValidatable)e).Validate(sentence));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Configurationプロパティ等価性により等価性を判定します。
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public bool Equals(RedPen other)
        {
            return configuration.Equals(other.configuration);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>Null参照の場合0を返す。</returns>
        public override int GetHashCode()
        {
            return configuration == null ? 0 : configuration.GetHashCode();
        }

        /// <summary>
        /// Tos the string.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "RedPen{" +
                   "configuration=" + configuration +
                   ", sentenceExtractor=" + sentenceExtractor +
                   ", validators=" + validators +
                   '}';
        }
    }
}
