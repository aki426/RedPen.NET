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
        public static (bool success, string gokan) GetGokanAsVerb(TokenElement token)
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
                    "体言接続特殊２" => (true, $"{token.Surface}"),
                    "命令ｉ" => (true, $"{token.Surface.RemoveEnd("い")}"),
                    "命令ｙｏ" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "基本形" => (true, $"{token.Surface.RemoveEnd("る")}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "未然形" => (true, $"{token.Surface}"),
                    "連用形" => (true, $"{token.Surface}"),
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
                    "未然形" => (true, $"{token.Surface}"),
                    "連用形" => (true, $"{token.Surface}"),
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
                    "基本形" => (true, $"{token.Surface}"),
                    "未然ウ接続" => (true, $"{token.Surface.RemoveEnd("よ")}"),
                    "未然形" => (true, $"{token.Surface}"),
                    "連用形" => (true, $"{token.Surface}"),
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
                    "体言接続特殊２" => (true, $"{token.Surface}"),
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
            return token.InflectionType switch
            {
                "カ変・クル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("くる")}くれ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("くる")}くりゃ"),
                    "体言接続特殊" => (true, $"{token.BaseForm.RemoveEnd("くる")}くん"),
                    "体言接続特殊２" => (true, $"{token.BaseForm.RemoveEnd("くる")}く"),
                    "命令ｉ" => (true, $"{token.BaseForm.RemoveEnd("くる")}こい"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("くる")}こよ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("くる")}こよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("くる")}こ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("くる")}き"),
                    _ => (false, string.Empty)
                },
                "カ変・来ル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("る")}りゃ"),
                    "体言接続特殊" => (true, $"{token.BaseForm.RemoveEnd("る")}ん"),
                    "体言接続特殊２" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    "命令ｉ" => (true, $"{token.BaseForm.RemoveEnd("る")}い"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("る")}よ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}よ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    _ => (false, string.Empty)
                },
                "サ変・スル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("する")}すれ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("する")}すりゃ"),
                    "体言接続特殊" => (true, $"{token.BaseForm.RemoveEnd("する")}すん"),
                    "体言接続特殊２" => (true, $"{token.BaseForm.RemoveEnd("する")}す"),
                    "命令ｉ" => (true, $"{token.BaseForm.RemoveEnd("する")}せい"),
                    "命令ｒｏ" => (true, $"{token.BaseForm.RemoveEnd("する")}しろ"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("する")}せよ"),
                    "文語基本形" => (true, $"{token.BaseForm.RemoveEnd("する")}す"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("する")}しょ"),
                    "未然ヌ接続" => (true, $"{token.BaseForm.RemoveEnd("する")}せ"),
                    "未然レル接続" => (true, $"{token.BaseForm.RemoveEnd("する")}さ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("する")}し"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("する")}し"),
                    _ => (false, string.Empty)
                },
                "サ変・－スル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("する")}すれ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("する")}すりゃ"),
                    "命令ｒｏ" => (true, $"{token.BaseForm.RemoveEnd("する")}しろ"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("する")}せよ"),
                    "文語基本形" => (true, $"{token.BaseForm.RemoveEnd("する")}す"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("する")}しょ"),
                    "未然レル接続" => (true, $"{token.BaseForm.RemoveEnd("する")}せ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("する")}し"),
                    _ => (false, string.Empty)
                },
                "サ変・－ズル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ずれ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ずりゃ"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ぜよ"),
                    "文語基本形" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ず"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ぜよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ずる")}ぜ"),
                    _ => (false, string.Empty)
                },
                "ラ変" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("り")}れ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("り")}る"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("り")}れ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("り")}ら"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("り")}り"),
                    _ => (false, string.Empty)
                },
                "一段" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("る")}りゃ"),
                    "体言接続特殊" => (true, $"{token.BaseForm.RemoveEnd("る")}ん"),
                    "命令ｒｏ" => (true, $"{token.BaseForm.RemoveEnd("る")}ろ"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("る")}よ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}よ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    _ => (false, string.Empty)
                },
                "一段・クレル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("れる")}れれ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("れる")}れりゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("れる")}れ"),
                    "命令ｒｏ" => (true, $"{token.BaseForm.RemoveEnd("れる")}れろ"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("れる")}れよ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("れる")}れよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("れる")}れ"),
                    "未然特殊" => (true, $"{token.BaseForm.RemoveEnd("れる")}ん"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("れる")}れ"),
                    _ => (false, string.Empty)
                },
                "一段・得ル" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    _ => (false, string.Empty)
                },
                "下二・カ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("く")}くれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("く")}くる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("く")}けよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    _ => (false, string.Empty)
                },
                "下二・ガ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}ぐれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}ぐる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}げよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}げ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}げ"),
                    _ => (false, string.Empty)
                },
                "下二・ダ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("づ")}づれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("づ")}づる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("づ")}でよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("づ")}で"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("づ")}で"),
                    _ => (false, string.Empty)
                },
                "下二・ハ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ふれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ふる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("ふ")}へよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}へ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}へ"),
                    _ => (false, string.Empty)
                },
                "下二・マ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("む")}むれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("む")}むる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("む")}めよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("む")}め"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("む")}め"),
                    _ => (false, string.Empty)
                },
                "下二・得" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("")}れ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("")}る"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("")}よ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("")}よ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("")}"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("")}"),
                    _ => (false, string.Empty)
                },
                "五段・ガ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}げ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}ぎゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}げ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}ご"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}が"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}い"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ぐ")}ぎ"),
                    _ => (false, string.Empty)
                },
                "五段・カ行イ音便" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("く")}きゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("く")}こ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("く")}か"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("く")}い"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("く")}き"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("く")}きゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("く")}こ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("く")}か"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("く")}っ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("く")}き"),
                    _ => (false, string.Empty)
                },
                "五段・カ行促音便ユク" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("く")}きゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("く")}け"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("く")}こ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("く")}か"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("く")}き"),
                    _ => (false, string.Empty)
                },
                "五段・サ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("す")}せ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("す")}しゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("す")}せ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("す")}そ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("す")}さ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("す")}し"),
                    _ => (false, string.Empty)
                },
                "五段・タ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("つ")}て"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("つ")}ちゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("つ")}て"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("つ")}と"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("つ")}た"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("つ")}っ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("つ")}ち"),
                    _ => (false, string.Empty)
                },
                "五段・ナ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}ね"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}にゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}ね"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}の"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}な"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}ん"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ぬ")}に"),
                    _ => (false, string.Empty)
                },
                "五段・バ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}べ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}びゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}べ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}ぼ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}ば"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}ん"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}び"),
                    _ => (false, string.Empty)
                },
                "五段・マ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("む")}め"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("む")}みゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("む")}め"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("む")}も"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("む")}ま"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("む")}ん"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("む")}み"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("る")}りゃ"),
                    "体言接続特殊" => (true, $"{token.BaseForm.RemoveEnd("る")}ん"),
                    "体言接続特殊２" => (true, $"{token.BaseForm.RemoveEnd("る")}"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}ろ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("る")}ら"),
                    "未然特殊" => (true, $"{token.BaseForm.RemoveEnd("る")}ん"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}っ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("る")}り"),
                    _ => (false, string.Empty)
                },
                "五段・ラ行特殊" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "仮定縮約１" => (true, $"{token.BaseForm.RemoveEnd("る")}りゃ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("る")}れ"),
                    "命令ｉ" => (true, $"{token.BaseForm.RemoveEnd("る")}い"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}ろ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("る")}ら"),
                    "未然特殊" => (true, $"{token.BaseForm.RemoveEnd("る")}ん"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("る")}っ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("る")}い"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行ウ音便" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}お"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("う")}わ"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}う"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("う")}い"),
                    _ => (false, string.Empty)
                },
                "五段・ワ行促音便" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("う")}え"),
                    "未然ウ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}お"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("う")}わ"),
                    "連用タ接続" => (true, $"{token.BaseForm.RemoveEnd("う")}っ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("う")}い"),
                    _ => (false, string.Empty)
                },
                "四段・サ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("す")}せ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("す")}せ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("す")}さ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("す")}し"),
                    _ => (false, string.Empty)
                },
                "四段・タ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("つ")}て"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("つ")}て"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("つ")}た"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("つ")}ち"),
                    _ => (false, string.Empty)
                },
                "四段・ハ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}へ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("ふ")}へ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}は"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ひ"),
                    _ => (false, string.Empty)
                },
                "四段・バ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}べ"),
                    "命令ｅ" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}べ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}ば"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ぶ")}び"),
                    _ => (false, string.Empty)
                },
                "上二・ダ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("づ")}ずれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("づ")}ずる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("づ")}ぢよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("づ")}ぢ"),
                    "現代基本形" => (true, $"{token.BaseForm.RemoveEnd("づ")}ず"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("づ")}ぢ"),
                    _ => (false, string.Empty)
                },
                "上二・ハ行" => type switch
                {
                    "基本形" => (true, token.BaseForm),
                    "仮定形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ふれ"),
                    "体言接続" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ふる"),
                    "命令ｙｏ" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ひよ"),
                    "未然形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ひ"),
                    "連用形" => (true, $"{token.BaseForm.RemoveEnd("ふ")}ひ"),
                    _ => (false, string.Empty)
                },
                _ => (false, string.Empty)
            };
        }
    }
}
