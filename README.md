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
  * 設定ファイルをJsonフォーマット化し、OFFオプションを用意、コメントを許容するなど実際の運用時の効率を優先。
  * [textlint](https://github.com/textlint/textlint)の日本語Validation機能を積極的に取り込むことを優先。
  * 豊富なテストケースにより各Validatorの挙動検証を重視。
* 劣後事項（もしくは、構想）
  * MarkdownやAsciidocなど多種多様なフォーマットのParser（C# Parserライブラリの利用を構想中……）。
  * 本家のredpen-cliやredpen-serverなど単体動作のためのインターフェースに相当するプロジェクトの再実装（Blazorによるサーバサイド実装を構想中……）。
  * 英語など日本語以外の言語のみに対応するValidatorの再実装。
  * Javascriptで記述されたValidatorのアドオン機能（対応しない可能性大）。

## システム情報

### ソリューション構成

* RedPen.NET.Core
  * 本家[RedPen](https://github.com/redpen-cc/redpen)の[redpen-core](https://github.com/redpen-cc/redpen/tree/master/redpen-core)ライブラリに相当します。
  * .NET Standard 2.0、C# 10を採用しています。これは.NETだけでなく.Net Frameworkアプリケーションからの利用も想定しているためです。
  * Immutableで参照透過なFunctionalスタイルで再実装しており、本家からクラス構成が変更されています。
  * リソース管理をJAVA版のプロパティテキストファイル方式からResXManagerによるリソース管理方式へ変更しています。
    * エラーメッセージは日英どちらでも出力できるように構成を変更し、文言を全面的に見直しています。
  * リソースや設定ファイルを内部で取り回すために本家がobject型の多相性を用いていたのに対して、EnumやInterfaceによる明示的な定義を用いています。
  * 文法ルールを考慮したValidationのために本家のExpressionRuleクラスを拡張したGrammerRuleクラスを使用しています。
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
  * ※その他はソリューションのNuGetパッケージマネージャや[NOTICE](https://github.com/aki426/RedPen.NET/blob/main/NOTICE.md)を確認してください。
* RedPen.Net.Core.Tests
  * xunit
  * xunit.runner.visualstudio
  * FluentAssertions

上記パッケージの依存パッケージも利用しています。

## 基本的な使い方

RedPen.NETは大きく設定とValidation実行の2つのフェーズに分かれます。

1. 設定
   1. JsonのConfigurationファイルを読み込むことなどによって、Configurationインスタンスを生成し、RedPenの動作を定義します。
      1. Lang設定はRedPenが検証するドキュメントの言語です。RedPenはその言語向けの設定を読み込み、その言語専用のValidatorになります。
         1. JsonでLangを設定する際は、[System.Globalization.CultureInfo.Name](https://learn.microsoft.com/ja-jp/dotnet/api/system.globalization.cultureinfo.name?view=net-8.0)のフォーマットに従ってください。主には"ja-JP"と"en-US"の2種類です。
      2. 記号（Symbol）の設定などLang設定だけでは不十分な場合、Variantを設定します。
         1. 現在Variantの設定はLangが"ja-JP"の場合のみ有効です。一般的な設定"zenkaku"、理系論文などの記号運用に即した"zenkaku2"、半角記号を使用する"hankaku"などがあります。
      3. ValidatorConfigurations設定は、個々のValidatorの設定を記述します。
         1. 全Validator共通のプロパティとして、NameとLevelがあります。
            1. NameにはValidationNameを記載します。
            2. LevelにはERROR、WARN、INFO、OFFの4つのうちいずれかを記載します。OFFの場合その設定に対応するValidatorは実行されません。
         2. 個別のプロパティは、Validatorごとに設定できる項目が決まっています。後述する一覧表やプロパティの説明を参照してください。
      4. Symbols設定は、LagnとVariantだけでは記号の設定が不十分な場合に設定します。デフォルトの記号設定に対して個別に設定を上書きします。
   2. Configurationに定義された設定でValidator、SymbolTableをロードします。
      1. RedPen.NETはConfigurationを読み込んだ後はその「状態」を維持してドキュメントを読み込み、エラーを返すValidatorの集合体として振る舞います。途中で設定の一部を変更することは想定していませんので、別の設定でValidationしたい場合は別のConfigurationを生成し、RedPenに読み込ませてください。
2. Validation実行
   1. テキストファイルとしてドキュメントを読み込みます。
   2. ドキュメントの言語とフォーマットに合わせてTokenizerとParserを選択し、Documentクラスインスタンスを生成します。
   3. Documentクラスインスタンスに対して、各Validatorを適用し、結果のValidationErrorクラスインスタンスを収集します。
   4. ValidationErrorをそのまま使用したり、ErrorMessageManagerを用いて任意の言語のエラーメッセージへ変換します。

## 動作設定

### Json形式のConfigurationファイル

※作成中※

Jsonファイルのサンプルは[SampleConf.json](https://github.com/aki426/RedPen.NET/blob/main/RedPen.Net.Core.Tests/Config/DATA/SampleConf.json)を参考にしてください。

### Validator Configuration

最新の実装状況は[RedPen.NET_Validator一覧表.csv](https://github.com/aki426/RedPen.NET/blob/main/docs/RedPen.NET_Validator%E4%B8%80%E8%A6%A7%E8%A1%A8.csv)を確認してください。

実装済みの主なValidatorとそのConfigurationは以下のとおりです。Langは対応するドキュメントの言語を表します。

| No. |               Name               |     Lang     |                                                                  Description                                                                  |                      Property                       |
| --- | -------------------------------- | ------------ | --------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| 1   | CommaCount                       | ANY          | センテンス内でMinCount以上カンマが使用されていた場合エラーとなります。                                                                        | MinCount                                            |
| 3   | DoubledConjunctiveParticleGa     | ja-JP        | センテンス内で接続助詞「が」が2回以上使用されていた場合エラーとなります。（例：「Aですが、Bですが、Cです。」といった文が検出対象となります）  |                                                     |
| 4   | DoubledJoshi                     | ja-JP        | センテンス内の同一助詞の重複使用を検出します（例：「一人でで行く」といった文が検出対象となります）。                                          | WordSet, MaxInterval                                |
| 5   | DoubledWord                      | en-US, ja-JP | 同一単語の重複使用を検出します。                                                                                                              | WordSet, MinLength                                  |
| 6   | DoubleNegative                   | en-US, ja-JP | 二重否定表現を検出します。※日本語の場合読みと品詞に基づいて「ズニ～ナイ」「ナイト～マセン」などの表現を検出します。                           |                                                     |
| 11  | HankakuKana                      | ja-JP        | 半角カナ文字を検出します。                                                                                                                    |                                                     |
| 14  | InvalidExpression                | ANY          | 不正な表現を検出します。                                                                                                                      | ExpressionSet                                       |
| 15  | InvalidSymbol                    | ANY          | シンボル定義で指定された不正なシンボルを検出します。                                                                                          | ※Symbolsブロックで定義                              |
| 18  | JapaneseAmbiguousNounConjunction | ja-JP        | 格助詞「の」の連続使用（AのBのC）を曖昧な名詞接続のパターンとして検出します。                                                                 | ExpressionSet                                       |
| 22  | JapaneseWordVariation            | ja-JP        | 読みが同じでありながら表記が異なる単語を、表記ゆれの可能性がある単語として検出します。                                                        | WordMap                                             |
| 28  | JapaneseJoyoKanji                | ja-JP        | 常用漢字以外の漢字を検出します。                                                                                                              | CharSet                                             |
| 30  | JapaneseNumberExpression         | ja-JP        | NumberStyleで指定された計数表現スタイル（「1つ、１つ、一つ、ひとつ」の4つのうちいずれか）以外のスタイルが使用されていた場合エラーとなります。 | NumberStyle                                         |
| 33  | JapaneseStyle                    | ja-JP        | JodoshiStyleで指定されたスタイル（「だ・である調」または「です・ます調」）以外のスタイルが使用されていた場合エラーとなります。                | JodoshiStyle                                        |
| 38  | KatakanaEndHyphen                | ja-JP        | JISZ8301:2008-G.6.2.2b-G.3に基づき、カタカナ単語の語尾にハイフンがあった場合エラーとなります。                                                | ExpressionSet                                       |
| 39  | KatakanaSpellCheck               | ja-JP        | カタカナ単語の表記ゆれを検出します。                                                                                                          | ExpressionSet, MinFreq, MaxRatio, EnableDefaultDict |
| 41  | LongKanjiChain                   | ja-JP        | 文字長がMinLength以上の漢字連続表現（熟語）を検出します。                                                                                     | ExpressionSet, Minlength                            |
| 43  | Okurigana                        | ja-JP        | 不正な送りがなを検出します。                                                                                                                  |                                                     |
| 46  | InvalidParenthesis               | ANY          | 不正な括弧を検出します。                                                                                                                      | MinLength, MinCount, MinLevel                       |
| 49  | SentenceLength                   | ANY          | センテンスの文字長がMinLenght以上の場合エラーとなります。                                                                                     | MinLength                                           |
| 51  | SpaceWithAlphabeticalExpression  | ja-JP        | 日本語文中アルファベット表現の前後の空白が存在しない場合エラーとなります。                                                                    | Forbidden, SkipBefore, SkipAfter                    |
| 54  | SuccessiveSentence               | ANY          | 連続する2つのセンテンスが類似している場合エラーとなります。                                                                                   | MinLength, MaxDistance                              |
| 55  | SuccessiveWord                   | ANY          | 同一の単語の連続使用を検出します。                                                                                                            |                                                     |
| 56  | SuggestExpression                | ANY          | 検出した不正な表現に対して推奨される表現を提案します。                                                                                        | ExpressionMap                                       |
| 57  | SymbolWithSpace                  | en-US        | シンボル定義で指定された「シンボル前後のスペースの有無」に従って、シンボル前後の不正なスペースを検出します。                                  | ※Symbolsブロックで定義                              |
| 58  | Taigendome                       | ja-JP        | 体言止めを検出します。※末尾が体言＝名詞かどうかの判定はKuromojiの判定結果に依存します。                                                       |                                                     |
| 64  | SuggestGrammarRule               | ANY          | 品詞や読みを考慮した文法ルールに合致した表現に対して推奨される表現を提案します。                                                              | GrammarRuleMap                                      |

#### Validator Configuration Property

各ValidatorのConfigurationはJsonファイルで適切なプロパティを指定する必要があります。
プロパティごとの定義と意味は次の一覧表のとおりです。

##### 共通プロパティ

| Property |  Type  |                                        Description                                         |
| -------- | ------ | ------------------------------------------------------------------------------------------ |
| Name     | string | ※すべてのValidator Configurationで必須のプロパティです。                                   |
| Level    | string | ※すべてのValidator Configurationで必須のプロパティです。                                   |
| DictFile | string | 辞書定義ファイルです。※～Setまたは～MapプロパティがあるValidator Configurationで有効です。 |

##### 個別プロパティ

※作成中※

|                Property                |           Type           |                                           Description                                            |
| -------------------------------------- | ------------------------ | ------------------------------------------------------------------------------------------------ |
| MaxDistance / MinDistance              | number                   | 「距離」を浮動小数点数で表現したものです。                                                       |
| EnableDefaultDict                      | true / false             | RedPen.NETの標準辞書がある場合、それを使用する／しないを制御します。                             |
| Forbidden                              | true / false             | Validation条件を反転させて適用可能なものについて、true / falseで反転する／しないを制御できます。 |
| MaxCount / MinCount                    |                          | 最大／最小回数をintで表現したものです。大体の場合、出現回数です。                                |
| MaxLength / MinLength                  | number                   | 最大／最小長をintで表現したものです。大体の場合、文字数です。                                    |
| MaxLevel / MinLevel                    | number                   | 最大／最小レベルをintで表現したものです。大体の場合、章レベルを表します。                        |
| MinFreq                                | number                   | 最小頻度をintで表現したものです。※割合ではなく、出現回数です。                                   |
| MinInterval                            | number                   | 出現する表現の間隔を表現したものです。大体の場合、間に挟まる文字数を表します。                   |
| MinRatio                               | number                   | 最小割合を浮動小数点数で表現したものです。                                                       |
| NoSpace                                | true / false             | trueの場合、スペースを許容しなくなります。                                                       |
| WordMap, ExpressionMap, GrammarRuleMap | <string, string>のobject | それぞれ単語、表現、文法ルールとそれに対応する情報の辞書定義です。                               |
| CharSet, WordSet, ExpressionSet        | stringのarray            | それぞれ文字、単語、表現のリストです。処理速度のため内部実装はHashSetを用いています。            |

##### 補足説明

※作成中※

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
* EnableDefaultDict
  * RedPen.NETの標準辞書がある場合、それを使用するかしないかのフラグです。
  * WordMapやWordSetをプロパティとして持つValidationにおいて、WordMapやWordSetに標準辞書のデータを追加する／しない、という挙動になります。
* WordMap
  * JsonのオブジェクトとしてKeyに「検出したい表現」を、Valueに「提案したい表現」を記述します。表記上は誤→正の順番になります。
* WordSet
  * Validationによって意味は変わりますが、大体の場合許容する表現のリストとして機能します。詳細は各Validationの解説やソースコードを参照してください。

##### パラメータの命名規則

* プロパティ名にMax/Minが付いている場合「Max～」は「その値以下（その値を含む）の場合エラー検出対象とする」、逆に「Min～」は「その値以上（その値を含む）の場合エラー検出対象とする」という意味です。
  * 「Max～」「Min～」プロパティを指定する際は、「ここまではエラーとして検出したい」という最低または最大の基準値を設定するイメージを持つと値を決めやすいです。
* プロパティ名末尾に～Setが付いている場合それは順列を考慮しない単なる集合という意味です。
  * 単なる不正な文字、表現のリスト、一覧というイメージです。
* プロパティ名末尾に～Mapが付いている場合それはある文字列に対して別の文字列を紐づけた（マッピングした）辞書データという意味です。
  * 不正な表現に対する正しい表現の辞書や、ある表現に対する読み仮名の辞書などとして用いられます。

## RedPenの機能

### Levenstein距離

単語やセンテンスの類似性の判定アルゴリズムとしてLevenstein距離（編集距離）を採用しています。
Levenstein距離の解説は<https://en.wikipedia.org/wiki/Levenshtein_distance>などを参照してください。

ReadPen.NETではInsertionCost（挿入コスト）、DeletionCost（削除コスト）、SubstitutionCost（置換コスト）をすべて1としているため、Levenstein距離は「2つの文字列を比較した場合の差分の文字数」とほぼ同じ意味になります。
Levenstein距離を使用するValidatorについては、「何文字まで違っていても類似しているとみなすか」というイメージで値を設定するようにしてください。

## RedPen.NET特有の情報

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
<grammar-rule-text> ::= <grammar-rule> <eol>
<grammar-rule> ::= <token-part> | <token-part> "+" <grammar-rule> | <token-part> "=" <grammar-rule>
<token-part> ::= <surface> | <surface> ":" <tags-part> | <surface> ":" <tags-part> ":" <reading>
<surface> ::= "" | "*" | <text>
<tags-part> ::= <tag> | <tag> "," <tags-part>
<tag> ::= "" | "*"  | <text>
<reading> ::= "" | "*" | <text>
```

* 空白は無視されます。
* "+"記号は、そのあとに書かれたTokenが直前のTokenの直後に連続することを表します。
* "="記号は、そのあとに書かれたTokenが直前のTokenの直後または間に1～複数のTokenを挟んだ後に現れることを表します。
* Tokenの表現については、表層形（Surface）、品詞（Tags、Part of Speech）、読み（Reading）を":"や","で区切って表しますが、空文字、"*"またはそもそも記述しなかった場合はワイルドカードとして扱われいかなるパターンにもマッチします。

#### 文法ルールのパターンマッチ具体例

例えば、「吾輩は猫だが犬でもある。」という文をKuromojiで形態素解析すると次のようになります。※1行につき1Tokenの情報です。

```text
Token1 : Surface = "吾輩", Tags = [ "名詞", "代名詞", "一般", "*", "*" ], Reading = "ワガハイ"
Token2 : Surface = "は", Tags = [ "助詞", "係助詞", "*", "*" ], Reading = "ハ"
Token3 : Surface = "猫", Tags = [ "名詞", "一般", "*", "*" ], Reading = "ネコ"
Token4 : Surface = "だ", Tags = [ "助動詞", "特殊・ダ", "基本形" ], Reading = "ダ"
Token5 : Surface = "が", Tags = [ "助詞", "接続助詞", "*", "*" ], Reading = "ガ"
Token6 : Surface = "犬", Tags = [ "名詞", "一般", "*", "*" ], Reading = "イヌ"
Token7 : Surface = "で", Tags = [ "助詞", "格助詞", "一般", "*", "*" ], Reading = "デ"
Token8 : Surface = "も", Tags = [ "助詞", "係助詞", "*", "*" ], Reading = "モ"
Token9 : Surface = "ある", Tags = [ "動詞", "自立", "五段・ラ行", "基本形" ], Reading = "アル"
Token10 : Surface = "。", Tags = [ "記号", "句点", "*", "*" ], Reading = "。"
```

上記の形態素解析された文に対して、文法ルールとパターンマッチされた具体例を列挙します。

1. 「猫」
   * Token3の「猫」にマッチします。
2. 「猫:*,一般:ネコ」
   * Token3の「猫」にマッチします。
3. 「::ネコ」
   * Token3の「猫」にマッチします。
4. 「猫::イヌ」
   * 何もマッチしません。
5. 「吾輩 + は」
   * Token1とToken2の「吾輩は」にマッチします。
6. 「*:名詞 + :助詞」（※空文字列と"\*"は同じ扱いです）
   * Token1とToken2の「吾輩は」とToken6とToken7の「犬で」にマッチします。
7. 「::ワガハイ + は = ::アル」
   * Token1～9の「吾輩は猫だが犬でもある」にマッチします。
8. 「:名詞 = :名詞 = :名詞」
   * Token1、3、6の「吾輩は猫だが犬」にマッチします。

文法ルールは指定されたパターンを最短一致で判定し、先に一致した部分は飛ばして次に一致する箇所を探索します。
このため、1つの文の中に文字列を共有しないパターンが複数現れる場合、複数回エラーを検出します。
また例えば、6「\*:名詞 + :助詞」の連結記号を変更し「\*:名詞 = :助詞」とした場合も結果は同じです。
先にToken1とToken2の「吾輩は」にマッチするので、Token1とToken8にマッチして「吾輩は猫だが犬でも」を検出することはありません。

品詞や読みとその出現順を検出したい場合、文法ルールによるパターンマッチを活用してください。

## License

ライセンス形態は本家Java版やkuromoji、その他利用パッケージに準拠する形を取ります。
現在は本家のライセンスを継承する意図から「Apache-2.0 license」を採用しています。
