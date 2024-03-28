using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;

namespace RedPen.Net.Core.Tests.Utility
{
    /// <summary>
    /// DictionaryLoader tests.
    /// </summary>
    public class DictionaryLoaderTests
    {
        private readonly DictionaryLoader<HashSet<string>> WORD_LIST = new DictionaryLoader<HashSet<string>>(
            () => new HashSet<string>(),
            (set, line) => set.Add(line));

        private readonly DictionaryLoader<Dictionary<string, string>> KEY_VALUE = new DictionaryLoader<Dictionary<string, string>>(
            () => new Dictionary<string, string>(),
            (dict, line) =>
            {
                string[] parts = line.Split('\t');
                if (parts.Length == 2)
                {
                    dict[parts[0]] = parts[1];
                }
                // MEMO: Validatorにあるプロダクトコードではparts.Lengthが2以外の場合はログ出力している。
            });

        // TODO: Validatorのテストケースでは？
        /// <summary>
        /// Creates the word list test.
        /// </summary>
        [Fact]
        public void CreateWordListTest()
        {
            string sampleWordSet = "Saitama\nGumma\nGifu\n";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                HashSet<string> result = WORD_LIST.Load(stream);
                result.Should().HaveCount(3);
            }
        }

        // TODO: Validatorのテストケースでは？
        /// <summary>
        /// Creates the vacant word list test.
        /// </summary>
        [Fact]
        public void CreateVacantWordListTest()
        {
            string sampleWordSet = "";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                HashSet<string> result = WORD_LIST.Load(stream);
                result.Should().BeEmpty();
            }
        }

        // TODO: Validatorのテストケースでは？
        /// <summary>
        /// Tests the create key value list.
        /// </summary>
        [Fact]
        public void CreateKeyValueListTest()
        {
            string sampleWordSet = "Saitama\t100\nGumma\t530000\nGifu\t1200\n";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                Dictionary<string, string> result = KEY_VALUE.Load(stream);
                result.Should().HaveCount(3);
                result["Saitama"].Should().Be("100");
                result["Gumma"].Should().Be("530000");
                result["Gifu"].Should().Be("1200");
            }
        }

        // TODO: Validatorのテストケースでは？
        /// <summary>
        /// Creates the vacant key value list test.
        /// </summary>
        [Fact]
        public void CreateVacantKeyValueListTest()
        {
            string sampleWordSet = "";
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                Dictionary<string, string> result = KEY_VALUE.Load(stream);
                result.Should().BeEmpty();
            }
        }

        /// <summary>
        /// Loads the cached file test.
        /// </summary>
        [Fact]
        public void LoadCachedFileTest()
        {
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, "foo");

            try
            {
                HashSet<string> strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(1).And.ContainSingle("foo");

                // TODO: キャッシュにHITOしているかどうか判断できないテストケースになっているので要改善。
                // hopefully loaded from cache
                strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(1).And.ContainSingle("foo");

                long lastModified = new FileInfo(tempFilePath).LastWriteTimeUtc.ToFileTimeUtc();

                // ファイルの更新テスト。ただしファイルの更新日は変更しないものとする。
                File.WriteAllText(tempFilePath, "foo\nbar");
                File.SetLastWriteTimeUtc(tempFilePath, DateTime.FromFileTimeUtc(lastModified));

                // won't be reloaded because the last modified date is not changed
                // キャッシュヒットするので古いデータのまま。
                strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(1).And.ContainSingle("foo");

                File.SetLastWriteTimeUtc(tempFilePath, DateTime.FromFileTimeUtc(lastModified + 1000));
                // will be reloaded because the last modified date is changed
                strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(2).And.Contain("foo").And.Contain("bar");
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        /// <summary>
        /// Loadings the inexisting resource returns an empty collection test.
        /// </summary>
        [Fact]
        public void LoadingInexistingResourceReturnsAnEmptyCollectionTest()
        {
            HashSet<string> result = new DictionaryLoader<HashSet<string>>(
                () => new HashSet<string>(),
                null).LoadCachedFromResource("hello.xml", "hello");
            result.Should().BeEmpty();
        }
    }
}
