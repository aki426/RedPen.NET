using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using RedPen.Net.Core.Errors;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Parser;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Validators;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace RedPen.Net.Core.Tests.Validators
{
    public static class ValidatorTestsUtility
    {
        /// <summary>
        /// 入力文を単一センテンスとしてValidationを実行する。
        /// </summary>
        /// <param name="validator">Sentence validator.</param>
        /// <param name="text">The text.</param>
        /// <param name="documentLang">The document lang.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="output">The output.</param>
        public static void CommonSentenceErrorPatternTest(
            ISentenceValidatable validator,
            string text,
            CultureInfo documentLang,
            int errorCount,
            string expected,
            ITestOutputHelper output)
        {
            // Tokenizeが走るので大仰でもDocumentを生成する。
            Document document = Document.Builder(RedPenTokenizerFactory.CreateTokenizer(documentLang))
                    .AddSection(1)
                    .AddParagraph()
                    .AddSentence(new Sentence(text, 1))
                    .Build(); // TokenizeをBuild時に実行する。

            var sentence = document.Sections[0].Paragraphs[0].Sentences[0];

            output.WriteLine("★全Token:");
            foreach (var token in sentence.Tokens)
            {
                output.WriteLine(token.ToString());
            }

            // Validation
            var errors = validator.Validate(sentence);

            errors.Should().HaveCount(errorCount);

            if (errors.Any())
            {
                var manager = ErrorMessageManager.GetInstance();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }

        /// <summary>
        /// 入力文を複数センテンスの可能性があるPlainTestとしてValidationを実行する。
        /// </summary>
        /// <param name="validator">Sentence validator.</param>
        /// <param name="text">The text.</param>
        /// <param name="documentLang">The document lang.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="output">The output.</param>
        public static void CommonSentenceWithPlainTextParseErrorPatternTest(
            ISentenceValidatable validator,
            string text,
            CultureInfo documentLang,
            int errorCount,
            string expected,
            ITestOutputHelper output)
        {
            // PlainTextParseにより原文をParseする。複数センテンスの場合これでセンテンス分割ができる。

            Document document = null;
            // Lang設定以外はデフォルト。
            //Configuration configuration = Configuration.Builder()
            //    .SetLang(documentLang.Name)
            //    .Build();
            Configuration configuration = new Configuration()
            {
                DocumentLang = documentLang.Name,
                Variant = "",
                ValidatorConfigurations = new List<ValidatorConfiguration>(),
                Symbols = new List<Symbol>(),
            };

            // Parse
            try
            {
                document = new PlainTextParser().Parse(
                    text,
                    new SentenceExtractor(configuration.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(configuration.DocumentCultureInfo));
            }
            catch (Exception e)
            {
                Assert.True(false, "Exception not expected.");
            }

            // 全センテンスに対してValidationを実行する。
            List<ValidationError> errors = new List<ValidationError>();
            foreach (var sentence in document.GetAllSentences())
            {
                output.WriteLine("★全Token:");
                foreach (var token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");

                errors.AddRange(validator.Validate(sentence));
            }

            // エラーの数検証。
            errors.Should().HaveCount(errorCount);

            // 全エラーの可視化とエラー内容の検証。
            if (errors.Any())
            {
                var manager = ErrorMessageManager.GetInstance();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
            }
        }

        /// <summary>
        /// 入力文を複数センテンスの可能性があるPlainTestとしてValidationを実行する。
        /// </summary>
        /// <param name="validator">Document validator.</param>
        /// <param name="text">The text.</param>
        /// <param name="documentLang">The document lang.</param>
        /// <param name="errorCount">The error count.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="output">The output.</param>
        public static void CommonDocumentWithPlainTextParseErrorPatternTest(
            IDocumentValidatable validator,
            string text,
            CultureInfo documentLang,
            int errorCount,
            string expected,
            ITestOutputHelper output)
        {
            // PlainTextParseにより原文をParseする。複数センテンスの場合これでセンテンス分割ができる。

            Document document = null;
            // Lang設定以外はデフォルト。
            //Configuration configuration = Configuration.Builder()
            //    .SetLang(documentLang.Name)
            //    .Build();
            Configuration configuration = new Configuration()
            {
                DocumentLang = documentLang.Name,
                Variant = "",
                ValidatorConfigurations = new List<ValidatorConfiguration>(),
                Symbols = new List<Symbol>(),
            };

            // Parse
            try
            {
                document = new PlainTextParser().Parse(
                    text,
                    new SentenceExtractor(configuration.SymbolTable),
                    RedPenTokenizerFactory.CreateTokenizer(configuration.DocumentCultureInfo));
            }
            catch (Exception e)
            {
                Assert.True(false, "Exception not expected.");
            }

            // 目視確認。
            foreach (var sentence in document.GetAllSentences())
            {
                output.WriteLine($"[{sentence.Content}]");
                output.WriteLine("★全Token:");
                foreach (var token in sentence.Tokens)
                {
                    output.WriteLine(token.ToString());
                }
                output.WriteLine("");
            }

            // Documentに対してValidationを実行する。
            List<ValidationError> errors = validator.Validate(document);

            //List<ValidationError> errors = new List<ValidationError>();

            // エラーの数検証。
            errors.Should().HaveCount(errorCount);

            // 全エラーの可視化とエラー内容の検証。
            if (errors.Any())
            {
                var manager = ErrorMessageManager.GetInstance();

                output.WriteLine("");
                output.WriteLine("★Errors:");
                foreach (var error in errors)
                {
                    output.WriteLine(error.ToString());
                    output.WriteLine(manager.GetErrorMessage(error, CultureInfo.GetCultureInfo("ja-JP")));
                }

                // デリミタ間違えるので「,」でも「|」でもどちらでもよいようにした。
                var commaSeparated = string.Join(",", errors.Select(e => e.MessageArgs[0].ToString()));
                if (commaSeparated == expected)
                {
                    string.Join(",", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
                }
                else
                {
                    string.Join("|", errors.Select(e => e.MessageArgs[0].ToString())).Should().Be(expected);
                }
            }
        }
    }
}
