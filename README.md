# RedPen.NET

redpenのC#再実装です。

## 概要

readpenをC#アプリケーションからスマートに利用したかったため、C#で再実装することを目的としています。
基本的な興味の対象が日本語プレーンテキストであるため、その他のフォーマットは後回しにしています。
ライセンス形態やkuromojiの利用などは本家redpenに準拠する形を取ります。

## 利用パッケージ

- xunit
- xunit.runner.visualstudio
- FluentAssertions
- ToString.Fody
- IsExternalInit
- 上記パッケージの依存パッケージ

## ロードマップ

1. redpen-coreのC#再実装
   1. 日本語かつPlainTextフォーマットに対するredpen基本機能
   2. 日本語を対象とするValidator
   3. javascriptアドインValidator
   4. json形式のconfファイル入力
2. redpen-cliのC#再実装
   1. .NET8でのCLIの実装
3. redpen-coreの全機能
   1. 日本語以外の言語を対象とするValidation
   2. PlainTextフォーマット以外を対象とするValidation
4. redpen-serverのC#再実装

## License

Apache-2.0 license
