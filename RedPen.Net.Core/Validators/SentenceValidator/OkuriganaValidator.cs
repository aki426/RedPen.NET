﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Tokenizer;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Validators.SentenceValidator
{
    /// <summary>OkuriganaのConfiguration</summary>
    public record OkuriganaConfiguration : ValidatorConfiguration
    {
        public OkuriganaConfiguration(ValidationLevel level) : base(level)
        {
        }
    }

    /// <summary>OkuriganaのValidator</summary>
    public class OkuriganaValidator : Validator, ISentenceValidatable
    {
        /// <summary>Nlog</summary>
        private static Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>ConfigurationGets the config.</summary>
        public OkuriganaConfiguration Config { get; init; }

        /// <summary>サポート言語はja-JPのみ</summary>
        public override List<string> SupportedLanguages => new List<string>() { "ja-JP" };

        /// <summary>不正な送り仮名の読みのリスト</summary>
        private static readonly ImmutableList<string> InvalidOkurigana;

        /// <summary>不正な送り仮名をTokenで表現した語順パターンのリスト</summary>
        private static readonly ImmutableList<ExpressionRule> InvalidOkuriganaTokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="OkuriganaValidator"/> class.
        /// </summary>
        static OkuriganaValidator()
        {
            // TODO: Tokenに分割した表現パターンはTagsを見ていないので厳密性に欠ける。
            // 一方、Kuromoji+ipadicのTokenizeはそこまで精度が高くないので、Tagsも考慮したマッチングで
            // 期待通りに不正な送り仮名を検出できるかは不明。

            InvalidOkuriganaTokens = new List<ExpressionRule>
            {
                ExpressionRuleExtractor.Run("合さ:動詞,自立"),
                ExpressionRuleExtractor.Run("合し:動詞,自立"),
                ExpressionRuleExtractor.Run("合す:動詞,自立"),
                ExpressionRuleExtractor.Run("合せ:動詞,自立"),
                ExpressionRuleExtractor.Run("押え:動詞,自立"),
                ExpressionRuleExtractor.Run("押える:動詞,自立"),
                ExpressionRuleExtractor.Run("押えれ:動詞,自立"),
                ExpressionRuleExtractor.Run("押えろ:動詞,自立"),
                ExpressionRuleExtractor.Run("押えよ:動詞,自立"),
                ExpressionRuleExtractor.Run("起ら:動詞,自立"),
                ExpressionRuleExtractor.Run("起り:動詞,自立"),
                ExpressionRuleExtractor.Run("起る:動詞,自立"),
                ExpressionRuleExtractor.Run("起れ:動詞,自立"),
                ExpressionRuleExtractor.Run("起ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("起よ:動詞,自立"),
                ExpressionRuleExtractor.Run("起:動詞,自立"),
                ExpressionRuleExtractor.Run("著かろ:形容詞,自立"),
                ExpressionRuleExtractor.Run("著く:形容詞,自立"),
                ExpressionRuleExtractor.Run("著かっ:形容詞,自立"),
                ExpressionRuleExtractor.Run("著い:形容詞,自立"),
                ExpressionRuleExtractor.Run("著けれ:形容詞,自立"),
                ExpressionRuleExtractor.Run("試:動詞,自立"),
                ExpressionRuleExtractor.Run("試る:動詞,自立"),
                ExpressionRuleExtractor.Run("試れ:動詞,自立"),
                ExpressionRuleExtractor.Run("試ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("試よ:動詞,自立"),
                ExpressionRuleExtractor.Run("恥しかろ:形容詞,自立"),
                ExpressionRuleExtractor.Run("恥しく:形容詞,自立"),
                ExpressionRuleExtractor.Run("恥しかっ:形容詞,自立"),
                ExpressionRuleExtractor.Run("恥しい:形容詞,自立"),
                ExpressionRuleExtractor.Run("恥しけれ:形容詞,自立"),
                ExpressionRuleExtractor.Run("生れ:動詞,自立"),
                ExpressionRuleExtractor.Run("生れる:動詞,自立"),
                ExpressionRuleExtractor.Run("生れれ:動詞,自立"),
                ExpressionRuleExtractor.Run("生れろ:動詞,自立"),
                ExpressionRuleExtractor.Run("生れよ:動詞,自立"),
                ExpressionRuleExtractor.Run("妨:動詞,自立"),
                ExpressionRuleExtractor.Run("妨る:動詞,自立"),
                ExpressionRuleExtractor.Run("妨れ:動詞,自立"),
                ExpressionRuleExtractor.Run("妨ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("妨よ:動詞,自立"),
                ExpressionRuleExtractor.Run("進:動詞,自立"),
                ExpressionRuleExtractor.Run("進る:動詞,自立"),
                ExpressionRuleExtractor.Run("進れ:動詞,自立"),
                ExpressionRuleExtractor.Run("進ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("進よ:動詞,自立"),
                ExpressionRuleExtractor.Run("進てい:名詞,左辺接続"),
                ExpressionRuleExtractor.Run("勧:動詞,自立"),
                ExpressionRuleExtractor.Run("勧る:動詞,自立"),
                ExpressionRuleExtractor.Run("勧れ:動詞,自立"),
                ExpressionRuleExtractor.Run("勧ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("勧よ:動詞,自立"),
                ExpressionRuleExtractor.Run("考:動詞,自立"),
                ExpressionRuleExtractor.Run("考る:動詞,自立"),
                ExpressionRuleExtractor.Run("考れ:動詞,自立"),
                ExpressionRuleExtractor.Run("考ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("考よ:動詞,自立"),
                ExpressionRuleExtractor.Run("行なわ:動詞,自立"),
                ExpressionRuleExtractor.Run("行ない:動詞,自立"),
                ExpressionRuleExtractor.Run("行なう:動詞,自立"),
                ExpressionRuleExtractor.Run("行なえ:動詞,自立"),
                ExpressionRuleExtractor.Run("行なお:動詞,自立"),
                ExpressionRuleExtractor.Run("表わさ:動詞,自立"),
                ExpressionRuleExtractor.Run("表わし:動詞,自立"),
                ExpressionRuleExtractor.Run("表わす:動詞,自立"),
                ExpressionRuleExtractor.Run("表わせ:動詞,自立"),
                ExpressionRuleExtractor.Run("表わそ:動詞,自立"),
                ExpressionRuleExtractor.Run("現われ:動詞,自立"),
                ExpressionRuleExtractor.Run("現われる:動詞,自立"),
                ExpressionRuleExtractor.Run("現われれ:動詞,自立"),
                ExpressionRuleExtractor.Run("現われろ:動詞,自立"),
                ExpressionRuleExtractor.Run("現われよ:動詞,自立"),
                ExpressionRuleExtractor.Run("断わら:動詞,自立"),
                ExpressionRuleExtractor.Run("断わり:動詞,自立"),
                ExpressionRuleExtractor.Run("断わる:動詞,自立"),
                ExpressionRuleExtractor.Run("断われ:動詞,自立"),
                ExpressionRuleExtractor.Run("断わろ:動詞,自立"),
                ExpressionRuleExtractor.Run("聞え:動詞,自立"),
                ExpressionRuleExtractor.Run("聞える:動詞,自立"),
                ExpressionRuleExtractor.Run("聞えれ:動詞,自立"),
                ExpressionRuleExtractor.Run("聞えろ:動詞,自立"),
                ExpressionRuleExtractor.Run("聞えよ:動詞,自立"),
                ExpressionRuleExtractor.Run("当ら:動詞,自立"),
                ExpressionRuleExtractor.Run("当り:動詞,自立"),
                ExpressionRuleExtractor.Run("当る:動詞,自立"),
                ExpressionRuleExtractor.Run("当れ:動詞,自立"),
                ExpressionRuleExtractor.Run("当ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("落さ:動詞,自立"),
                ExpressionRuleExtractor.Run("落し:動詞,自立"),
                ExpressionRuleExtractor.Run("落す:動詞,自立"),
                ExpressionRuleExtractor.Run("落せ:動詞,自立"),
                ExpressionRuleExtractor.Run("落そ:動詞,自立"),
                ExpressionRuleExtractor.Run("終ら:動詞,自立"),
                ExpressionRuleExtractor.Run("終り:動詞,自立"),
                ExpressionRuleExtractor.Run("終る:動詞,自立"),
                ExpressionRuleExtractor.Run("終れ:動詞,自立"),
                ExpressionRuleExtractor.Run("終ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("果さ:動詞,自立"),
                ExpressionRuleExtractor.Run("果し:動詞,自立"),
                ExpressionRuleExtractor.Run("果す:動詞,自立"),
                ExpressionRuleExtractor.Run("果せ:動詞,自立"),
                ExpressionRuleExtractor.Run("果そ:動詞,自立"),
                ExpressionRuleExtractor.Run("変ら:動詞,自立"),
                ExpressionRuleExtractor.Run("変り:動詞,自立"),
                ExpressionRuleExtractor.Run("変る:動詞,自立"),
                ExpressionRuleExtractor.Run("変れ:動詞,自立"),
                ExpressionRuleExtractor.Run("変ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("買:動詞,自立"),
                ExpressionRuleExtractor.Run("買る:動詞,自立"),
                ExpressionRuleExtractor.Run("買れ:動詞,自立"),
                ExpressionRuleExtractor.Run("買よ:動詞,自立"),
                ExpressionRuleExtractor.Run("買て:名詞,一般"),
                ExpressionRuleExtractor.Run("上ら:動詞,自立"),
                ExpressionRuleExtractor.Run("上り:動詞,自立"),
                ExpressionRuleExtractor.Run("上る:動詞,自立"),
                ExpressionRuleExtractor.Run("上れ:動詞,自立"),
                ExpressionRuleExtractor.Run("上ろ:動詞,自立"),
                ExpressionRuleExtractor.Run("費さ:動詞,自立"),
                ExpressionRuleExtractor.Run("費し:動詞,自立"),
                ExpressionRuleExtractor.Run("費す:動詞,自立"),
                ExpressionRuleExtractor.Run("費せ:動詞,自立"),
                ExpressionRuleExtractor.Run("費そ:動詞,自立"),
                ExpressionRuleExtractor.Run("危かろ:形容詞,自立"),
                ExpressionRuleExtractor.Run("危く:形容詞,自立"),
                ExpressionRuleExtractor.Run("危かっ:形容詞,自立"),
                ExpressionRuleExtractor.Run("危い:形容詞,自立"),
                ExpressionRuleExtractor.Run("危:名詞,一般 + い:名詞,一般"), // Kuromoji + ipadicの辞書の場合このTokenizeが起きる。
                // ExpressionRuleExtractor.Run("危:名詞,一般 + い:動詞,自立"), // Kuromoji + ipadicの辞書の場合このTokenizeが起きる。
                ExpressionRuleExtractor.Run("危けれ:形容詞,自立"),
                ExpressionRuleExtractor.Run("逸さ:動詞,自立"),
                ExpressionRuleExtractor.Run("逸し:動詞,自立"),
                ExpressionRuleExtractor.Run("逸す:動詞,自立"),
                ExpressionRuleExtractor.Run("逸せ:動詞,自立"),
                ExpressionRuleExtractor.Run("逸そ:動詞,自立"),
                ExpressionRuleExtractor.Run("反さ:動詞,自立"),
                ExpressionRuleExtractor.Run("反し:動詞,自立"),
                ExpressionRuleExtractor.Run("反す:動詞,自立"),
                ExpressionRuleExtractor.Run("反せ:動詞,自立"),
                ExpressionRuleExtractor.Run("反そ:動詞,自立"),
                ExpressionRuleExtractor.Run("過さ:動詞,自立"),
                ExpressionRuleExtractor.Run("過し:動詞,自立"),
                ExpressionRuleExtractor.Run("過す:動詞,自立"),
                ExpressionRuleExtractor.Run("過せ:動詞,自立"),
                ExpressionRuleExtractor.Run("過そ:動詞,自立")
            }.ToImmutableList();

            InvalidOkurigana = new List<string>
            {
                "恐し",
                "短か",
                "著るしい",
                "被ぶ",
                "紛わしい",
                "逆う",
                "悔ま",
                "陥いる",
                "恥かし",
                "憐ま",
                "憐み",
                "憐む",
                "憐め",
                "商なう",
                "美い",
                "荒ら",
                "輝し",
                "静ず",
                "明か",
                "必ら",
                "再たび",
                "著わ",
                "積る",
                "替る",
                "換る",
                "開らく",
                "甚し",
                "懐ろ"
            }.ToImmutableList();
        }

        // TODO: コンストラクタの引数定義は共通にすること。
        public OkuriganaValidator(
            CultureInfo documentLangForTest,
            SymbolTable symbolTable,
            OkuriganaConfiguration config) :
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
            // 読みだけを考慮したマッチング。
            foreach (string value in InvalidOkurigana)
            {
                int startPosition = sentence.Content.IndexOf(value);
                if (startPosition != -1)
                {
                    result.Add(new ValidationError(
                        ValidationType.Okurigana,
                        this.Level,
                        sentence,
                        sentence.ConvertToLineOffset(startPosition),
                        sentence.ConvertToLineOffset(startPosition + value.Length - 1),
                        MessageArgs: new object[] { value }
                    ));
                }
            }

            // Tokenのリストを使ったマッチング。
            foreach (ExpressionRule rule in InvalidOkuriganaTokens)
            {
                (bool isMatch, List<TokenElement> tokens) value = rule.MatchSurface(sentence.Tokens);
                if (value.isMatch)
                {
                    result.Add(new ValidationError(
                        ValidationType.Okurigana,
                        this.Level,
                        sentence,
                        value.tokens.First().OffsetMap[0],
                        value.tokens.Last().OffsetMap[^1],
                        MessageArgs: new object[] { rule.ToSurface() }
                    ));
                }
            }

            return result;
        }
    }
}