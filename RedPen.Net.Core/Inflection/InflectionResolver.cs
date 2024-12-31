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
using RedPen.Net.Core.Model;
using RedPen.Net.Core.Utility;

namespace RedPen.Net.Core.Inflection
{
    /// <summary>
    /// 活用変化のある品詞の活用形を解決するためのクラス。
    /// </summary>
    public class InflectionResolver
    {
        /// <summary>
        /// TokenElementを任意の活用形に変換する関数。
        /// NOTE: 品詞の指定は引数のTokenElementに含まれているため自動的に解決される。
        /// 文字列として活用形を与えるだけでよい。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static (bool success, string surface) Resolve(TokenElement token, string type)
        {
            return token.PartOfSpeech[0] switch
            {
                "動詞" => ResolveAsVerb(token, type),
                "形容詞" => (false, string.Empty),
                "助動詞" => (false, string.Empty),
                _ => (false, string.Empty)
            };

            //return (false, string.Empty);
        }

        /// <summary>
        /// 動詞の語幹を取得する関数。
        /// </summary>
        /// <param name="token">TokenElementの品詞は動詞であること。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">PartOfSpeechを参照し品詞が動詞以外の場合例外となる</exception>
        public static (bool success, string gokan) ResolveGokanAsVerb(TokenElement token)
        {
            if (token.PartOfSpeech[0] != "動詞")
            {
                throw new ArgumentException(
                    $"Token must be a verb. Actual: {token.ToString()}",
                    nameof(token)
                );
            }

            return token.InflectionType switch
            {
                "カ変・クル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("くれ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("くりゃ")}"),
                    "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("くん")}"),
                    "体言接続特殊２" => (true, $"{token.Surface.RemoveEnd("く")}"),
                    "命令ｉ" => (true, $"{token.Surface.RemoveEnd("こい")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("こよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("くる")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("こよ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("こ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("き")}"),
                    _ => (false, string.Empty)
                },
                "カ変・来ル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("りゃ")}"),
                    "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "体言接続特殊２" => (true, token.Surface),
                    "命令ｉ" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "未然形" => (true, token.Surface),
                    "連用形" => (true, token.Surface),
                    _ => (false, string.Empty)
                },
                "サ変・スル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("すれ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("すりゃ")}"),
                    "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("すん")}"),
                    "体言接続特殊２" => (true, $"{token.Surface.RemoveEnd("す")}"),
                    "命令ｉ" => (true, $"{token.Surface.RemoveEnd("せい")}"),
                    "命令ｒｏ" => (true, $"{token.Surface.RemoveEnd("しろ")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("せよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("する")}"),
                    "文語基本形" => (true, $"{token.Surface.RemoveEnd("す")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("しょ")}"),
                    "未然ヌ接続" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "未然レル接続" => (true, $"{token.Surface.RemoveEnd("さ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("し")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("し")}"),
                    _ => (false, string.Empty)
                },
                "サ変・－スル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("すれ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("すりゃ")}"),
                    "命令ｒｏ" => (true, $"{token.Surface.RemoveEnd("しろ")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("せよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("する")}"),
                    "文語基本形" => (true, $"{token.Surface.RemoveEnd("す")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("しょ")}"),
                    "未然レル接続" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("し")}"),
                    _ => (false, string.Empty)
                },
                "サ変・－ズル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ずれ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("ずりゃ")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("ぜよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ずる")}"),
                    "文語基本形" => (true, $"{token.Surface.RemoveEnd("ず")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("ぜよ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ぜ")}"),
                    _ => (false, string.Empty)
                },
                "ラ変" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("り")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ら")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("り")}"),
                    _ => (false, string.Empty)
                },
                "一段" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("りゃ")}"),
                    "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "命令ｒｏ" => (true, $"{token.Surface.RemoveEnd("ろ")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "未然形" => (true, token.Surface),
                    "連用形" => (true, token.Surface),
                    _ => (false, string.Empty)
                },
                "一段・クレル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れれ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("れりゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "命令ｒｏ" => (true, $"{token.Surface.RemoveEnd("れろ")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("れよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("れる")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("れよ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "未然特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    _ => (false, string.Empty)
                },
                "一段・得ル" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    _ => (false, string.Empty)
                },
                "下二・カ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("くれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("くる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("けよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("く")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    _ => (false, string.Empty)
                },
                "下二・ガ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ぐれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("ぐる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("げよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ぐ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("げ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("げ")}"),
                    _ => (false, string.Empty)
                },
                "下二・ダ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("づれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("づる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("でよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("づ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("で")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("で")}"),
                    _ => (false, string.Empty)
                },
                "下二・ハ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ふれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("ふる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("へよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ふ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("へ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("へ")}"),
                    _ => (false, string.Empty)
                },
                "下二・マ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("むれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("むる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("めよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("む")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("め")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("め")}"),
                    _ => (false, string.Empty)
                },
                "下二・得" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "基本形" => (true, token.Surface),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "未然形" => (true, token.Surface),
                    "連用形" => (true, token.Surface),
                    _ => (false, string.Empty)
                },
                "五段・ガ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("げ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("ぎゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("げ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ぐ")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("ご")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("が")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ぎ")}"),
                    _ => (false, string.Empty)
                },
                "五段・カ行イ音便" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("きゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("く")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("こ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("か")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("き")}"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("きゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("く")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("こ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("か")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("っ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("き")}"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便ユク" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("きゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("け")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("く")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("こ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("か")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("き")}"),
                    _ => (false, string.Empty)
                },
                "五段・サ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("しゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("す")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("そ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("さ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("し")}"),
                    _ => (false, string.Empty)
                },
                "五段・タ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("て")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("ちゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("て")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("つ")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("と")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("た")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("っ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ち")}"),
                    _ => (false, string.Empty)
                },
                "五段・ナ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ね")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("にゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("ね")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ぬ")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("の")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("な")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("に")}"),
                    _ => (false, string.Empty)
                },
                "五段・バ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("べ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("びゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("べ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ぶ")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("ぼ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ば")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("び")}"),
                    _ => (false, string.Empty)
                },
                "五段・マ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("め")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("みゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("め")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("む")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("も")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ま")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("み")}"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("りゃ")}"),
                    "体言接続特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "体言接続特殊２" => (true, token.Surface),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("ろ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ら")}"),
                    "未然特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("っ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("り")}"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行特殊" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "仮定縮約１" => (true, $"{token.Surface.RemoveEnd("りゃ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("れ")}"),
                    "命令ｉ" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("ろ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ら")}"),
                    "未然特殊" => (true, $"{token.Surface.RemoveEnd("ん")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("っ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行ウ音便" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("え")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("え")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("う")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("お")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("わ")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("う")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行促音便" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("え")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("え")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("う")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("お")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("わ")}"),
                    "連用タ接続" => (true, $"{token.Surface.RemoveEnd("っ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    _ => (false, string.Empty)
                },
                "四段・サ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("せ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("す")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("さ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("し")}"),
                    _ => (false, string.Empty)
                },
                "四段・タ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("て")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("て")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("つ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("た")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ち")}"),
                    _ => (false, string.Empty)
                },
                "四段・ハ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("へ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("へ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ふ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("は")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ひ")}"),
                    _ => (false, string.Empty)
                },
                "四段・バ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("べ")}"),
                    "命令ｅ" => (true, $"{token.Surface.RemoveEnd("べ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ぶ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ば")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("び")}"),
                    _ => (false, string.Empty)
                },
                "上二・ダ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ずれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("ずる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("ぢよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("づ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ぢ")}"),
                    "現代基本形" => (true, $"{token.Surface.RemoveEnd("ず")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ぢ")}"),
                    _ => (false, string.Empty)
                },
                "上二・ハ行" => token.InflectionForm switch
                {
                    "仮定形" => (true, $"{token.Surface.RemoveEnd("ふれ")}"),
                    "体言接続" => (true, $"{token.Surface.RemoveEnd("ふる")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("ひよ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("ふ")}"),
                    "未然形" => (true, $"{token.Surface.RemoveEnd("ひ")}"),
                    "連用形" => (true, $"{token.Surface.RemoveEnd("ひ")}"),
                    _ => (false, string.Empty)
                },
                _ => (false, string.Empty)
            };
        }

        /// <summary>
        /// 動詞の活用変換テーブルをエミュレートした動詞用活用変化関数。
        /// NOTE: 活用変換テーブルは「RedPen.NET\docs\常体敬体変換検証\Verb活用ルール.csv」がベースとなる。
        /// 同フォルダ内の「Verb活用ルールC#コード変換.ps1」を実行するとコード断片が生成されるので関数内に貼り付ける。
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static (bool success, string surface) ResolveAsVerb(TokenElement token, string type)
        {
            var (success, gokan) = ResolveGokanAsVerb(token);

            // 語幹が解決できなかった場合は失敗。
            if (!success) { return (false, string.Empty); }

            return token.InflectionType switch
            {
                "カ変・クル" => type switch
                {
                    "仮定形" => (true, $"{gokan}くれ"),
                    "仮定縮約１" => (true, $"{gokan}くりゃ"),
                    "体言接続特殊" => (true, $"{gokan}くん"),
                    "体言接続特殊２" => (true, $"{gokan}く"),
                    "命令ｉ" => (true, $"{gokan}こい"),
                    "命令ｙｏ" => (true, $"{gokan}こよ"),
                    "基本形" => (true, $"{gokan}くる"),
                    "未然ウ接続" => (true, $"{gokan}こよ"),
                    "未然形" => (true, $"{gokan}こ"),
                    "連用形" => (true, $"{gokan}き"),
                    _ => (false, string.Empty)
                },
                "カ変・来ル" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "仮定縮約１" => (true, $"{gokan}りゃ"),
                    "体言接続特殊" => (true, $"{gokan}ん"),
                    "体言接続特殊２" => (true, gokan),
                    "命令ｉ" => (true, $"{gokan}い"),
                    "命令ｙｏ" => (true, $"{gokan}よ"),
                    "基本形" => (true, $"{gokan}る"),
                    "未然ウ接続" => (true, $"{gokan}よ"),
                    "未然形" => (true, gokan),
                    "連用形" => (true, gokan),
                    _ => (false, string.Empty)
                },
                "サ変・スル" => type switch
                {
                    "仮定形" => (true, $"{gokan}すれ"),
                    "仮定縮約１" => (true, $"{gokan}すりゃ"),
                    "体言接続特殊" => (true, $"{gokan}すん"),
                    "体言接続特殊２" => (true, $"{gokan}す"),
                    "命令ｉ" => (true, $"{gokan}せい"),
                    "命令ｒｏ" => (true, $"{gokan}しろ"),
                    "命令ｙｏ" => (true, $"{gokan}せよ"),
                    "基本形" => (true, $"{gokan}する"),
                    "文語基本形" => (true, $"{gokan}す"),
                    "未然ウ接続" => (true, $"{gokan}しょ"),
                    "未然ヌ接続" => (true, $"{gokan}せ"),
                    "未然レル接続" => (true, $"{gokan}さ"),
                    "未然形" => (true, $"{gokan}し"),
                    "連用形" => (true, $"{gokan}し"),
                    _ => (false, string.Empty)
                },
                "サ変・－スル" => type switch
                {
                    "仮定形" => (true, $"{gokan}すれ"),
                    "仮定縮約１" => (true, $"{gokan}すりゃ"),
                    "命令ｒｏ" => (true, $"{gokan}しろ"),
                    "命令ｙｏ" => (true, $"{gokan}せよ"),
                    "基本形" => (true, $"{gokan}する"),
                    "文語基本形" => (true, $"{gokan}す"),
                    "未然ウ接続" => (true, $"{gokan}しょ"),
                    "未然レル接続" => (true, $"{gokan}せ"),
                    "未然形" => (true, $"{gokan}し"),
                    _ => (false, string.Empty)
                },
                "サ変・－ズル" => type switch
                {
                    "仮定形" => (true, $"{gokan}ずれ"),
                    "仮定縮約１" => (true, $"{gokan}ずりゃ"),
                    "命令ｙｏ" => (true, $"{gokan}ぜよ"),
                    "基本形" => (true, $"{gokan}ずる"),
                    "文語基本形" => (true, $"{gokan}ず"),
                    "未然ウ接続" => (true, $"{gokan}ぜよ"),
                    "未然形" => (true, $"{gokan}ぜ"),
                    _ => (false, string.Empty)
                },
                "ラ変" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "体言接続" => (true, $"{gokan}る"),
                    "命令ｅ" => (true, $"{gokan}れ"),
                    "基本形" => (true, $"{gokan}り"),
                    "未然形" => (true, $"{gokan}ら"),
                    "連用形" => (true, $"{gokan}り"),
                    _ => (false, string.Empty)
                },
                "一段" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "仮定縮約１" => (true, $"{gokan}りゃ"),
                    "体言接続特殊" => (true, $"{gokan}ん"),
                    "命令ｒｏ" => (true, $"{gokan}ろ"),
                    "命令ｙｏ" => (true, $"{gokan}よ"),
                    "基本形" => (true, $"{gokan}る"),
                    "未然ウ接続" => (true, $"{gokan}よ"),
                    "未然形" => (true, gokan),
                    "連用形" => (true, gokan),
                    _ => (false, string.Empty)
                },
                "一段・クレル" => type switch
                {
                    "仮定形" => (true, $"{gokan}れれ"),
                    "仮定縮約１" => (true, $"{gokan}れりゃ"),
                    "命令ｅ" => (true, $"{gokan}れ"),
                    "命令ｒｏ" => (true, $"{gokan}れろ"),
                    "命令ｙｏ" => (true, $"{gokan}れよ"),
                    "基本形" => (true, $"{gokan}れる"),
                    "未然ウ接続" => (true, $"{gokan}れよ"),
                    "未然形" => (true, $"{gokan}れ"),
                    "未然特殊" => (true, $"{gokan}ん"),
                    "連用形" => (true, $"{gokan}れ"),
                    _ => (false, string.Empty)
                },
                "一段・得ル" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "基本形" => (true, $"{gokan}る"),
                    _ => (false, string.Empty)
                },
                "下二・カ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}くれ"),
                    "体言接続" => (true, $"{gokan}くる"),
                    "命令ｙｏ" => (true, $"{gokan}けよ"),
                    "基本形" => (true, $"{gokan}く"),
                    "未然形" => (true, $"{gokan}け"),
                    "連用形" => (true, $"{gokan}け"),
                    _ => (false, string.Empty)
                },
                "下二・ガ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}ぐれ"),
                    "体言接続" => (true, $"{gokan}ぐる"),
                    "命令ｙｏ" => (true, $"{gokan}げよ"),
                    "基本形" => (true, $"{gokan}ぐ"),
                    "未然形" => (true, $"{gokan}げ"),
                    "連用形" => (true, $"{gokan}げ"),
                    _ => (false, string.Empty)
                },
                "下二・ダ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}づれ"),
                    "体言接続" => (true, $"{gokan}づる"),
                    "命令ｙｏ" => (true, $"{gokan}でよ"),
                    "基本形" => (true, $"{gokan}づ"),
                    "未然形" => (true, $"{gokan}で"),
                    "連用形" => (true, $"{gokan}で"),
                    _ => (false, string.Empty)
                },
                "下二・ハ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}ふれ"),
                    "体言接続" => (true, $"{gokan}ふる"),
                    "命令ｙｏ" => (true, $"{gokan}へよ"),
                    "基本形" => (true, $"{gokan}ふ"),
                    "未然形" => (true, $"{gokan}へ"),
                    "連用形" => (true, $"{gokan}へ"),
                    _ => (false, string.Empty)
                },
                "下二・マ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}むれ"),
                    "体言接続" => (true, $"{gokan}むる"),
                    "命令ｙｏ" => (true, $"{gokan}めよ"),
                    "基本形" => (true, $"{gokan}む"),
                    "未然形" => (true, $"{gokan}め"),
                    "連用形" => (true, $"{gokan}め"),
                    _ => (false, string.Empty)
                },
                "下二・得" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "体言接続" => (true, $"{gokan}る"),
                    "命令ｙｏ" => (true, $"{gokan}よ"),
                    "基本形" => (true, gokan),
                    "未然ウ接続" => (true, $"{gokan}よ"),
                    "未然形" => (true, gokan),
                    "連用形" => (true, gokan),
                    _ => (false, string.Empty)
                },
                "五段・ガ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}げ"),
                    "仮定縮約１" => (true, $"{gokan}ぎゃ"),
                    "命令ｅ" => (true, $"{gokan}げ"),
                    "基本形" => (true, $"{gokan}ぐ"),
                    "未然ウ接続" => (true, $"{gokan}ご"),
                    "未然形" => (true, $"{gokan}が"),
                    "連用タ接続" => (true, $"{gokan}い"),
                    "連用形" => (true, $"{gokan}ぎ"),
                    _ => (false, string.Empty)
                },
                "五段・カ行イ音便" => type switch
                {
                    "仮定形" => (true, $"{gokan}け"),
                    "仮定縮約１" => (true, $"{gokan}きゃ"),
                    "命令ｅ" => (true, $"{gokan}け"),
                    "基本形" => (true, $"{gokan}く"),
                    "未然ウ接続" => (true, $"{gokan}こ"),
                    "未然形" => (true, $"{gokan}か"),
                    "連用タ接続" => (true, $"{gokan}い"),
                    "連用形" => (true, $"{gokan}き"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便" => type switch
                {
                    "仮定形" => (true, $"{gokan}け"),
                    "仮定縮約１" => (true, $"{gokan}きゃ"),
                    "命令ｅ" => (true, $"{gokan}け"),
                    "基本形" => (true, $"{gokan}く"),
                    "未然ウ接続" => (true, $"{gokan}こ"),
                    "未然形" => (true, $"{gokan}か"),
                    "連用タ接続" => (true, $"{gokan}っ"),
                    "連用形" => (true, $"{gokan}き"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便ユク" => type switch
                {
                    "仮定形" => (true, $"{gokan}け"),
                    "仮定縮約１" => (true, $"{gokan}きゃ"),
                    "命令ｅ" => (true, $"{gokan}け"),
                    "基本形" => (true, $"{gokan}く"),
                    "未然ウ接続" => (true, $"{gokan}こ"),
                    "未然形" => (true, $"{gokan}か"),
                    "連用形" => (true, $"{gokan}き"),
                    _ => (false, string.Empty)
                },
                "五段・サ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}せ"),
                    "仮定縮約１" => (true, $"{gokan}しゃ"),
                    "命令ｅ" => (true, $"{gokan}せ"),
                    "基本形" => (true, $"{gokan}す"),
                    "未然ウ接続" => (true, $"{gokan}そ"),
                    "未然形" => (true, $"{gokan}さ"),
                    "連用形" => (true, $"{gokan}し"),
                    _ => (false, string.Empty)
                },
                "五段・タ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}て"),
                    "仮定縮約１" => (true, $"{gokan}ちゃ"),
                    "命令ｅ" => (true, $"{gokan}て"),
                    "基本形" => (true, $"{gokan}つ"),
                    "未然ウ接続" => (true, $"{gokan}と"),
                    "未然形" => (true, $"{gokan}た"),
                    "連用タ接続" => (true, $"{gokan}っ"),
                    "連用形" => (true, $"{gokan}ち"),
                    _ => (false, string.Empty)
                },
                "五段・ナ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}ね"),
                    "仮定縮約１" => (true, $"{gokan}にゃ"),
                    "命令ｅ" => (true, $"{gokan}ね"),
                    "基本形" => (true, $"{gokan}ぬ"),
                    "未然ウ接続" => (true, $"{gokan}の"),
                    "未然形" => (true, $"{gokan}な"),
                    "連用タ接続" => (true, $"{gokan}ん"),
                    "連用形" => (true, $"{gokan}に"),
                    _ => (false, string.Empty)
                },
                "五段・バ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}べ"),
                    "仮定縮約１" => (true, $"{gokan}びゃ"),
                    "命令ｅ" => (true, $"{gokan}べ"),
                    "基本形" => (true, $"{gokan}ぶ"),
                    "未然ウ接続" => (true, $"{gokan}ぼ"),
                    "未然形" => (true, $"{gokan}ば"),
                    "連用タ接続" => (true, $"{gokan}ん"),
                    "連用形" => (true, $"{gokan}び"),
                    _ => (false, string.Empty)
                },
                "五段・マ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}め"),
                    "仮定縮約１" => (true, $"{gokan}みゃ"),
                    "命令ｅ" => (true, $"{gokan}め"),
                    "基本形" => (true, $"{gokan}む"),
                    "未然ウ接続" => (true, $"{gokan}も"),
                    "未然形" => (true, $"{gokan}ま"),
                    "連用タ接続" => (true, $"{gokan}ん"),
                    "連用形" => (true, $"{gokan}み"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "仮定縮約１" => (true, $"{gokan}りゃ"),
                    "体言接続特殊" => (true, $"{gokan}ん"),
                    "体言接続特殊２" => (true, gokan),
                    "命令ｅ" => (true, $"{gokan}れ"),
                    "基本形" => (true, $"{gokan}る"),
                    "未然ウ接続" => (true, $"{gokan}ろ"),
                    "未然形" => (true, $"{gokan}ら"),
                    "未然特殊" => (true, $"{gokan}ん"),
                    "連用タ接続" => (true, $"{gokan}っ"),
                    "連用形" => (true, $"{gokan}り"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行特殊" => type switch
                {
                    "仮定形" => (true, $"{gokan}れ"),
                    "仮定縮約１" => (true, $"{gokan}りゃ"),
                    "命令ｅ" => (true, $"{gokan}れ"),
                    "命令ｉ" => (true, $"{gokan}い"),
                    "基本形" => (true, $"{gokan}る"),
                    "未然ウ接続" => (true, $"{gokan}ろ"),
                    "未然形" => (true, $"{gokan}ら"),
                    "未然特殊" => (true, $"{gokan}ん"),
                    "連用タ接続" => (true, $"{gokan}っ"),
                    "連用形" => (true, $"{gokan}い"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行ウ音便" => type switch
                {
                    "仮定形" => (true, $"{gokan}え"),
                    "命令ｅ" => (true, $"{gokan}え"),
                    "基本形" => (true, $"{gokan}う"),
                    "未然ウ接続" => (true, $"{gokan}お"),
                    "未然形" => (true, $"{gokan}わ"),
                    "連用タ接続" => (true, $"{gokan}う"),
                    "連用形" => (true, $"{gokan}い"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行促音便" => type switch
                {
                    "仮定形" => (true, $"{gokan}え"),
                    "命令ｅ" => (true, $"{gokan}え"),
                    "基本形" => (true, $"{gokan}う"),
                    "未然ウ接続" => (true, $"{gokan}お"),
                    "未然形" => (true, $"{gokan}わ"),
                    "連用タ接続" => (true, $"{gokan}っ"),
                    "連用形" => (true, $"{gokan}い"),
                    _ => (false, string.Empty)
                },
                "四段・サ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}せ"),
                    "命令ｅ" => (true, $"{gokan}せ"),
                    "基本形" => (true, $"{gokan}す"),
                    "未然形" => (true, $"{gokan}さ"),
                    "連用形" => (true, $"{gokan}し"),
                    _ => (false, string.Empty)
                },
                "四段・タ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}て"),
                    "命令ｅ" => (true, $"{gokan}て"),
                    "基本形" => (true, $"{gokan}つ"),
                    "未然形" => (true, $"{gokan}た"),
                    "連用形" => (true, $"{gokan}ち"),
                    _ => (false, string.Empty)
                },
                "四段・ハ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}へ"),
                    "命令ｅ" => (true, $"{gokan}へ"),
                    "基本形" => (true, $"{gokan}ふ"),
                    "未然形" => (true, $"{gokan}は"),
                    "連用形" => (true, $"{gokan}ひ"),
                    _ => (false, string.Empty)
                },
                "四段・バ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}べ"),
                    "命令ｅ" => (true, $"{gokan}べ"),
                    "基本形" => (true, $"{gokan}ぶ"),
                    "未然形" => (true, $"{gokan}ば"),
                    "連用形" => (true, $"{gokan}び"),
                    _ => (false, string.Empty)
                },
                "上二・ダ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}ずれ"),
                    "体言接続" => (true, $"{gokan}ずる"),
                    "命令ｙｏ" => (true, $"{gokan}ぢよ"),
                    "基本形" => (true, $"{gokan}づ"),
                    "未然形" => (true, $"{gokan}ぢ"),
                    "現代基本形" => (true, $"{gokan}ず"),
                    "連用形" => (true, $"{gokan}ぢ"),
                    _ => (false, string.Empty)
                },
                "上二・ハ行" => type switch
                {
                    "仮定形" => (true, $"{gokan}ふれ"),
                    "体言接続" => (true, $"{gokan}ふる"),
                    "命令ｙｏ" => (true, $"{gokan}ひよ"),
                    "基本形" => (true, $"{gokan}ふ"),
                    "未然形" => (true, $"{gokan}ひ"),
                    "連用形" => (true, $"{gokan}ひ"),
                    _ => (false, string.Empty)
                },
                _ => (false, string.Empty)
            };
        }
    }
}
