using System;
using System.Collections.Generic;
using System.Globalization;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Validators.SentenceValidator;
using Xunit;
using Xunit.Abstractions;

namespace RedPen.Net.Core.Tests.Validator.SentenceValidator
{
    public class DoubledWordValidatorTests
    {
        private ITestOutputHelper output;

        public DoubledWordValidatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("001", "the good item is a good example.", 3, 1, "good")]
        [InlineData("002", "Good item is a good example.", 3, 1, "good")]
        [InlineData("003", "A validator is a validator.", 3, 1, "validator")]
        // MinLength
        [InlineData("004", "A validator is a validator.", 10, 0, "")]
        // Defaulst WordSet
        [InlineData("005", "That is true, as far as I know.", 1, 0, "")]
        [InlineData("006", "Each instance in distributed search engines stores the the fractions of data.", 3, 0, "")]
        public void EnglishBasicTest(string nouse1, string text, int minLength, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // DefaultResourceの読み込み。
            HashSet<string> ignoreWords = new HashSet<string>();
            string v = DefaultResources.DoubledWordSkiplist_en;
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ignoreWords.Add(line.Trim());
            }

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledWordConfiguration(
                ValidationLevel.ERROR,
                ignoreWords,
                minLength);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledWordValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Theory]
        [InlineData("001", "RedPen is RedPen right?", "redpen", 3, 0, "")]
        [InlineData("002", "RedPen is RedPen right?", "RedPen", 3, 0, "")]
        public void EnglishAdditionalIgnoreWordTest(string nouse1, string text, string ignoreWordsStr, int minLength, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("en-US");

            // DefaultResourceの読み込み。
            HashSet<string> ignoreWords = new HashSet<string>();
            string v = DefaultResources.DoubledWordSkiplist_en;
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ignoreWords.Add(line.Trim());
            }

            foreach (string ignoreWord in ignoreWordsStr.Split(','))
            {
                // MEMO: 英語の場合小文字でマッチングを取るので、無視単語は小文字に変換しておく必要がある。
                ignoreWords.Add(ignoreWord.Trim().ToLower());
            }

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledWordConfiguration(
                ValidationLevel.ERROR,
                ignoreWords,
                minLength);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledWordValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }

        [Theory]
        [InlineData("001", "こんにちは、そしてこんにちは！", 100, 1, "こんにちは")] // 日本語の場合MinLengthは内部で無視され1になる。
        [InlineData("002", "それは真実であり，それが正しい。", 3, 1, "それ")]
        [InlineData("003", "青は藍より出でて藍より青し。", 3, 2, "藍,より")] // 青と青しは名詞と形容詞でTokenのSurfaceが異なるためエラーにならない点に注意。
        public void JapaneseBasicTest(string nouse1, string text, int minLength, int errorCount, string expected)
        {
            // Document
            CultureInfo documentLang = CultureInfo.GetCultureInfo("ja-JP");

            // DefaultResourceの読み込み。
            HashSet<string> ignoreWords = new HashSet<string>();
            // 日本語のデフォルト無視リストを使う。
            string v = DefaultResources.DoubledWordSkiplist_ja;
            foreach (string line in v.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ignoreWords.Add(line.Trim());
            }

            // ValidatorConfiguration
            var validatorConfiguration = new DoubledWordConfiguration(
                ValidationLevel.ERROR,
                ignoreWords,
                minLength);

            // カスタムシンボルを使わない場合は空リストを渡す。デフォルトシンボルはnew時に自動的にSymbolTableにロードされる。
            SymbolTable symbolTable = new SymbolTable(documentLang, "", new List<Symbol>());

            // Validator
            var validator = new DoubledWordValidator(
                documentLang,
                symbolTable,
                validatorConfiguration);

            ValidatorTestsUtility.CommonSentenceWithPlainTextParseErrorPatternTest(
                validator,
                text,
                documentLang,
                errorCount,
                expected,
                output);
        }
    }
}
