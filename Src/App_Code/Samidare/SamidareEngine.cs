using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

using Samidare.Model;

namespace Samidare
{
    /// <summary>
    /// エントリーやサイトの状態を保持して処理するためのクラスです。
    /// このクラスはメモリ上に保持され、再利用されます。
    /// </summary>
    public class SamidareEngine
    {
        private static ConcurrentDictionary<String, SamidareEngine> _engines = new ConcurrentDictionary<String, SamidareEngine>(StringComparer.OrdinalIgnoreCase);

        private IDictionary<String, ILookup<String, Entry>> _entryIndexes;
        private Entry[] _entries;

        /// <summary>
        /// データのルートディレクトリを取得します。
        /// </summary>
        public String RootDirectory { get; private set; }

        /// <summary>
        /// エントリを表示するテンプレートを決定する処理を取得、設定します。
        /// </summary>
        public IDictionary<String, Func<IEnumerable<Entry>, String>> CustomTemplates { get; private set; }
        /// <summary>
        /// エントリをフォーマットするためのフォーマッターを登録するを取得、設定します。
        /// </summary>
        public IDictionary<String, IEntryFormatter> Formatters { get; private set; }
        /// <summary>
        /// エントリーやページを列挙するクローラーを登録するディクショナリを取得します。
        /// </summary>
        public IDictionary<String, Func<SamidareEngine, IEnumerable<Entry>>> Crawlers { get; private set; }
        /// <summary>
        /// インデックスを作るためのインデックサーを登録するディクショナリを取得します。
        /// </summary>
        public IDictionary<String, Func<IEnumerable<Entry>, ILookup<String, Entry>>> Indexers { get; private set; }
        /// <summary>
        /// メタデータを変換するためのコンバーターを登録するディクショナリを取得します。
        /// </summary>
        public IDictionary<String, Func<String, dynamic>> MetadataConverters { get; private set; }
        /// <summary>
        /// 読み込んだ後の処理を登録するディクショナリを取得します。
        /// </summary>
        public IDictionary<String, Func<IEnumerable<Entry>, IEnumerable<Entry>>> PostProcessors { get; private set; }
        /// <summary>
        /// ルーティングを登録するディクショナリを取得します。
        /// </summary>
        public IDictionary<String, Func<Match, SamidareEngine, RouteDispatchResult>> Routes { get; private set; }
        /// <summary>
        /// ルーティングに従ってディスパッチする処理を取得します。
        /// </summary>
        public Func<String, RouteDispatchResult> Dispatcher { get; private set; }

        /// <summary>
        /// インデックス化されたエントリー情報を取得します。
        /// </summary>
        public IDictionary<String, ILookup<String, Entry>> EntryIndexes
        {
            get { return _entryIndexes; }
        }

        /// <summary>
        /// すべてのエントリーを取得します。
        /// </summary>
        public Entry[] Entries
        {
            get { return _entries; }
        }

        /// <summary>
        /// キャッシュをセットするための処理を設定します。
        /// 第一引数はデータディレクトリ(キャッシュのキー)、第二引数は開始されたメインページのパス
        /// </summary>
        public static Action<String, String> CacheSetStrategy { get; set; }
        /// <summary>
        /// キャッシュが切れているかどうかを取得する処理を設定します。
        /// </summary>
        public static Func<String, Boolean> CacheExpiredStrategy { get; set; }

        /// <summary>
        /// 指定したディレクトリに対するエンジンを取得します。
        /// </summary>
        /// <param name="entryPointPagePath"></param>
        /// <param name="rootDirectory"></param>
        /// <param name="engineCreated"></param>
        /// <returns></returns>
        public static SamidareEngine GetInstance(String entryPointPagePath, String rootDirectory, Action<SamidareEngine> engineCreated, Boolean disableCache)
        {
            // キャッシュ切れてたら作りなおす
            if (CacheExpiredStrategy == null || CacheExpiredStrategy(rootDirectory) || disableCache)
            {
                SamidareEngine oldEngine;
                _engines.TryRemove(rootDirectory, out oldEngine);
            }

            return _engines.GetOrAdd(rootDirectory, _ =>
                                                          {
                                                              var engine = new SamidareEngine(rootDirectory);
                                                              if (engineCreated != null)
                                                              {
                                                                  engineCreated(engine);
                                                              }
                                                              
                                                              engine.Initialize();

                                                              if (CacheSetStrategy != null)
                                                              {
                                                                  CacheSetStrategy(rootDirectory, entryPointPagePath);
                                                              }
                                                              return engine;
                                                          });
        }

        static SamidareEngine()
        {
            CacheSetStrategy = (rootDirectory, entryPointPagePath) => HttpContext.Current.Cache.Add(
                "Samidare.EntriesCache:" + rootDirectory,
                new Object(),
                new System.Web.Caching.CacheDependency(
                    Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories)
                    .Union(new[] { rootDirectory, entryPointPagePath })
                    .ToArray()
                ),
                DateTime.Now.Add(TimeSpan.FromMinutes(60)),
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Low,
                null
            );
            CacheExpiredStrategy = (rootDirectory) => HttpContext.Current.Cache.Get("Samidare.EntriesCache:" + rootDirectory) == null;
        }

        public SamidareEngine(String rootDirectory)
        {
            _entryIndexes = new Dictionary<String, ILookup<String, Entry>>(StringComparer.OrdinalIgnoreCase);
            _entries = new Entry[0];

            RootDirectory = rootDirectory;

            Crawlers = new Dictionary<String, Func<SamidareEngine, IEnumerable<Entry>>>(StringComparer.OrdinalIgnoreCase);
            Indexers = new Dictionary<String, Func<IEnumerable<Entry>, ILookup<String, Entry>>>(StringComparer.OrdinalIgnoreCase);
            MetadataConverters = new Dictionary<String, Func<String, dynamic>>(StringComparer.OrdinalIgnoreCase);
            PostProcessors = new Dictionary<String, Func<IEnumerable<Entry>, IEnumerable<Entry>>>(StringComparer.OrdinalIgnoreCase);
            Formatters = new Dictionary<String, IEntryFormatter>(StringComparer.OrdinalIgnoreCase);
            CustomTemplates = new Dictionary<String, Func<IEnumerable<Entry>, String>>(StringComparer.OrdinalIgnoreCase);
            Routes = new Dictionary<String, Func<Match, SamidareEngine, RouteDispatchResult>>(StringComparer.OrdinalIgnoreCase);

            Builtin.MetadataConverters.Register(this);
            Builtin.Indexers.Register(this);
            Builtin.PostProcessors.Register(this);
            Builtin.Formatters.Register(this);
            Builtin.Crawlers.Register(this);
            Builtin.Routes.Register(this);
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Initialize()
        {
            UpdateRoutes();
            CrawlAllEntries();
            UpdateIndexes();
        }

        /// <summary>
        /// すべてのエントリーを読み込みます。
        /// </summary>
        public void CrawlAllEntries()
        {
            _entryIndexes.Clear();
            _entries = ExecutePostProcess(
                Crawlers.SelectMany(x => x.Value(this))
            ).ToArray();
        }

        /// <summary>
        /// インデックスの更新
        /// </summary>
        private void UpdateIndexes()
        {
            foreach (var indexer in Indexers)
            {
                _entryIndexes[indexer.Key] = indexer.Value(_entries);
            }
        }

        /// <summary>
        /// ルーティングの更新
        /// </summary>
        private void UpdateRoutes()
        {
            var routeRegexArray = Routes.Select(x => new { Regex = new Regex("^/" + x.Key + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled), Func = x.Value }).ToArray();

            Dispatcher = (path) =>
                         {
                             var matchedRoute = routeRegexArray
                                 .Select(x => new {Match = x.Regex.Match(path), Func = x.Func})
                                 .FirstOrDefault(x => x.Match.Success);

                             if (matchedRoute != null)
                             {
                                 return matchedRoute.Func(matchedRoute.Match, this);
                             }
                             else
                             {
                                 return null;
                             }
                         };
        }

        /// <summary>
        /// 読み込み後のページの処理。オーダーなどもここ。
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        private IEnumerable<Entry> ExecutePostProcess(IEnumerable<Entry> entries)
        {
            foreach (var postprocess in PostProcessors.Where(x => !new[] { "Order" }.Contains(x.Key)))
            {
                entries = postprocess.Value(entries);
            }

            entries = PostProcessors["Order"](entries);

            return entries;
        }

        /// <summary>
        /// エントリーを生成します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Entry CreateEntry(String baseDirectory, String path)
        {
            var entry = new Entry(baseDirectory, path);
            entry.ParseMetadata(this.MetadataConverters);

            Formatters[Path.GetExtension(path)].Process(entry);

            return entry;
        }

        /// <summary>
        /// 指定したディレクトリをベースディレクトリとして、そのディレクトリ以下に存在するエントリーを読み込みます。
        /// </summary>
        /// <param name="baseDir"></param>
        /// <returns></returns>
        public IEnumerable<Entry> LoadEntries(String baseDir)
        {
            return Directory.EnumerateFiles(baseDir, "*.*", SearchOption.AllDirectories)
                            .Where(x => Formatters.Keys.Contains(Path.GetExtension(x)))
                            .AsParallel() // お手軽並列化
                            .WithDegreeOfParallelism(8)
                            .Select(x => CreateEntry(baseDir, x))
                            .ToArray();
        }
    }
}
