using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using RedPen.Net.Core.Config;
using Xunit;

namespace RedPen.Net.Core.Tests.Config
{
    /// <summary>
    /// The configuration tests.
    /// </summary>
    public class ConfigurationTests
    {
        /// <summary>
        /// 単純なValidatorConfigurationのビルドテスト。
        /// </summary>
        [Fact]
        public void SentenceValidatorConfigurationBuildTest()
        {
            Configuration configuration = Configuration.Builder()
                .AddValidatorConfig(new ValidatorConfiguration("SentenceLength"))
                .AddValidatorConfig(new ValidatorConfiguration("InvalidExpression"))
                .AddValidatorConfig(new ValidatorConfiguration("SpaceBeginningOfSentence"))
                .AddValidatorConfig(new ValidatorConfiguration("CommaNumber"))
                .AddValidatorConfig(new ValidatorConfiguration("WordNumber"))
                .AddValidatorConfig(new ValidatorConfiguration("SuggestExpression"))
                .AddValidatorConfig(new ValidatorConfiguration("InvalidCharacter"))
                .AddValidatorConfig(new ValidatorConfiguration("SymbolWithSpace"))
                .AddValidatorConfig(new ValidatorConfiguration("KatakanaEndHyphen"))
                .AddValidatorConfig(new ValidatorConfiguration("KatakanaSpellCheck"))
                .Build();

            configuration.ValidatorConfigs.Count.Should().Be(10);
        }

        /// <summary>
        /// 無効なValidatorConfigurationのビルドテスト。
        /// </summary>
        [Fact]
        public void InvalidValidatorConfigurationTest()
        {
            // JAVA版では以下のコメントがあった。
            // NOTE: not throw a exception even when adding a non exist validator.
            // The errors occurs when creating the added non existing validator instance.
            // しかしテスト内容は例外がスローされないことを確認しており、C＃版でも例外は発生しない。

            Action act1 = () =>
                Configuration.Builder().AddValidatorConfig(new ValidatorConfiguration("ThereIsNoSuchValidator"));
            act1.Should().NotThrow();

            Action act2 = () =>
                Configuration.Builder().AddValidatorConfig(new ValidatorConfiguration("ThereIsNoSuchValidator")).Build();
            act2.Should().NotThrow();
        }

        /// <summary>
        /// Sections the validator configuration test.
        /// MEMO: Secton用のValidatorのテストだが、ValidatorConfigurationをnewして追加するだけなら
        /// 何を追加してもエラーにならないのでテストの意味がない。
        /// </summary>
        [Fact]
        public void SectionValidatorConfigurationTest()
        {
            Configuration configuration = Configuration.Builder()
                .AddValidatorConfig(new ValidatorConfiguration("SectionLength"))
                .AddValidatorConfig(new ValidatorConfiguration("MaxParagraphNumber"))
                .AddValidatorConfig(new ValidatorConfiguration("ParagraphStartWith"))
                .Build();
            configuration.ValidatorConfigs.Count.Should().Be(3);
        }

        /// <summary>
        /// Lang設定を何も与えなかった場合にenがデフォルトとして設定されることを確認するテスト。
        /// </summary>
        [Fact]
        public void DefaultLangTest()
        {
            Configuration configuration = Configuration.Builder()
                .Build(); // NOTE: load "en" setting when lang is not specified
            configuration.Lang.Should().Be("en");
            configuration.Lang.Should().NotBeNull();
        }

        /// <summary>
        /// UniqueKeyのテスト。
        /// </summary>
        [Fact]
        public void UniqueKeyTest()
        {
            // MEMO: new FileInfo("")は例外スローするので暫定的に"C:\\redpen"を利用。
            var confBase = new DirectoryInfo(@"C:\redpen");
            Configuration configuration;

            // ja.hankaku
            configuration = new Configuration(
                confBase,
                new SymbolTable("ja", "hankaku", new List<Symbol>()),
                new List<ValidatorConfiguration>(),
                "ja",
                false);
            configuration.GetKey().Should().Be("ja.hankaku");

            // en
            configuration = new Configuration(
                confBase,
                new SymbolTable("en", null, new List<Symbol>()),
                new List<ValidatorConfiguration>(),
                "en",
                false);
            configuration.GetKey().Should().Be("en");

            // ja.zenkaku
            configuration = new Configuration(
                confBase,
                new SymbolTable("ja", "zenkaku", new List<Symbol>()),
                new List<ValidatorConfiguration>(),
                "ja",
                false);
            configuration.GetKey().Should().Be("ja");
        }

        /// <summary>
        /// REDPEN_HOMEが存在しない場合にHomeディレクトリをworking directoryにセットするかどうかを検証する。
        /// </summary>
        public void HomeIsWorkingDirectoryByDefaultTest()
        {
            // JAVAではJAVACのシステムプロパティをクリアするロジックが存在し、REDPEN_HOMEを削除することで
            // 一時的にREDPEN_HOMEが存在しない場合のデフォルトフォルダパスを取得するテストを実現している。
            //System.clearProperty("REDPEN_HOME");
            //assertEquals(new File(""), Configuration.builder().build().getHome());

            // TODO: C#でデフォルトフォルダパスを検証する方法を検討する。
        }

        /// <summary>
        /// REDPEN_HOMEをHomeディレクトリとして設定することを検証する。
        /// </summary>
        [Fact]
        public void HomeIsResolvedFromSystemPropertyOrEnvironment()
        {
            // JAVAではJAVACのシステムプロパティを上書きするロジックが存在し、REDPEN_HOMEを設定することで
            // REDPEN_HOMEが存在する場合のデフォルトフォルダパスを取得するテストを実現している。
            //System.setProperty("REDPEN_HOME", "/foo");
            //assertEquals(new File("/foo"), Configuration.builder().build().getHome());

            // 一方、C#にはシステムプロパティが無いので、システム環境変数をテストする。
            Environment.GetEnvironmentVariable("REDPEN_HOME").Should().Be(@"C:\redpen");
            Environment.SetEnvironmentVariable("REDPEN_HOME", @"C:\foo");
            Environment.GetEnvironmentVariable("REDPEN_HOME").Should().Be(@"C:\foo");
            // この環境変数の変更は一時的なもので、Windows OSの環境変数設定には影響しない。

            Configuration.Builder().Build().Home.FullName.Should().Be(@"C:\foo");
        }

        /// <summary>
        /// Confファイルを特定するFindFile関数のためのテスト。
        /// TODO: Configurationとファイル管理は切り離した方が良い可能性があり、分離を検討する。
        /// </summary>
        [Fact]
        public void FindFileTest()
        {
            // MEMO: JAVAではFile("")でカレントディレクトリを取得しているが、C#ではDirectory.GetCurrentDirectory()を使用する。
            string currrentDirPath = new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;
            string srcDirPath = new DirectoryInfo("src").FullName;
            string firstLocalFilePath = Directory.GetFiles(Directory.GetCurrentDirectory()).FirstOrDefault();

            // LooksInWorkingDirectoryFirst
            Configuration.Builder().Build().FindFile(firstLocalFilePath).Should().Be(new FileInfo(firstLocalFilePath));

            // LooksInConfigBaseDirectorySecond
            Configuration.Builder().SetBaseDir(new DirectoryInfo("src")).Build().FindFile("main")
                .Should().Be(new FileInfo(Path.Combine("src", "main")));

            // LooksInRedPenHomeDirectoryThird
            Environment.SetEnvironmentVariable("REDPEN_HOME", "src");
            Configuration.Builder().Build().FindFile("main")
                .Should().Be(new FileInfo(Path.Combine("src", "main")));

            // FailsIfFileNotFound
            //Environment.SetEnvironmentVariable("REDPEN_HOME", "src");
            Action action = () => Configuration.Builder().Build().FindFile("hello.xml");
            action.Should().Throw<RedPenException>()
                .WithMessage($"hello.xml is not under working directory ({currrentDirPath}), $REDPEN_HOME ({srcDirPath}).");

            // FailsIfFileNotFound_BasePathPresent
            //Environment.SetEnvironmentVariable("REDPEN_HOME", "src");
            action = () => Configuration.Builder().SetBaseDir(new DirectoryInfo("base_dir")).Build().FindFile("hello.xml");
            action.Should().Throw<RedPenException>()
                .WithMessage($"hello.xml is not under working directory ({currrentDirPath}), base (base_dir), $REDPEN_HOME ({srcDirPath}).");

            // WorkingDirectorySecureMode()
            Environment.SetEnvironmentVariable("REDPEN_HOME", "");
            action = () => Configuration.Builder().SetSecure().Build().FindFile(firstLocalFilePath);
            action.Should().Throw<RedPenException>()
                .WithMessage($"{firstLocalFilePath} is not under $REDPEN_HOME ({currrentDirPath}).");

            // SecureMode
            //Environment.SetEnvironmentVariable("REDPEN_HOME", "");
            action = () => Configuration.Builder().SetSecure().Build().FindFile("/etc/passwd");
            action.Should().Throw<RedPenException>()
                .WithMessage($"/etc/passwd is not under $REDPEN_HOME ({currrentDirPath}).");
        }

        /// <summary>
        /// Cans the be cloned.
        /// TODO: C#のCloneはpublic object Clone()であり、Configurationを返すDeepCopy()で代用した。
        /// 最終的にCloneをDeepCopyへすべて置き換えるか要検討。
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            var conf = Configuration.Builder("ja.hankaku")
                .AddValidatorConfig(new ValidatorConfiguration("SentenceLength")).Build();

            var clone = conf.DeepCopy();
            clone.Should().NotBeSameAs(conf);
            clone.Lang.Should().Be(conf.Lang);
            clone.Variant.Should().Be(conf.Variant);

            clone.ValidatorConfigs.Should().NotBeSameAs(conf.ValidatorConfigs);
            clone.ValidatorConfigs[0].Should().NotBeSameAs(conf.ValidatorConfigs[0]);
            clone.ValidatorConfigs.Should().BeEquivalentTo(conf.ValidatorConfigs);

            clone.SymbolTable.Should().NotBeSameAs(conf.SymbolTable);
            clone.SymbolTable.Should().BeEquivalentTo(conf.SymbolTable);
        }

        /// <summary>
        /// Equals the.
        /// </summary>
        [Fact]
        public void EqualsTest()
        {
            var conf = Configuration.Builder("ja.hankaku")
                .AddValidatorConfig(new ValidatorConfiguration("SentenceLength")).Build();

            var clone = conf.DeepCopy();
            clone.Should().Be(conf);
            clone.GetHashCode().Should().Be(conf.GetHashCode());

            clone.ValidatorConfigs.RemoveAt(0);
            clone.Should().NotBe(conf);

            clone = conf.DeepCopy();
            clone.SymbolTable.UpdateSymbol(new Symbol(SymbolType.AMPERSAND, '^'));
            clone.Should().NotBe(conf);
        }

        /// <summary>
        /// Serializables the.
        /// </summary>
        [Fact]
        public void Serializable()
        {
            var conf = Configuration.Builder("ja.hankaku")
                .AddValidatorConfig(new ValidatorConfiguration("SentenceLength")).Build();

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, conf);
                ms.Seek(0, SeekOrigin.Begin);
                var conf2 = (Configuration)formatter.Deserialize(ms);
                conf2.Should().Be(conf);
                conf2.Tokenizer.GetType().Should().Be(conf.Tokenizer.GetType());
            }
        }

        /// <summary>
        /// Adds the available validators for language.
        /// </summary>
        [Fact]
        public void AddAvailableValidatorsForLanguage()
        {
            var ja = Configuration.Builder("ja").AddAvailableValidatorConfigs().Build();
            ja.ValidatorConfigs.Should().ContainSingle(v => v.ConfigurationName == "SentenceLength");
            ja.ValidatorConfigs.Should().ContainSingle(v => v.ConfigurationName == "HankakuKana");

            var en = Configuration.Builder("en").AddAvailableValidatorConfigs().Build();
            en.ValidatorConfigs.Should().ContainSingle(v => v.ConfigurationName == "SentenceLength");
            en.ValidatorConfigs.Should().NotContain(v => v.ConfigurationName == "HankakuKana");

            var sentenceLength = en.ValidatorConfigs.SingleOrDefault(v => v.ConfigurationName == "SentenceLength");
            sentenceLength.Should().NotBeNull();
            sentenceLength.Properties.Should().ContainValue("120");

            var spelling = en.ValidatorConfigs.SingleOrDefault(v => v.ConfigurationName == "Spelling");
            spelling.Should().NotBeNull();
            spelling.Properties.Should().ContainValue("");
            spelling.Properties.Should().ContainValue("");
        }

        //    @Test
        //    void findFileLooksInWorkingDirectoryFirst() throws Exception {
        //        String firstLocalFilePath = new File(".").list()[0];
        //assertEquals(new File(firstLocalFilePath), Configuration.builder().build().findFile(firstLocalFilePath));
        //    }

        //    @Test
        //    void findFileLooksInConfigBaseDirectorySecond() throws Exception {
        //        assertEquals(new File("src/main"), Configuration.builder().setBaseDir(new File("src")).build().findFile("main"));
        //    }

        //    @Test
        //    void findFileLooksInRedPenHomeDirectoryThird() throws Exception {
        //        System.setProperty("REDPEN_HOME", "src");
        //assertEquals(new File("src/main"), Configuration.builder().build().findFile("main"));
        //    }

        //    @Test
        //    void findFileFailsIfFileNotFound() throws Exception {
        //        try {
        //            System.setProperty("REDPEN_HOME", "src");
        //Configuration.builder().build().findFile("hello.xml");
        //fail("Expecting RedPenException");
        //        }
        //        catch (RedPenException e) {
        //            assertEquals("hello.xml is not under working directory (" + new File("").getAbsoluteFile() + "), $REDPEN_HOME (" + new File("src").getAbsoluteFile() + ").", e.getMessage());
        //        }
        //    }

        //    @Test
        //    void findFileFailsIfFileNotFound_basePathPresent() throws Exception {
        //        try {
        //            System.setProperty("REDPEN_HOME", "src");
        //Configuration.builder().setBaseDir(new File("base_dir")).build().findFile("hello.xml");
        //fail("Expecting RedPenException");
        //        }
        //        catch (RedPenException e) {
        //            assertEquals("hello.xml is not under working directory (" + new File("").getAbsoluteFile() + "), base (base_dir), $REDPEN_HOME (" + new File("src").getAbsoluteFile() + ").", e.getMessage());
        //        }
        //    }

        //    @Test
        //    void findFile_workingDirectorySecureMode() throws Exception {
        //        String firstLocalFilePath = new File(".").list()[0];
        //try
        //{
        //    System.setProperty("REDPEN_HOME", "");
        //    Configuration.builder().secure().build().findFile(firstLocalFilePath);
        //    fail("Secure mode should not allow files from working directory");
        //}
        //catch (RedPenException e)
        //{
        //    assertEquals(firstLocalFilePath + " is not under $REDPEN_HOME (" + new File("").getAbsoluteFile() + ").", e.getMessage());
        //}
        //    }

        //    @Test
        //    void findFile_secureMode() throws Exception {
        //        try {
        //            System.setProperty("REDPEN_HOME", "");
        //Configuration.builder().secure().build().findFile("/etc/passwd");
        //fail("Secure mode should not allow file locations outside config paths");
        //        }
        //        catch (RedPenException e) {
        //            assertEquals("/etc/passwd is not under $REDPEN_HOME (" + new File("").getAbsoluteFile() + ").", e.getMessage());
        //        }
        //    }

        //    @Test
        //    void canBeCloned() throws Exception {
        //        Configuration conf = Configuration.builder("ja.hankaku")
        //          .addValidatorConfig(new ValidatorConfiguration("SentenceLength")).build();

        //Configuration clone = conf.clone();
        //assertNotSame(conf, clone);
        //assertEquals(conf.getLang(), clone.getLang());
        //assertEquals(conf.getVariant(), clone.getVariant());

        //assertNotSame(conf.getValidatorConfigs(), clone.getValidatorConfigs());
        //assertNotSame(conf.getValidatorConfigs().get(0), clone.getValidatorConfigs().get(0));
        //assertEquals(conf.getValidatorConfigs(), clone.getValidatorConfigs());

        //assertNotSame(conf.getSymbolTable(), clone.getSymbolTable());
        //assertEquals(conf.getSymbolTable(), clone.getSymbolTable());
        //    }

        //    @Test
        //    void equals() throws Exception {
        //        Configuration conf = Configuration.builder("ja.hankaku")
        //          .addValidatorConfig(new ValidatorConfiguration("SentenceLength")).build();

        //Configuration clone = conf.clone();
        //assertEquals(conf, clone);
        //assertEquals(conf.hashCode(), clone.hashCode());

        //clone.getValidatorConfigs().remove(0);
        //assertFalse(conf.equals(clone));

        //clone = conf.clone();
        //clone.getSymbolTable().overrideSymbol(new Symbol(AMPERSAND, '^'));
        //assertFalse(conf.equals(clone));
        //    }

        //    @Test
        //    void serializable() throws Exception {
        //        Configuration conf = Configuration.builder("ja.hankaku")
        //          .addValidatorConfig(new ValidatorConfiguration("SentenceLength")).build();

        //ByteArrayOutputStream bytes = new ByteArrayOutputStream();
        //ObjectOutputStream out = new ObjectOutputStream(bytes);
        //        out.writeObject(conf);

        //ObjectInputStream in = new ObjectInputStream(new ByteArrayInputStream(bytes.toByteArray()));
        //Configuration conf2 = (Configuration)in.readObject();

        //assertEquals(conf, conf2);
        //assertEquals(conf.getTokenizer().getClass(), conf2.getTokenizer().getClass());
        //    }

        //    @Test
        //    void addAvailableValidatorsForLanguage() throws Exception {
        //        Configuration ja = Configuration.builder("ja").addAvailableValidatorConfigs().build();
        //assertTrue(ja.getValidatorConfigs().stream().anyMatch(v->v.getConfigurationName().equals("SentenceLength")));
        //assertTrue(ja.getValidatorConfigs().stream().anyMatch(v->v.getConfigurationName().equals("HankakuKana")));

        //Configuration en = Configuration.builder("en").addAvailableValidatorConfigs().build();
        //assertTrue(en.getValidatorConfigs().stream().anyMatch(v->v.getConfigurationName().equals("SentenceLength")));
        //assertFalse(en.getValidatorConfigs().stream().anyMatch(v->v.getConfigurationName().equals("HankakuKana")));

        //ValidatorConfiguration sentenceLength = en.getValidatorConfigs().stream().filter(v->v.getConfigurationName().equals("SentenceLength")).findAny().get();
        //assertEquals("120", sentenceLength.getProperty("max_len"));

        //ValidatorConfiguration spelling = en.getValidatorConfigs().stream().filter(v->v.getConfigurationName().equals("Spelling")).findAny().get();
        //assertEquals("", spelling.getProperty("list"));
        //assertEquals("", spelling.getProperty("dict"));
        //    }
    }
}
