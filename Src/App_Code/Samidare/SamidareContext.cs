using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Samidare
{
    /// <summary>
    /// Samidareのメインのクラスです。このクラスを通して実行します。
    /// </summary>
    public class SamidareContext
    {
        /// <summary>
        /// サイトごとのエンジン
        /// </summary>
        public SamidareEngine Engine { get; private set; }

        /// <summary>
        /// サイトの名前
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// サイトの説明
        /// </summary>
        public String Description { get; set; }
        
        /// <summary>
        /// サイト上のルートパス(Samidareの設置されているルート)
        /// </summary>
        public String SiteRoot { get; set; }

        /// <summary>
        /// データのディレクトリ(EntriesとかTemplatesを含むディレクトリ)
        /// </summary>
        public String DataDirectory { get; set; }

        /// <summary>
        /// テンプレートのセットの名前。デフォルトは Default
        /// </summary>
        public String TemplatesSetName { get; set; }

        /// <summary>
        /// レイアウトのパス。未指定時にはテンプレートディレクトリのLayout.cshtml
        /// </summary>
        public String DefaultLayout { get; set; }

        /// <summary>
        /// 一ページに表示するエントリーの数
        /// </summary>
        public Int32 ShowEntriesCount { get; set; }

        /// <summary>
        /// エントリーマネージャが生成されたときに実行される処理
        /// </summary>
        public Action<SamidareEngine> EngineCreated { get; set; }

        /// <summary>
        /// デバッグ用にエンジンのキャッシュを無効化するかどうか
        /// </summary>
        public Boolean DisableEngineCache { get; set; }

        /// <summary>
        /// 静的生成モードかどうか
        /// </summary>
        public Boolean IsStaticGeneration { get; set; }

        public SamidareContext(String siteRoot, String dataDirectory, Action<SamidareEngine> engineCreated = null)
        {
            Name = "Samidare";
            SiteRoot = siteRoot.TrimEnd('/');
            DataDirectory = dataDirectory;
            TemplatesSetName = "Default";
            ShowEntriesCount = 5;
            EngineCreated = engineCreated;
        }

        /// <param name="entryPointPagePath">実行を開始したページの物理ファイルパス</param>
        public void Initialize(String entryPointPagePath)
        {
            // TODO: HttpContextェ…
            Engine = SamidareEngine.GetInstance(
                entryPointPagePath,
                Path.Combine(HttpContext.Current.Server.MapPath(DataDirectory), "Data"),
                EngineCreated,
                DisableEngineCache
            );
        }
    }
}
