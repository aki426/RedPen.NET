using System;
using System.Collections.Generic;
using System.IO;
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

        public void HomeIsWorkingDirectoryByDefaultTest()
        {
            // JAVAではJAVACのシステムプロパティをクリアするロジックが存在し、REDPEN_HOMEを削除することで
            // 一時的にREDPEN_HOMEが存在しない場合のデフォルトフォルダパスを取得するテストを実現している。
            //System.clearProperty("REDPEN_HOME");
            //assertEquals(new File(""), Configuration.builder().build().getHome());

            // TODO: C#でデフォルトフォルダパスを検証する方法を検討する。
        }

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

        //    @Test
        //    void findFileLooksInWorkingDirectoryFirst() throws Exception {
        //        String localFile = new File(".").list()[0];
        //assertEquals(new File(localFile), Configuration.builder().build().findFile(localFile));
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
        //        String localFile = new File(".").list()[0];
        //try
        //{
        //    System.setProperty("REDPEN_HOME", "");
        //    Configuration.builder().secure().build().findFile(localFile);
        //    fail("Secure mode should not allow files from working directory");
        //}
        //catch (RedPenException e)
        //{
        //    assertEquals(localFile + " is not under $REDPEN_HOME (" + new File("").getAbsoluteFile() + ").", e.getMessage());
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
