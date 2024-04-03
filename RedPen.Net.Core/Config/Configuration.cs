using NLog;
using RedPen.Net.Core.Tokenizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedPen.Net.Core.Config
{
    /// <summary>
    /// The configuration.
    /// </summary>
    [ToString]
    [Serializable]
    public class Configuration : ICloneable, IEquatable<Configuration>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>Gets the symbol table.</summary>
        public SymbolTable SymbolTable { get; init; }

        /// <summary>Gets the validator configs.</summary>
        public List<ValidatorConfiguration> ValidatorConfigs { get; init; } = new List<ValidatorConfiguration>();

        /// <summary>Gets the lang.</summary>
        public string Lang { get; init; }

        /// <summary>Gets the variant.</summary>
        public string Variant => SymbolTable.Variant;

        // MEMO: JAVA版ではtransientを使っているが、C#版では[NonSerialized]属性や[JsonIgnore]を使用すること。
        // TODO: そもそもどのようなシリアライズが必要なのか、JAVA版の実装を確認してから対応する。
        // 参考: https://qiita.com/NBT/items/9f76c9fd1c7a90506658
        [NonSerialized] private IRedPenTokenizer _tokenizer;

        /// <summary>
        /// Gets the tokenizer.
        /// </summary>
        public IRedPenTokenizer Tokenizer => this._tokenizer;

        // MEMO: JAVA版では環境変数とJVMに渡された引数を使用しているが、C#版では環境変数のみを考慮。
        // TODO: Configurationはデータクラスとしての役割を持つため、環境変数の取得はより浅いレベルの別クラスで行うべき。
        // 実行時に環境変数を取得しHomeフォルダを確定したあと、Configurationに渡すようにする。
        /// <summary>
        /// Gets the home directory.
        /// </summary>
        public DirectoryInfo Home { get; init; } =
            // MEMO: JAVA版では"REDPEN_HOME"環境変数を利用できなかった場合、空文字列でFileクラスをnewしている。
            // 一方、C#のFile/DirectoryInfoは空文字列を与えるとExceptionになってしまう。
            // そこで代わりに@".\"をセットするようにした。)を使用しているが、C#版では環境変数を使用する。
            new DirectoryInfo(Environment.GetEnvironmentVariable("REDPEN_HOME") ?? @".\");

        /// <summary>
        /// Gets the conf base dir.
        /// </summary>
        public DirectoryInfo ConfBaseDir { get; init; }

        /// <summary>
        /// ディレクトリやフォルダのパス指定を厳密にチェックするかどうかのフラグ。
        /// </summary>
        public bool IsSecure { get; init; }

        /// <summary>
        /// default supported languages and variants that can be used with builder(string)
        /// </summary>
        public static List<string> DefaultConfigKeys { get; } =
            new List<string>() { "en", "ja", "ja.hankaku", "ja.zenkaku2", "ru", "ko" };

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="confBaseDir">The conf base dir.</param>
        /// <param name="symbolTable">The symbol table.</param>
        /// <param name="validatorConfigs">The validator configs.</param>
        /// <param name="lang">The lang.</param>
        /// <param name="isSecure">If true, is secure.</param>
        public Configuration(
            DirectoryInfo confBaseDir,
            SymbolTable symbolTable,
            List<ValidatorConfiguration> validatorConfigs,
            string lang,
            bool isSecure)
        {
            this.ConfBaseDir = confBaseDir;
            this.SymbolTable = symbolTable;

            this.ValidatorConfigs.AddRange(validatorConfigs);
            this.Lang = lang;
            this.IsSecure = isSecure;

            if (Lang == "ja")
            {
                // MEMO: Tokenizerの選択はConfigurationクラスのコンストラクタですでに決定する。
                this._tokenizer = new NeologdJapaneseTokenizer();
            }
            else
            {
                this._tokenizer = new WhiteSpaceTokenizer();
            }

            // InitTokenizer();
        }

        ///// <summary>
        ///// Inits the tokenizer.
        ///// </summary>
        //private void InitTokenizer()
        //{
        //    if (Lang == "ja")
        //    {
        //        this.Tokenizer = new NeologdJapaneseTokenizer();
        //    }
        //    else
        //    {
        //        this.Tokenizer = new WhiteSpaceTokenizer();
        //    }
        //}

        /// <summary>
        /// langとvariantの組み合わせに対する一意のキーを返す。
        /// MEMO: ja & zenkakuの組み合わせの場合は、jaのみを返す。
        /// TODO: UniqueKeyプロパティなどで公開することを検討する。
        /// </summary>
        /// <returns>unique key for this lang and variant combination</returns>
        public string GetKey()
        {
            if (Lang == "ja" && Variant == "zenkaku") { return "ja"; }

            return Lang + (string.IsNullOrEmpty(Variant) ? string.Empty : "." + Variant);
        }

        /// <summary>
        /// Finds file relative to either working directory, base directory or $REDPEN_HOME
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>resolved file if it exists</returns>
        public FileInfo FindFile(string relativePath)
        {
            FileInfo file;

            if (!IsSecure)
            {
                file = new FileInfo(Path.GetFullPath(relativePath));
                if (file.Exists) return file;
            }

            if (ConfBaseDir != null)
            {
                file = new FileInfo(Path.Combine(this.ConfBaseDir.FullName, relativePath));
                if (SecureExists(file, this.ConfBaseDir)) { return file; }
            }

            file = new FileInfo(Path.Combine(this.Home.FullName, relativePath));
            if (SecureExists(file, this.Home)) { return file; }

            // MEMO: https://docs.oracle.com/javase/jp/6/api/java/io/File.html
            // FileInfo("").getAbsoluteFile()の戻り値は、カレントディレクトリを表すパスになるが、OSにより挙動が異なる。
            // C#では近いものとして、現在の作業ディレクトリを返すSystem.Environment.CurrentDirectoryを採用した。
            // TODO: 実際の挙動としてどのようなものが適当か要検討。
            string currentDirectoryStr = IsSecure ? "" : $"working directory ({System.Environment.CurrentDirectory}), ";
            string baseDirStr = this.ConfBaseDir == null ? "" : $"base ({this.ConfBaseDir}), ";
            // いずれのケースでもファイルが見つからなかったので末尾で例外をスロー。
            throw new RedPenException(
                $"{relativePath} is not under {currentDirectoryStr}{baseDirStr}$REDPEN_HOME ({this.Home.FullName}).");
        }

        /// <summary>
        /// ファイルの存在チェックをセキュアに行う関数。
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="confBaseDir">The conf base dir.</param>
        /// <returns>A bool.</returns>
        private bool SecureExists(FileInfo file, DirectoryInfo confBaseDir)
        {
            try
            {
                return file.Exists && (!IsSecure || file.FullName.StartsWith(confBaseDir.FullName));
            }
            catch (IOException ex)
            {
                log.Error(ex); // MEMO: JAVA版からlogの追加。
                return false; // TODO: 例外握りつぶしてfalseで良いのか要確認。
            }
        }

        /// <summary>
        /// Clones the.
        /// </summary>
        /// <returns>An object.</returns>
        public object Clone()
        {
            return DeepCopy();
        }

        /// <summary>
        /// a deep copy of this configuration
        /// </summary>
        /// <returns>A Configuration.</returns>
        public Configuration DeepCopy()
        {
            try
            {
                return new Configuration(
                    new DirectoryInfo(this.ConfBaseDir.FullName),
                    this.SymbolTable.DeepCopy(),
                    this.ValidatorConfigs.Select(v => v.Clone()).ToList(),
                    this.Lang,
                    this.IsSecure);
            }
            catch (Exception e)
            {
                log.Error(e); // MEMO: JAVA版からlogの追加。
                throw;
            }
        }

        // MEMO: JAVA版では以下のメソッドが実装されていたが、使用実績が無いためC#版では実装しない。
        //private void readObject(ObjectInputStream in) private throws IOException, ClassNotFoundException {
        //    in.private defaultReadObject();
        //    private InitTokenizer();
        //}

        /// <summary>
        /// Equals the.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>A bool.</returns>
        public bool Equals(Configuration? other)
        {
            if (this == other) return true;
            if (other == null) return false;
            if (other is not Configuration) return false;

            return this.Lang == other.Lang &&
                   this.SymbolTable.Equals(other.SymbolTable) &&
                   this.ValidatorConfigs.SequenceEqual(other.ValidatorConfigs);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>An int.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Lang, Variant, SymbolTable, ValidatorConfigs);
        }

        /// <summary>
        /// Builders the.
        /// </summary>
        /// <returns>A ConfigurationBuilder.</returns>
        public static ConfigurationBuilder Builder()
        {
            return new ConfigurationBuilder();
        }

        /// <summary>
        /// Builders the.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A ConfigurationBuilder.</returns>
        public static ConfigurationBuilder Builder(string key)
        {
            // 文字列の中から最初のピリオドの位置を取得する。
            int dotPos = key.IndexOf('.');
            ConfigurationBuilder builder = new ConfigurationBuilder().SetLang(dotPos > 0 ? key.Substring(0, dotPos) : key);
            if (dotPos > 0) { builder.SetVariant(key.Substring(dotPos + 1)); }
            return builder;
        }
    }
}
