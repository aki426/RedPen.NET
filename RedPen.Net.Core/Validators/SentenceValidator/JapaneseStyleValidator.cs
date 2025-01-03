﻿//   Copyright (c) 2024 KANEDA Akihiro <taoist.aki@gmail.com>
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using NLog;
using RedPen.Net.Core.Config;
using RedPen.Net.Core.Model;

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

        // MEMO: コンストラクタの引数定義は共通にすること。
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

        // 複合助動詞「～だと」「～たの」「～だったの」は「だ・である調」でも「です・ます調」でも許容するため、どちらにも入れない。
        // そのため、どちらのパターンでもエラーとして扱われなくなる。

        // TODO: ですます調↔だである調の修正提案をしたい。ValidationSuggestionなどというクラスを別途作った方が良さそう。

        /// <summary>だ・である調の助動詞のリスト</summary>
        private static List<string> DaDearuPattern = new List<string>()
        {
            // TODO: 来る、食べる、起きる、元気になるの「る」が必要。例）来る→来ます、食べる→食べます、起きる→起きます、元気になる→元気になります、など。
            "た", // ました
            "だ", // です
            "だった", // でした
            "だろ", //
            "だろう",  // でしょう
            "であった", // でした
            "である", // です
            "であろう", // でしょう
            "ない", // ません（※単純な置換は成立しない。動詞の活用形を考慮する必要がある。例）OK: 操作しない→操作しません、NG: 行かない→行かません）
            "ぬ", // 現代語だとナ行変格活用は「死ぬ」くらいか。例）死ぬ→死にます
        };

        /// <summary>です・ます調の助動詞のリスト</summary>
        private static List<string> DesuMasuPattern = new List<string>()
        {
            "でした", // だった
            "でしょう", // だろう
            "です", // だ
            "ないでしょう", // ないだろう
            "ないです", // ない
            "ました", // た
            "ます", // TODO: 活用を考慮が必要。例）行きます→行く
            "ません" // TODO: 活用を考慮が必要。例）行きません→行かない
        };

        /// <summary>
        /// センテンスの末尾が言い切り表現になっているかどうかを検出する関数。
        /// 「言い切り表現」とは、文末が「動詞＋助詞」の形で終わっており助動詞が現れないパターンを示す。
        /// </summary>
        /// <param name="sentence">The sentence.</param>
        /// <returns>言い切り表現になっている場合はTrue</returns>
        public static (bool success, TokenElement tokenInError) IsEndWithDoshiWithoutJodoshi(Sentence sentence)
        {
            // Tokenがない場合は言い切り表現とみなす。
            if (!sentence.Tokens.Any())
            {
                // sucdess == trueの場合はtokenInErrorは使わないので適当な値をセット。
                return (false, new TokenElement(
                    "",
                    new List<string>(),
                    sentence.LineNumber,
                    sentence.StartPositionOffset));
            }

            // 記号や助詞を除いた最後のTokenが助動詞では無く動詞だった場合は言い切り表現とみなす。
            // MEMO: 名詞だった場合は体言止めなので、また別のValidationで検出すべきケースになるため、
            // 助動詞があるべき動詞によるセンテンス終わりのみを検出する。
            var noSymbols = sentence.Tokens.Where(t => t.PartOfSpeech[0] is not ("記号" or "助詞")).ToList();
            if (noSymbols.Any() && (noSymbols.Last().PartOfSpeech[0] is ("動詞" or "形容詞")))
            {
                return (true, noSymbols.Last());
            }
            else
            {
                // sucdess == trueの場合はtokenInErrorは使わないので適当な値をセット。
                return (false, sentence.Tokens.Last());
            }
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
                if (token.PartOfSpeech[0] == "助動詞")
                {
                    buffer.Add(token);
                }
                else
                {
                    if (buffer.Any())
                    {
                        // TODO: GrammarRuleでのパターンマッチへのリファクタリングを検討する。

                        // MEMO: 助動詞「だ」の後に助詞「と」が続く場合は、助動詞「だと」とみなして1Tokenとして登録する。
                        // 「だと」という表現はです・ます調でも不自然ではない表現として使われるため。
                        // 例）「今日は雨だと思います。」
                        if (buffer.Any()
                            && buffer.Last().Surface == "だ"
                            && token.Surface == "と"
                            && token.PartOfSpeech[0] == "助詞")
                        {
                            buffer.Add(token);
                        }

                        // MEMO: 助動詞「た」の後に「の」が続く場合は、助動詞「たの」「だったの」とみなして1Tokenとして登録する。
                        // 「だと」という表現はです・ます調でも不自然ではない表現として使われるため。
                        // 例）「今日は雨だったのです。」
                        if (buffer.Any()
                            && buffer.Last().Surface == "た"
                            && token.Surface == "の"
                            && token.PartOfSpeech[0] == "名詞" && token.PartOfSpeech[1] == "非自立"
                            )
                        {
                            buffer.Add(token);
                        }

                        // MEMO: 助動詞「ない」の後に「の」が続く場合は、助動詞「ないの」とみなして1Tokenとして登録する。
                        // 「ないの」という表現はです・ます調でも不自然ではない表現として使われるため。
                        // 例）「今日は雨は降らないのです。」
                        if (buffer.Any()
                            && buffer.Last().Surface == "ない"
                            && token.Surface == "の"
                            && token.PartOfSpeech[0] == "名詞" && token.PartOfSpeech[1] == "非自立"
                            )
                        {
                            buffer.Add(token);
                        }

                        // 連結した助動詞を1Tokenとして登録。
                        CompoundJodoshi.Add(new TokenElement(
                            string.Join("", buffer.Select(t => t.Surface)),
                            buffer.First().PartOfSpeech, // Tagsは使わないので先頭のTokenのものを暫定的にセット。
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
                    buffer.First().PartOfSpeech, // Tagsは使わないので先頭のTokenのものを暫定的にセット。
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
                    // です・ます調の場合、末尾が言い切り表現になっている場合はです・ます表現ではないとみなせるので末尾をエラーにする。
                    var endCheck = IsEndWithDoshiWithoutJodoshi(sentence);
                    if (endCheck.success)
                    {
                        errorJodoshi.Add(endCheck.tokenInError);
                    }

                    break;
            }

            foreach (var token in errorJodoshi)
            {
                result.Add(new ValidationError(
                    ValidationName,
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
