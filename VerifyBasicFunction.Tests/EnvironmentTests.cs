using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace VerifyBasicFunction.Tests
{
    /// <summary>環境変数など環境依存のコードを検証するためのテスト。</summary>
    public class EnvironmentTests
    {
        /// <summary>
        /// Environment.GetEnvironmentVariableの動作検証。
        /// </summary>
        [Fact]
        public void 環境変数テスト()
        {
            // MEMO: .NET Standard 2.0と.Net Framework 4.8でEnvironment.GetEnvironmentVariableの挙動が異なる可能性はあるが、
            // 一旦ベタ書きで検証する。
            Environment.GetEnvironmentVariable("ABSOLUTORY_NOT_EXIST_ENVIRONMENT_VARIABLE").Should().BeNull();
            // MEMO: 必ずREDPEN_HOME環境変数を設定済みの環境で実行すること。
            Environment.GetEnvironmentVariable("REDPEN_HOME").Should().NotBeNull();

            Action act;

            act = () => Environment.GetEnvironmentVariable(null);
            act.Should().Throw<ArgumentNullException>();

            act = () => new DirectoryInfo(string.Empty);
            act.Should().Throw<ArgumentException>();

            act = () => new FileInfo(string.Empty);
            act.Should().Throw<ArgumentException>();
        }

        /// <summary>
        /// FileInfoおよびDirectoryInfoの動作検証。
        /// </summary>
        [Fact]
        public void Infoクラスの検証()
        {
            Action act;

            // まずstring.Emptyを引数に渡した場合Exceptionが発生することを確認する。
            act = () => new FileInfo(string.Empty).Should().NotBeNull();
            act.Should().Throw<ArgumentException>();

            act = () => new DirectoryInfo(string.Empty).Should().NotBeNull();
            act.Should().Throw<ArgumentException>();

            // 現在のディレクトリを取得。
            // テストケース実行中なので次の行のようなDebugフォルダのフルパスになる。
            // "C:\~\RedPen.NET\VerifyBasicFunction.Tests\bin\Debug"
            string currentDirectory = Directory.GetCurrentDirectory();

            // DirectoryInfoのコンストラクタに"."を渡した場合、現在のディレクトリを表すインスタンスが生成される。
            new DirectoryInfo(@".\").FullName.Should().Be(currentDirectory + "\\");
            new DirectoryInfo(@".").FullName.Should().Be(currentDirectory);

            // FileInfoのコンストラクタに"."を渡した場合、現在のディレクトリを表すインスタンスが生成される。
            new FileInfo(@".\").FullName.Should().Be(currentDirectory + "\\");
            new FileInfo(@".").FullName.Should().Be(currentDirectory);
        }
    }
}
