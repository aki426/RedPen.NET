# RedPen.NET

RedPenのC#再実装です。

## 概要

readpenをC#アプリケーションからスマートに利用したかったため、C#で再実装することを目的としています。
基本的な興味の対象が日本語プレーンテキストであるため、その他のフォーマットは後回しにしています。
ライセンス形態は本家Java版やkuromoji、その他利用パッケージに準拠する形を取ります。

## 対象フレームワーク

- .NET Standard 2.0

## ソリューション構成

- RedPen.NET.Core
  - RedPen本体のCoreクラスライブラリ。.NET Standard 2.0対応。
  - Nullable, init, recordを使用するためC# LangVersionを10.0へ変更済み。
  - リソース管理をJAVA版のプロパティテキストファイル方式からResXManagerによるリソース管理へ変更。
- VerifyBasicFunction
  - .NET Standard 2.0フレームワークや利用パッケージの動作確認用のクラスライブラリです。
  - RedPen.NET.Coreと同じ設定、同じ利用パッケージを維持し、フレームワークやパッケージ依存のバグを検出することが目的です。
  - このプロジェクトにプロダクトコードを追加しないでください。

## 利用パッケージ

- xunit
- xunit.runner.visualstudio
- FluentAssertions
- ToString.Fody
- IsExternalInit
- Microsoft.Bcl.HashCode
- System.Collections.Immutable
- PolySharp
- 上記パッケージの依存パッケージ

## ロードマップ

1. RedPen.NET.Coreの実装（redpen-coreのC#再実装）
   1. 日本語のPlainTextフォーマットに対するRedPen基本機能
   2. 日本語を対象とするValidator
   3. json形式のconfファイル入力
2. RedPen.NET.CLIの実装（redpen-cliのC#再実装）
   1. .NETまたは.Net FrameworkでのCLIの実装
3. RedPen.NET.Coreの機能の拡充（redpen-coreの全機能のカバー）
   1. 日本語以外の言語を対象とするValidation
   2. PlainTextフォーマット以外を対象とするValidation
   3. javascriptアドインValidator
4. RedPen.NET.Servereの実装（redpen-serverのC#再実装）

## Validator Configuration

当面優先して実装予定のValidatorのConfigurationです。

| Done |               Name               |                         Description                          | Lang  |            Property            |
| ---- | -------------------------------- | ------------------------------------------------------------ | ----- | ------------------------------ |
| v    | SentenceLength                   | 最大文字長を超えるセンテンス                                 | ANY   | MaxLength                      |
| v    | InvalidExpression                | 不正な表現                                                   | ANY   | DictFile, WordList             |
| v    | CommaNumber                      | センテンス内の最大回数を超えるコンマの使用                   | ANY   | MaxNumber                      |
| v    | SuggestExpression                | 不正な表現に対する推奨表現の提案                             | ANY   | DictFile, WordMap              |
|      | InvalidSymbol                    | 不正なシンボル                                               | ANY   | ※Symbolsブロックで定義         |
|      | SymbolWithSpace                  | シンボル前後のスペースの有無                                 | ANY   | ※Symbolsブロックで定義         |
|      | KatakanaEndHyphen                | JIS Z8301、G.6.2.2 b、G.3.基準のカタカナ単語の語尾のハイフン | ANY   | WordList                       |
|      | KatakanaSpellCheck               | カタカナ単語の表記ゆれ                                       | ja-JP | DictFile, MinRatio, MinFreq    |
|      | SpaceBetweenAlphabeticalWord     | アルファベット単語前後の空白                                 | ja-JP | NoSpace                        |
|      | DoubledWord                      | センテンス内の同一表現の重複使用                             | ANY   | DictFile, WordList             |
|      | SuccessiveWord                   | 同一の単語の連続使用                                         | ANY   |                                |
|      | JapaneseStyle                    | ですます調とである調の混在                                   | ja-JP |                                |
|      | DoubleNegative                   | 二重否定                                                     | ja-JP |                                |
|      | ParenthesizedSentence            | 不正な括弧                                                   | ANY   | MaxLength, MaxNumber, MaxLevel |
|      | DoubledJoshi                     | センテンス内の同一助詞の重複使用                             | ja-JP |                                |
|      | HankakuKana                      | 半角カナ文字                                                 | ja-JP |                                |
|      | Okurigana                        | 不正な送りがな                                               | ja-JP |                                |
|      | LongKanjiChain                   | 最大文字長を超える漢字の連続                                 | ja-JP | Maxlength                      |
|      | JapaneseAmbiguousNounConjunction | 曖昧な名詞接続のパターン（格助詞「の」の連続使用など）       | ja-JP |                                |
|      | JapaneseJoyoKanji                | 常用漢字以外の漢字                                           | ja-JP |                                |
| v    | JapaneseExpressionVariation      | 日本語の表記ゆれ                                             | ja-JP | DictFile, WordMap              |
|      | JapaneseNumberExpression         | 計数表現スタイルの一貫性                                     | ja-JP | NumberStyle                    |
|      | SuccessiveSentence               | 最小文字長以上かつ編集距離閾値以下の類似文の二回連続使用     | ANY   | Distance, MinLength            |
|      | DoubledConjunctiveParticleGa     | センテンス内の接続助詞「が」の2回以上の使用                  | ja-JP |                                |

## Configuration Property

- Name
  - ※すべてのValidator Configurationで必須のプロパティです。
  - 表のName列に記載されている文字列を記載することで、対象ValidatorのConfigurationであると認識されます。
  - このプロパティは先頭に記載する必要があります。それ以外の場合構文エラーになります。
- Level
  - ※すべてのValidator Configurationで必須のプロパティです。
  - ERROR, WARN, INFO, OFFの4種類があります。
  - OFFを指定した場合は当該Validatorは実行されません。
- MaxLength
  - Validatorに対して与える最大長をintで表現したものです。
- MaxNumber
  - Validatorに対して与える最大数をintで表現したものです。（※MaxLengthと型の違いはありませんがConfファイルを人が記述する際の可読性のため分けています）
- DictFile
  - Validatorごとに与える辞書定義ファイルです。実行環境におけるファイルへのパスを指定します。
  - フォーマット：WordListをプロパティに持つ場合は1行1文字列のリスト、WordMapをプロパティに持つ場合は1行につき2つの文字列をタブ区切りで記載します。
- WordMap
  - Validatorごとに与える辞書定義です。
  - JsonのオブジェクトとしてKeyに「検出したい表現」を、Valueに「提案したい表現」を記述します。表記上は誤→正の順番になります。

## License

Apache-2.0 license
