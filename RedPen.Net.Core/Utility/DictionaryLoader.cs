using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;

namespace RedPen.Net.Core.Utility
{
    //使用例
    //var loader = new DictionaryLoader<string, string>(
    //    () => new Dictionary<string, string>(),
    //    (map, line) =>
    //    {
    //        var parts = line.Split('=');
    //        map.Add(parts[0], parts[1]);
    //    });
    //
    //var config = loader.Load("config.txt");

    ///// <summary>
    ///// JAVAのDictonaryLoaderの手前実装。
    ///// MEMO: 記述を簡素化できるかもしれないが、JAVA版のコードを踏襲するためにこのような実装としている。
    ///// </summary>
    //public class DictionaryLoader<TKey, TValue>
    //{
    //    private readonly Func<Dictionary<TKey, TValue>> _dictionaryFactory;
    //    private readonly Action<Dictionary<TKey, TValue>, string> _lineProcessor;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="DictionaryLoader"/> class.
    //    /// </summary>
    //    /// <param name="dictionaryFactory">The dictionary factory.</param>
    //    /// <param name="lineProcessor">The line processor.</param>
    //    public DictionaryLoader(
    //        Func<Dictionary<TKey, TValue>> dictionaryFactory,
    //        Action<Dictionary<TKey, TValue>, string> lineProcessor)
    //    {
    //        _dictionaryFactory = dictionaryFactory;
    //        _lineProcessor = lineProcessor;
    //    }

    //    /// <summary>
    //    /// Loads the.
    //    /// </summary>
    //    /// <param name="filename">The filename.</param>
    //    /// <returns>A Dictionary.</returns>
    //    public Dictionary<TKey, TValue> Load(string filename)
    //    {
    //        var dictionary = _dictionaryFactory();
    //        foreach (var line in File.ReadAllLines(filename))
    //        {
    //            _lineProcessor(dictionary, line);
    //        }
    //        return dictionary;
    //    }
    //}

    /// <summary>
    /// load dictionary data from input source.
    /// </summary>

    public class DictionaryLoader<E>
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly Func<E> supplier;
        private readonly Action<E, string> loader;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryLoader"/> class.
        /// </summary>
        /// <param name="supplier">The supplier.</param>
        /// <param name="loader">The loader.</param>
        public DictionaryLoader(Func<E> supplier, Action<E, string> loader)
        {
            this.supplier = supplier;
            this.loader = loader;
        }

        /// <summary>
        /// Given a input stream, load the contents.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <returns>An E.</returns>
        public E Load(Stream inputStream)
        {
            E e = supplier();
            using (StreamReader reader = new StreamReader(inputStream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    loader(e, line);
                }
            }
            return e;
        }

        /// <summary>
        /// Load a given input file
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <returns>An E.</returns>
        private E LoadFromResource(string resourcePath)
        {
            using (Stream inputStream = typeof(DictionaryLoader<E>).Assembly.GetManifestResourceStream(resourcePath))
            {
                if (inputStream == null)
                {
                    throw new IOException($"Failed to load input {resourcePath}");
                }
                return Load(inputStream);
            }
        }

        /// <summary>
        /// Load a given input file
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>An E.</returns>
        private E LoadFromFile(FileInfo file)
        {
            using (FileStream fileStream = file.OpenRead())
            {
                return Load(fileStream);
            }
        }

        private readonly Dictionary<string, E> resourceCache = new Dictionary<string, E>();

        /// <summary>
        /// returns word list loaded from resource
        /// </summary>
        /// <param name="path">resource path</param>
        /// <param name="dictionaryName">name of the resource</param>
        /// <returns>An E.</returns>
        public E LoadCachedFromResource(string path, string dictionaryName)
        {
            if (resourceCache.ContainsKey(path))
            {
                return resourceCache[path];
            }
            else
            {
                try
                {
                    E result = LoadFromResource(path);
                    LOG.Info($"Succeeded to load {dictionaryName}.");
                    return result;
                }
                catch (IOException ex)
                {
                    LOG.Error($"Failed to load {dictionaryName}:{path}: {ex.Message}");
                    return supplier();
                }
            }
        }

        private readonly Dictionary<string, E> fileCache = new Dictionary<string, E>();
        private readonly Dictionary<string, long> fileNameTimestampMap = new Dictionary<string, long>();

        /// <summary>
        /// キャッシュを伴うファイルロード関数。更新チェックは更新日時を見ている。
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="dictionaryName">The dictionary name.</param>
        /// <returns>An E.</returns>
        public E LoadCachedFromFile(FileInfo file, string dictionaryName)
        {
            string path = file.FullName;
            if (!file.Exists)
            {
                throw new RedPenException($"File not found: {file}");
            }

            // ファイルが最後の読み込み以降に変更された場合、
            // つまりファイルとキャッシュの最終更新日時が不一致の場合はキャッシュをクリアする
            long currentModified = file.LastWriteTime.ToFileTimeUtc();
            if (fileNameTimestampMap.TryGetValue(path, out long lastModified))
            {
                if (lastModified != currentModified)
                {
                    fileCache.Remove(path);
                    // JAVA版ではComputeIfPresentを使ってfileCacheとfileNameTimestampMapの両方からpathを削除している。
                    fileNameTimestampMap.Remove(path);
                }
                else
                {
                    // lastModified == currentModifiedの場合、キャッシュは更新しなくてよい。
                    // JAVA版では次の行が実行されるのと同じ挙動だが、C#版では不要なのでコメントアウトした。
                    //fileNameTimestampMap[path] = lastModified;
                }
            }

            // ファイルの読み込みを試み、キャッシュに蓄える。
            E loaded = fileCache.ComputeIfAbsent(path, key =>
            {
                try
                {
                    // pathに対応するキャッシュが存在しない場合、ファイルから読み込む
                    E newlyLoaded = LoadFromFile(file);
                    // キャッシュを両方とも更新する。
                    fileNameTimestampMap[path] = file.LastWriteTime.ToFileTimeUtc();
                    // MEMO: JAVA版はnewlyLoaded == nullの場合例外スローするが、C#の場合Eはnull非許容であるためこれで良いと考える。
                    return newlyLoaded;
                }
                catch (IOException ex)
                {
                    LOG.Error(ex);
                    LOG.Error($"Failed to load {dictionaryName}:{path}");
                    // JAVA版ではreturn nullしているが、C#版ではnullを返すことができないのでthrowする。
                    // MEMO: 結果的にはloaded == nullの場合例外スローしているので同じ挙動になる。

                    throw;
                }
            });

            LOG.Info($"Succeeded to load {dictionaryName}.");
            return loaded;
        }
    }
}
