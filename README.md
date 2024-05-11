# RedPen.NET

[RedPen](https://github.com/redpen-cc/redpen)のC#再実装です。

## 概要

基本的な開発動機として、日本語の文章校正を行うためのC#アプリケーションから[RedPen](https://github.com/redpen-cc/redpen)を利用することを目的としています。
最終的には本家[RedPen](https://github.com/redpen-cc/redpen)の全機能をサポートすることを目標としていますが、次の独自方針を持っています。

* 優先事項
  * 「日本語」の「プレーンテキスト」のValidator再実装を優先。
  * 他のC#アプリケーションのバックエンドとして運用したいため、本家のredpen-coreプロジェクトの実装を優先。
  * です・ます調とだ・である調の統一など実際の文章校正ニーズに対応するため、既存Validatorの機能改善を優先。
  * 体言止めの検出など実際の文章校正ニーズに対応するため、Validatorの新規追加を優先。
  * 設定ファイルをJsonフォーマット化し、OFFオプションを用意するなど実際の運用時に修正が少なく済むことを優先。
* 劣後事項（もしくは、構想）
  * MarkdownやAsciidocなど多種多様なフォーマットのParse、C# Parserライブラリの利用。
  * 本家のredpen-cliやredpen-serverなど単体動作のためのインターフェースに相当するプロジェクトの再実装。
  * 英語など日本語以外の言語のみに対応するValidatorの再実装。
  * Javascriptで記述されたValidatorのアドオン機能。

## システム情報

### ソリューション構成

* RedPen.NET.Core
  * 本家[RedPen](https://github.com/redpen-cc/redpen)の[redpen-core](https://github.com/redpen-cc/redpen/tree/master/redpen-core)ライブラリに相当します。
  * .NET Standard 2.0、C# 10を採用しています。これは.Net Frameworkアプリケーションからの利用を想定しているためです。
  * Immutableで参照透過なFunctionalスタイルで再実装しており、クラス構成が変更されています。
  * リソース管理をJAVA版のプロパティテキストファイル方式からResXManagerによるリソース管理方式へ変更しています。
  * リソースや設定ファイルを内部で取り回すために本家がobject型の多相性を用いていたのに対して、EnumやInterfaceによる明示的な定義を用いています。
* VerifyBasicFunction
  * .NET Standard 2.0フレームワークや利用パッケージの動作確認用のクラスライブラリです。
  * RedPen.NET.Coreと同じ設定、同じ利用パッケージを維持し、フレームワークやパッケージ依存のバグを検出することが目的です。
  * あくまで開発時の検証用途のため、このプロジェクトにプロダクトコードを追加しないでください。

### 主な利用パッケージ

* RedPen.Net.Core
  * lucene.net.analysis.kuromoji (4.8.0-beta00016)
  * System.Collections.Immutable
  * PolySharp
  * Nlog
  * ※その他はソリューションのNuGetパッケージマネージャやNOTICE.mdを確認してください。
* RedPen.Net.Core.Tests
  * xunit
  * xunit.runner.visualstudio
  * FluentAssertions

上記パッケージの依存パッケージも利用しています。

## 基本的な使い方

※作成中※

## 動作設定

### Json形式のConfigurationファイル

※作成中※

### Validator Configuration

当面優先して実装予定のValidatorとそのConfigurationです。

#### 全言語に適用可能なValidator Configuration

| Done |        Name        |  Target  |                          Description                           |           Property            |
| ---- | ------------------ | -------- | -------------------------------------------------------------- | ----------------------------- |
| v    | CommaNumber        | Sentence | センテンス内の最大回数を超えるコンマの使用を検出               | MaxCount                      |
| v    | DoubledWord        | Sentence | センテンス内の同一表現の重複使用を検出                         | DictFile, WordSet, MinLength  |
| v    | InvalidExpression  | Sentence | 不正な表現を検出                                               | DictFile, WordSet             |
| v    | InvalidSymbol      | Sentence | 不正なシンボルを検出                                           | ※Symbolsブロックで定義        |
| v    | InvalidParenthesis | Sentence | 不正な括弧を検出                                               | MaxLength, MaxCount, MaxLevel |
| v    | SentenceLength     | Sentence | 最大文字長を超えるセンテンスを検出                             | MaxLength                     |
| v    | SuccessiveSentence | Sentence | 最小文字長以上かつ編集距離閾値以下の類似文の二回連続使用を検出 | Distance, MinLength           |
| v    | SuccessiveWord     | Sentence | 同一の単語の連続使用を検出                                     |                               |
| v    | SuggestExpression  | Sentence | 不正な表現に対する推奨表現の提案                               | DictFile, WordMap             |
| v    | SymbolWithSpace    | Sentence | シンボル前後のスペースの有無を検出                             | ※Symbolsブロックで定義        |

#### 日本語（Lang = ja-JP）にのみ適用可能なValidator Configuration

| Done |               Name               |  Target  |                               Description                                |                    Property                    |
| ---- | -------------------------------- | -------- | ------------------------------------------------------------------------ | ---------------------------------------------- |
| v    | DoubledConjunctiveParticleGa     | Sentence | センテンス内の接続助詞「が」の2回以上の使用を検出                        |                                                |
| v    | DoubledJoshi                     | Sentence | センテンス内の同一助詞の重複使用を検出                                   | MinInterval, WordSet                           |
| v    | DoubleNegative                   | Sentence | 二重否定表現を検出                                                       |                                                |
| v    | HankakuKana                      | Sentence | 半角カナ文字を検出                                                       |                                                |
| v    | JapaneseAmbiguousNounConjunction | Sentence | 曖昧な名詞接続のパターン（格助詞「の」の連続使用など）を検出             |                                                |
| v    | JapaneseExpressionVariation      | Document | 日本語の表記ゆれを検出                                                   | DictFile, WordMap                              |
| v    | JapaneseJoyoKanji                | Sentence | 常用漢字以外の漢字を検出                                                 | WordSet                                        |
| v    | JapaneseNumberExpression         | Sentence | 計数表現スタイルの一貫性の破れを検出                                     | NumberStyle                                    |
| v    | JapaneseStyle                    | Sentence | ですます調とである調の混在を検出                                         | JodoshiStyle                                   |
| v    | KatakanaEndHyphen                | Sentence | JIS Z8301:2008 - G.6.2.2 b - G.3基準のカタカナ単語の語尾のハイフンを検出 | WordSet                                        |
| v    | KatakanaSpellCheck               | Document | カタカナ単語の表記ゆれを検出                                             | DictFile, MinRatio, MinFreq, EnableDefaultDict |
| v    | LongKanjiChain                   | Sentence | 最大文字長を超える漢字の連続を検出                                       | Maxlength, WordSet                             |
| v    | Okurigana                        | Sentence | 不正な送りがなを検出                                                     |                                                |
| v    | SpaceWithAlphabeticalExpression  | Sentence | アルファベット単語前後の空白を検出                                       | Forbidden, SkipBefore, SkipAfter               |
| v    | Taigendome                       | Sentence | 体言止めを検出                                                           |                                                |

#### Validator Configuration Property

各ValidatorのConfigurationはJsonファイルで適切なプロパティを指定する必要があります。
プロパティごとの定義と意味は次の一覧表のとおりです。

| Property |  Type  |                                          Description                                           |
| -------- | ------ | ---------------------------------------------------------------------------------------------- |
| Name     | string | ※すべてのValidator Configurationで必須のプロパティです。                                       |
| Level    | string | ※すべてのValidator Configurationで必須のプロパティです。                                       |
| DictFile | string | 辞書定義ファイルです。※WordSetまたはWordMapプロパティがあるValidator Configurationで有効です。 |

|        Property         |           Type           |                                           Description                                            |
| ----------------------- | ------------------------ | ------------------------------------------------------------------------------------------------ |
| DecimalDelimiterIsComma |                          |                                                                                                  |
| DeviationFactor         |                          |                                                                                                  |
| Distance                | number                   | 「距離」を浮動小数点数で表現したものです。                                                       |
| EnableDefaultDict       | true / false             | RedPen.NETの標準辞書がある場合、それを使用する／しないを制御します。                             |
| Forbidden               | true / false             | Validation条件を反転させて適用可能なものについて、true / falseで反転する／しないを制御できます。 |
| IgnoreYear              |                          |                                                                                                  |
| JodoshiStyle            |                          |                                                                                                  |
| LeadingWordLimit        |                          |                                                                                                  |
| MaxCount                |                          |                                                                                                  |
| MaxLength               | number                   | 最大長をintで表現したものです。大体の場合、文字数です。                                          |
| MaxLevel                | number                   | 最大レベルをintで表現したものです。                                                              |
| MaxSentenceCount        | number                   | センテンスの個数の最大値をintで表現したものです。                                                |
| MinFreq                 | number                   | 最小頻度をintで表現したものです。                                                                |
| MinInterval             |                          |                                                                                                  |
| MinLength               | number                   | 最小長をintで表現したものです。大体の場合、文字数です。                                          |
| MinLevel                |                          |                                                                                                  |
| MinRatio                | number                   | 最小割合を浮動小数点数で表現したものです。                                                       |
| NoSpace                 | true / false             | trueの場合、スペースを許容しなくなります。                                                       |
| NumberStyle             |                          |                                                                                                  |
| PercentThreshold        |                          |                                                                                                  |
| SkipAfter               |                          |                                                                                                  |
| SkipBefore              |                          |                                                                                                  |
| StartWith               |                          |                                                                                                  |
| WordMap                 | <string, string>のobject | 辞書定義です。                                                                                   |
| WordSet                 | stringのarray            | 文字列リストです。処理速度のため内部実装はHashSetを用いています。                                |

##### 補足説明

* Name
  * すべてのValidator Configurationで必須のプロパティです。
  * オブジェクト内の先頭に記載する必要があります。
  * ValidationNameを記述します。内部ではValidationType列挙型に変換されます。
* Level
  * すべてのValidator Configurationで必須のプロパティです。
  * ERROR, WARN, INFO, OFFの4種類から1つを選択して記載します。
  * OFFを指定した場合は当該Validatorは実行されません。
* DictFile
  * 実行環境におけるファイルへのパスを指定します。
  * WordSetまたはWordMapの定義をJsonファイルに直接記入するのではなく、別ファイルで渡したい場合に使用します。
  * ファイルのフォーマットは、WordSetをプロパティに持つ場合は1行1文字列のリスト、WordMapをプロパティに持つ場合は1行につき2つの文字列をタブ区切りで記載したDictionaryとなります。
* DecimalDelimiterIsComma
* DeviationFactor
* Distance
* EnableDefaultDict
  * RedPen.NETの標準辞書がある場合、それを使用するかしないかのフラグです。
  * WordMapやWordSetをプロパティとして持つValidationにおいて、WordMapやWordSetに標準辞書のデータを追加する／しない、という挙動になります。
* Forbidden
* IgnoreYear
* JodoshiStyle
* LeadingWordLimit
* MaxCount
* MaxLength
* MaxLevel
* MaxSentenceCount
* MinFreq
* MinInterval
* MinLevel
* MinRatio
* NumberStyle
* PercentThreshold
* SkipAfter
* SkipBefore
* StartWith
* WordMap
  * JsonのオブジェクトとしてKeyに「検出したい表現」を、Valueに「提案したい表現」を記述します。表記上は誤→正の順番になります。
* WordSet
  * Validationによって意味は変わりますが、大体の場合許容する表現のリストとして機能します。詳細は各Validationの解説やソースコードを参照してください。

プロパティ名にMax/Minが付いている者については「Max～」は「その値を含み、その値以下の条件ではエラーにならない」、逆に「Min～」は「その値を含み、その値以上の条件ではエラーにならない」という意味です。
「Max～」「Min～」プロパティを指定する際は「エラーとして検出したくない値とその範囲」を指定するイメージを持つと値を決めやすいです。

## RedPen.Net特有の情報

### 用語

クラス名、Enum名などは以下の用語に準拠しています。

* Validation
  * ある文章エラーを検出するためのルール概念を指します。
* ValidationName
  * Validationの名前です。
  * 例として「SentenceLength」が「SentenceLength」というValidationの名前に該当します。
* ValidatorName
  * Validatorの名前です。ValidationNameに対して接尾辞「Validator」を付けたものです。
  * 例として「SentenceLength」に対する「SentenceLengthValidator」がValidatorNameに該当します。
* ValidatorConfigurationName
  * Validator1つに対するConfigurationの名前です。ValidationNameに対して接尾辞「ValidatorConfiguration」を付けたものです。
  * 例として「SentenceLength」に対する「SentenceLengthValidatorConfiguration」がValidatorConfigurationNameに該当します。

## License

ライセンス形態は本家Java版やkuromoji、その他利用パッケージに準拠する形を取ります。
現在は本家のライセンスを継承する意図から「Apache-2.0 license」を採用しています。
