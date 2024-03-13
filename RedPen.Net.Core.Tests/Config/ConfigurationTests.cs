using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedPen.Net.Core.Config;
using RedPen.Net.Core;
using Xunit;
using FluentAssertions;

namespace RedPen.Net.Core.Tests.Config
{
    public class ConfigurationTests
    {
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

        //        @Test
        //    void testSentenceValidatorConfiguration() throws Exception
        //        {
        //    }

        //    @Test
        //    void testInvalidValidatorConfiguration()
        //    {
        //        // NOTE: not throw a exception even when adding a non exist validator.
        //        // The errors occurs when creating the added non existing validator instance.
        //        try
        //        {
        //            Configuration.builder()
        //                    .addValidatorConfig(new ValidatorConfiguration("ThereIsNoSuchValidator")).build();
        //        }
        //        catch (Exception e)
        //        {
        //            fail("Exception not expected.");
        //        }
        //    }

        //    @Test
        //    void testSectionValidatorConfiguration() throws Exception
        //    {
        //        Configuration configuration = Configuration.builder().addValidatorConfig(new ValidatorConfiguration("SectionLength"))
        //                .addValidatorConfig(new ValidatorConfiguration("MaxParagraphNumber"))
        //                .addValidatorConfig(new ValidatorConfiguration("ParagraphStartWith")).build();
        //    assertEquals(3, configuration.getValidatorConfigs().size());
        //}

        //@Test
        //    void testSymbolTableWithoutLanguageSetting() throws Exception {
        //        Configuration configuration = Configuration.builder().build(); // NOTE: load "en" setting when lang is not specified
        //assertEquals("en", configuration.getLang());
        //assertNotNull(configuration.getLang());
        //    }

        //    @Test
        //    void keyIsLangAndType() throws Exception {
        //        SymbolTable symbolTable = new SymbolTable("ja", Optional.of("hankaku"), emptyList());
        //assertEquals("ja.hankaku", new Configuration(new File(""), symbolTable, emptyList(), "ja", false).getKey());
        //    }

        //    @Test
        //    void keyIsLangOnlyIfTypeIsMissing() throws Exception {
        //        SymbolTable symbolTable = new SymbolTable("en", Optional.empty(), emptyList());
        //assertEquals("en", new Configuration(new File(""), symbolTable, emptyList(), "en", false).getKey());
        //    }

        //    @Test
        //    void keyIsLangOnlyForZenkaku() throws Exception {
        //        SymbolTable symbolTable = new SymbolTable("ja", Optional.of("zenkaku"), emptyList());
        //assertEquals("ja", new Configuration(new File(""), symbolTable, emptyList(), "ja", false).getKey());
        //    }

        //    @Test
        //    void homeIsWorkingDirectoryByDefault() throws Exception {
        //        System.clearProperty("REDPEN_HOME");
        //assertEquals(new File(""), Configuration.builder().build().getHome());
        //    }

        //    @Test
        //    void homeIsResolvedFromSystemPropertyOrEnvironment() throws Exception {
        //        System.setProperty("REDPEN_HOME", "/foo");
        //assertEquals(new File("/foo"), Configuration.builder().build().getHome());
        //    }

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
