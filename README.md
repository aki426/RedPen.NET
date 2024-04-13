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

## License

Apache-2.0 license
