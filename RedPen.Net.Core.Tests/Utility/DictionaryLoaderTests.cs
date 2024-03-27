using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using RedPen.Net.Core.Utility;
using Xunit;

namespace RedPen.Net.Core.Tests.Utility
{
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
                dict[parts[0]] = parts[1];
            });

        [Fact]
        public void TestCreateWordList()
        {
            string sampleWordSet = "Saitama\nGumma\nGifu\n";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                HashSet<string> result = WORD_LIST.Load(stream);
                result.Should().HaveCount(3);
            }
        }

        [Fact]
        public void TestCreateVacantWordList()
        {
            string sampleWordSet = "";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                HashSet<string> result = WORD_LIST.Load(stream);
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public void TestCreateKeyValueList()
        {
            string sampleWordSet = "Saitama\t100\nGumma\t530000\nGifu\t1200\n";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                Dictionary<string, string> result = KEY_VALUE.Load(stream);
                result.Should().HaveCount(3);
                result["saitama"].Should().Be("100");
                result["gumma"].Should().Be("530000");
                result["gifu"].Should().Be("1200");
            }
        }

        [Fact]
        public void TestCreateVacantKeyValueList()
        {
            string sampleWordSet = "";
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sampleWordSet)))
            {
                Dictionary<string, string> result = KEY_VALUE.Load(stream);
                result.Should().BeEmpty();
            }
        }

        [Fact]
        public void TestLoadCachedFile()
        {
            string tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, "foo");

            try
            {
                HashSet<string> strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(1).And.ContainSingle("foo");

                // hopefully loaded from cache
                strings = WORD_LIST.LoadCachedFromFile(new FileInfo(tempFilePath), "temp file");
                strings.Should().HaveCount(1).And.ContainSingle("foo");

                long lastModified = new FileInfo(tempFilePath).LastWriteTimeUtc.ToFileTimeUtc();

                File.WriteAllText(tempFilePath, "foo\nbar");
                File.SetLastWriteTimeUtc(tempFilePath, DateTime.FromFileTimeUtc(lastModified));
                // won't be reloaded because the last modified date is not changed
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

        [Fact]
        public void TestLoadingInexistingResourceReturnsAnEmptyCollection()
        {
            HashSet<string> result = new DictionaryLoader<HashSet<string>>(
                () => new HashSet<string>(),
                null).LoadCachedFromResource("hello.xml", "hello");
            result.Should().BeEmpty();
        }
    }
}
