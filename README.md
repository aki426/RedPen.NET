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

##### 共通プロパティ

| Property |  Type  |                                          Description                                           |
| -------- | ------ | ---------------------------------------------------------------------------------------------- |
| Name     | string | ※すべてのValidator Configurationで必須のプロパティです。                                       |
| Level    | string | ※すべてのValidator Configurationで必須のプロパティです。                                       |
| DictFile | string | 辞書定義ファイルです。※WordSetまたはWordMapプロパティがあるValidator Configurationで有効です。 |

##### 個別プロパティ

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

### パターンマッチ

RedPen.NETではValidationのためにパターンマッチ機構を持っています。
パターンマッチの単位としては以下のものがあります。

1. 文字（Char）
   * 文字1文字に相当します。通常Validationでは完全一致で判定します。
   * Validator Configuration Propertyでは「CharSet」などで記述することになります。
2. 単語（Word）
   * 単語1つに相当します。通常Validationでは完全一致で判定します。
   * ここでいう単語とは言語ごとのTokenizerでToken分割されたToken1つ分の表層形のことです。
     * 英語では半角スペースでToken分割されるので、"This is a pen."という文においては"This"、"is"、"a"、"pen"が単語です。
     * 日本語ではKuromojiによる形態素解析によってToken分割されるので、「これは筆です。」という文においては「これ」「は」「筆」「です」が単語です。KuromojiのToken分割の仕方についてはKuromojiまたはIPAdicなどの仕様を参考にしてください（※大まかにはMeCab + IPAdicのToken分割とほぼ一致します）。
   * Validator Configuration Propertyでは「WordSet」などで記述することになります。
3. 表現（Expression）
   * 複数の単語を含む一連の文字列です。通常Validationでは完全一致で判定します。
   * 単語によるパターンマッチだけでは十分でない場合、まとまった表現として指定できる一連の表層形のことです。
     * 英語では、例えば"as soon as"という慣用表現を指定したい場合、それは表現として指定できます。半角スペースを含んだ文字列として"Please email me as soon as possible."などといった文に現れる同様の表現と完全一致するかしないかで判定します。
     * 日本語では、例えば「株式会社ABC」というまとまった表現んを指定したい場合、それは表現として指定できます。複数のTokenを含んだ文字列として「株式会社ABCの今期の売上は3億円でした。」などといった文に現れる同様の表現と完全一致するかしないかで判定します。
   * Validator Configuration Propertyでは「ExpressionSet」などで記述することになります。
4. 文法ルール（GrammarRule）
   * 表層形（Surface）、品詞（Tags、Part of Speech）、読み（Reading）の連続と順序を指定した文法ルールです。RedPen.NETの最も強力なパターンマッチ機構のための形式です。
   * Validationでは文法ルールとして与えられたパターンを、Token分割され、品詞と読みの情報を付与された文に対して適用し詳細なパターンマッチを行います。
   * 文法ルールのBNFは次のとおりです。

#### 文法ルールのBNF

```text
<grammer-rule> ::= <token-part> <eol> | <token-part> "+" <grammer-rule> <eol> | <token-part> "=" <grammer-rule> <eol> 
<token-part> ::= <surface> | <surface> ":" <tags-part> | <surface> ":" <tags-part> ":" <reading>
<surface> ::= "" | "*" | <text>
<tags-part> ::= <tag> | <tag> ":" <tags-part>
<tag> ::= "" | "*" | <text> "," <tag>
<reading> ::= "" | "*" | <text>
```

* 空白は無視されます。
* "+"記号は、そのあとに書かれたTokenが直前のTokenの直後に連続することを表します。
* "="記号は、そのあとに書かれたTokenが直前のTokenの直後または間に1～複数のTokenを挟んだ後に現れることを表します。
* Tokenの表現については、表層形（Surface）、品詞（Tags、Part of Speech）、読み（Reading）を":"や","で区切って表しますが、空文字、"*"またはそもそも記述しなかった場合はワイルドカードとして扱われいかなるパターンにもマッチします。

#### 文法ルールのパターンマッチ具体例

例えば、「吾輩は猫だが犬でもある。」という文をKuromojiで形態素解析すると次のようになります。※1行につき1Tokenの情報です。

```text
1 : Surface = "吾輩", Tags = [ "名詞", "代名詞", "一般", "*", "*" ], Reading = "ワガハイ"
2 : Surface = "は", Tags = [ "助詞", "係助詞", "*", "*" ], Reading = "ハ"
3 : Surface = "猫", Tags = [ "名詞", "一般", "*", "*" ], Reading = "ネコ"
4 : Surface = "だ", Tags = [ "助動詞", "特殊・ダ", "基本形" ], Reading = "ダ"
5 : Surface = "が", Tags = [ "助詞", "接続助詞", "*", "*" ], Reading = "ガ"
6 : Surface = "犬", Tags = [ "名詞", "一般", "*", "*" ], Reading = "イヌ"
7 : Surface = "で", Tags = [ "助詞", "格助詞", "一般", "*", "*" ], Reading = "デ"
8 : Surface = "も", Tags = [ "助詞", "係助詞", "*", "*" ], Reading = "モ"
9 : Surface = "ある", Tags = [ "動詞", "自立", "五段・ラ行", "基本形" ], Reading = "アル"
10 : Surface = "。", Tags = [ "記号", "句点", "*", "*" ], Reading = "。"
```

上記の形態素解析された文に対して、文法ルールとパターンマッチされた具体例を列挙します。

1. 「猫」
   * 3の「猫」にマッチします。
2. 「猫:*,一般:ネコ」
   * 3の「猫」にマッチします。
3. 「::ネコ」
   * 3の「猫」にマッチします。
4. 「猫::イヌ」
   * 何もマッチしません。
5. 「吾輩 + は」
   * 1と2の「吾輩は」にマッチします。
6. 「*:名詞 + :助詞」（※空文字列と"\*"は同じ扱いです）
   * 1と2の「吾輩は」と6と7の「犬で」にマッチします。
7. 「::ワガハイ + は = ::アル」
   * 1～9の「吾輩は猫だが犬でもある」にマッチします。
8. 「:名詞 = :名詞 = :名詞」
   * 1、3、6の「吾輩は猫だが犬」にマッチします。

文法ルールは指定されたパターンを最短一致で判定します。
このため、1つの文の中に2回以上一致するパターンが現れる場合、複数回検出します。
また例えば、6の連結記号を変更し「*:名詞 = :助詞」とした場合も結果は同じです。
1と8にマッチして「吾輩は猫だが犬でも」を検出することはありません。

品詞や読みとその出現順を検出したい場合、文法ルールによるパターンマッチを活用してください。

## License

ライセンス形態は本家Java版やkuromoji、その他利用パッケージに準拠する形を取ります。
現在は本家のライセンスを継承する意図から「Apache-2.0 license」を採用しています。
