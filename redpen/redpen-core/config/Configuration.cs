using redpen_core.tokenizer;

namespace redpen_core.config
{
    public class Configuration : ICloneable
    {
        public SymbolTable SymbolTable { get; init; }

        public List<ValidatorConfiguration> ValidatorConfigs { get; init; } = new List<ValidatorConfiguration>();

        public string Lang { get; init; }

        public string Variant => SymbolTable.Variant;

        // MEMO: JAVA版ではtransientを使っているが、C#版では[NonSerialized]属性や[JsonIgnore]を使用すること。
        // TODO: そもそもどのようなシリアライズが必要なのか、JAVA版の実装を確認してから対応する。
        // 参考: https://qiita.com/NBT/items/9f76c9fd1c7a90506658
        public IRedPenTokenizer Tokenizer { get; private set; }

        // MEMO: JAVA版では環境変数とJVMに渡された引数を使用しているが、C#版では環境変数のみを考慮。
        public FileInfo Home { get; init; } =
            new FileInfo(Environment.GetEnvironmentVariable("REDPEN_HOME") ?? string.Empty);

        public FileInfo ConfBaseDir { get; init; }

        public bool IsSecure { get; init; }

        /**
         * @return default supported languages and variants that can be used with {@link #builder(String)}
         */

        public static List<string> DefaultConfigKeys { get; } =
            new List<string>() { "en", "ja", "ja.hankaku", "ja.zenkaku2", "ru", "ko" };

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

        //        /**
        //         * Finds file relative to either working directory, base directory or $REDPEN_HOME
        //         * @param relativePath of file to find
        //         * @return resolved file if it exists
        //         * @throws RedPenException if file doesn't exist in either place
        //         */

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
                file = new FileInfo(Path.Combine(ConfBaseDir.FullName, relativePath));
                if (secureExists(file, ConfBaseDir)) { return file; }
            }

            file = new File(home, relativePath);
            if (secureExists(file, home)) return file;

            throw new RedPenException(relativePath + " is not under " +
              (!isSecure ? "working directory (" + new File("").getAbsoluteFile() + "), " : "") +
              (base != null ? "base (" + base + "), " : "") +
              "$REDPEN_HOME (" + home.getAbsolutePath() + ").");
        }

        private bool secureExists(FileInfo file, FileInfo confBaseDir)
        {
            try
            {
                return file.Exists && (!IsSecure || file.getCanonicalPath().startsWith(base.getCanonicalPath()));
            }
            catch (IOException e)
            {
                return false;
            }
        }

        //    public boolean isSecure()
        //    {
        //        return isSecure;
        //    }

        //    /**
        //     * @return a deep copy of this configuration
        //     */

        //    @Override public Configuration clone()
        //    {
        //        Configuration clone;
        //        try
        //        {
        //            clone = (Configuration)super.clone();
        //            clone.validatorConfigs = validatorConfigs.stream().map(ValidatorConfiguration::clone).collect(toList());
        //            clone.symbolTable = symbolTable.clone();
        //            return clone;
        //        }
        //        catch (CloneNotSupportedException e)
        //        {
        //            throw new RuntimeException(e);
        //        }
        //    }

        //    private void readObject(ObjectInputStream in) private throws IOException, ClassNotFoundException {
        //    in.private defaultReadObject();

        //    private InitTokenizer();
        //}

        //@Override public boolean equals(Object o)
        //{
        //    if (this == o) return true;
        //    if (!(o instanceof Configuration)) return false;
        //    Configuration that = (Configuration)o;
        //    return Objects.equals(lang, that.lang) &&
        //      Objects.equals(symbolTable, that.symbolTable) &&
        //      Objects.equals(validatorConfigs, that.validatorConfigs);
        //}

        //@Override public int hashCode()
        //{
        //    return getKey().hashCode();
        //}

        //@Override public String toString()
        //{
        //    return "Configuration{" +
        //      "lang='" + lang + '\'' +
        //      ", Tokenizer=" + Tokenizer +
        //      ", validatorConfigs=" + validatorConfigs +
        //      ", symbolTable=" + symbolTable +
        //    '}';
        //}

        //public static ConfigurationBuilder builder()
        //{
        //    return new ConfigurationBuilder();
        //}

        //public static ConfigurationBuilder builder(String key)
        //{
        //    int dotPos = key.indexOf('.');
        //    ConfigurationBuilder builder = new ConfigurationBuilder().setLanguage(dotPos > 0 ? key.substring(0, dotPos) : key);
        //    if (dotPos > 0) builder.setVariant(key.substring(dotPos + 1));
        //    return builder;
        //}

        ///**
        // * Builder class of Configuration.
        // */

        //public static class ConfigurationBuilder
        //{
        //    private final List<ValidatorConfiguration> validatorConfigs = new ArrayList<>();

        //    private final List<Symbol> customSymbols = new ArrayList<>();

        //    private boolean built = false;

        //    private String lang = "en";
        //    private Optional<String> variant = Optional.empty();
        //    private File base;
        //        private boolean isSecure;

        //    private void checkBuilt()
        //    {
        //        if (built) throw new IllegalStateException("Configuration already built.");
        //    }

        //    public ConfigurationBuilder setLanguage(String lang)
        //    {
        //        checkBuilt();
        //        this.lang = lang;
        //        return this;
        //    }

        //    public ConfigurationBuilder setBaseDir(File base)
        //    {
        //        checkBuilt();
        //        this.base = base;
        //        return this;
        //    }

        //    public ConfigurationBuilder addSymbol(Symbol symbol)
        //    {
        //        checkBuilt();
        //        customSymbols.add(symbol);
        //        return this;
        //    }

        //    public ConfigurationBuilder addValidatorConfig(ValidatorConfiguration config)
        //    {
        //        checkBuilt();
        //        validatorConfigs.add(config);
        //        return this;
        //    }

        //    public ConfigurationBuilder addAvailableValidatorConfigs()
        //    {
        //        checkBuilt();
        //        validatorConfigs.addAll(ValidatorFactory.getConfigurations(lang));
        //        return this;
        //    }

        //    public ConfigurationBuilder setVariant(String variant)
        //    {
        //        checkBuilt();
        //        this.variant = Optional.of(variant);
        //        return this;
        //    }

        //    /**
        //     * Enables isSecure mode suitable for servers, where validator properties can come from end-users.
        //     */

        //    public ConfigurationBuilder isSecure()
        //    {
        //        checkBuilt();
        //        isSecure = true;
        //        return this;
        //    }

        //    public Configuration build()
        //    {
        //        checkBuilt();
        //        built = true;
        //        return new Configuration(base, new symbolTable(lang, variant, customSymbols), this.validatorConfigs, this.lang, this.isSecure);
        //    }
        //}
    }
}
