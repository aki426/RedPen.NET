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
    public class Configuration : ICloneable, IEquatable<Configuration>
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

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
        /// <summary>
        /// Gets the tokenizer.
        /// </summary>
        public IRedPenTokenizer Tokenizer { get; private set; }

        // MEMO: JAVA版では環境変数とJVMに渡された引数を使用しているが、C#版では環境変数のみを考慮。
        // TODO: Configurationはデータクラスとしての役割を持つため、環境変数の取得はより浅いレベルの別クラスで行うべき。
        // 実行時に環境変数を取得しHomeフォルダを確定したあと、Configurationに渡すようにする。
        /// <summary>
        /// Gets the home directory.
        /// </summary>
        public FileInfo Home { get; init; } =
            new FileInfo(Environment.GetEnvironmentVariable("REDPEN_HOME") ?? string.Empty);

        /// <summary>
        /// Gets the conf base dir.
        /// </summary>
        public FileInfo ConfBaseDir { get; init; }

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
            FileInfo confBaseDir,
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

            InitTokenizer();
        }

        /// <summary>
        /// Inits the tokenizer.
        /// </summary>
        private void InitTokenizer()
        {
            if (Lang == "ja")
            {
                this.Tokenizer = new NeologdJapaneseTokenizer();
            }
            else
            {
                this.Tokenizer = new WhiteSpaceTokenizer();
            }
        }

        /// <summary>
        /// langとvariantの組み合わせに対する一意のキーを返す。
        /// MEMO: ja & zenkakuの組み合わせの場合は、jaのみを返す。
        /// TODO: UniqueKeyプロパティなどで公開することを検討する。
        /// </summary>
        /// <returns>unique key for this lang and variant combination</returns>
        public string getKey()
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
                if (secureExists(file, this.ConfBaseDir)) { return file; }
            }

            file = new FileInfo(Path.Combine(this.Home.FullName, relativePath));
            if (secureExists(file, this.Home)) { return file; }

            // MEMO: https://docs.oracle.com/javase/jp/6/api/java/io/File.html
            // FileInfo("").getAbsoluteFile()の戻り値は、カレントディレクトリを表すパスになるが、OSにより挙動が異なる。
            // C#では近いものとして、現在の作業ディレクトリを反すSystem.Environment.CurrentDirectoryを採用した。
            // TODO: 実際の挙動としてどのようなものが適当か要検討。
            string currentDirectoryStr = IsSecure ? "" : $"working directory ({System.Environment.CurrentDirectory}), ";
            string baseDirStr = this.ConfBaseDir == null ? "" : $"base ({this.ConfBaseDir}), ";
            // いずれのケースでもファイルが見つからなかったので末尾で例外をスロー。
            throw new RedPenException(
                $"{relativePath} is not under {currentDirectoryStr}{baseDirStr}$REDPEN_HOME ({this.Home.FullName}).");
        }

        private bool secureExists(FileInfo file, FileInfo confBaseDir)
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

        public object Clone()
        {
            return DeepCopy();
        }

        //    /**
        //     * @return a deep copy of this configuration
        //     */

        public Configuration DeepCopy()
        {
            try
            {
                return new Configuration(
                    new FileInfo(this.ConfBaseDir.FullName),
                    this.SymbolTable.Clone() as SymbolTable,
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

        public bool Equals(Configuration? other)
        {
            if (this == other) return true;
            if (other == null) return false;
            if (other is not Configuration) return false;

            return this.Lang == other.Lang &&
                   this.SymbolTable.Equals(other.SymbolTable) &&
                   this.ValidatorConfigs.SequenceEqual(other.ValidatorConfigs);
        }

        public int GetHashCode()
        {
            return HashCode.Combine(Lang, Variant, SymbolTable, ValidatorConfigs);
        }

        public static ConfigurationBuilder Builder()
        {
            return new ConfigurationBuilder();
        }

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
