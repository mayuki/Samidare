using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Samidare.Model
{
    /// <summary>
    /// エントリーの情報を保持するクラスです。
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// 内容のHTMLを取得、設定します。
        /// </summary>
        public String Content { get; set; }

        /// <summary>
        /// タイトルを取得、設定します。
        /// </summary>
        public String Title
        {
            get { return Metadata.ContainsKey("Title") ? Metadata["Title"] : String.Empty; }
            set { Metadata["Title"] = value; }
        }

        /// <summary>
        /// ファイルのパスを取得、設定します。
        /// </summary>
        public String FilePath
        {
            get { return Metadata.ContainsKey("FilePath") ? Metadata["FilePath"] : String.Empty; }
            set { Metadata["FilePath"] = value; }
        }
        /// <summary>
        /// サイト上のパスを取得、設定します。
        /// </summary>
        public String Path
        {
            get { return Metadata.ContainsKey("Path") ? Metadata["Path"] : String.Empty; }
            set { Metadata["Path"] = value; }
        }
        /// <summary>
        /// 変更時刻を取得、設定します。
        /// </summary>
        public DateTime ModifiedAt { get; set; }
        /// <summary>
        /// 作成時刻を取得、設定します。
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// エントリーのメタデータを取得、設定します。
        /// </summary>
        public IDictionary<String, dynamic> Metadata { get; set; }

        /// <summary>
        /// ベースディレクトリ、実体へのファイルパス、タイトル、内容のHTMLを指定してエントリー情報を初期化します。
        /// </summary>
        /// <param name="entriesBaseDir"></param>
        /// <param name="path"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public Entry(String entriesBaseDir, String path, String title, String content)
        {
            Metadata = new Dictionary<String, dynamic>(StringComparer.OrdinalIgnoreCase);
            ModifiedAt = File.GetLastWriteTime(path);
            CreatedAt = File.GetCreationTime(path);
            FilePath = path;
            // Remove .txt & Entries Directory path + replace "\" -> "/"
            Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path)).Replace(entriesBaseDir, "").Replace('\\', '/').TrimStart('/');
            Content = content;
            Title = title;

            CreateDefaultMetadata();
        }

        /// <summary>
        /// ベースディレクトリ、実体へのファイルパスを指定してエントリー情報を初期化します。
        /// 内容とタイトルはファイルから自動的に読み取ります。
        /// </summary>
        /// <param name="entriesBaseDir"></param>
        /// <param name="path"></param>
        public Entry(String entriesBaseDir, String path)
            : this(entriesBaseDir, path, null, null)
        {
            Content = File.ReadAllText(path, Encoding.UTF8);
            Title = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        private void CreateDefaultMetadata()
        {
            Metadata["Tags"] = new String[0];
        }

        public Boolean ParseMetadata(IDictionary<String, Func<String, dynamic>> metadataConverter)
        {
            if (String.IsNullOrWhiteSpace(Content))
            {
                return false;
            }

            var m = Regex.Match(Content, @"\A---(\s*\r?\n.*?\n?)(\n---\s*\n?)", RegexOptions.Singleline);
            if (m.Success)
            {
                // TODO: YAML…あっ、はい…
                var frontMatterBlock = m.Groups[1].Value.Trim();
                Content = Content.Substring(m.Index + m.Length);

                foreach (var keyValue in frontMatterBlock.Split('\n')
                    .Select(x => x.Split(new[] {':'}, 2))
                    .Where(x => x.Length == 2)
                    .Select(x => new {Key = x[0].Trim(), Value = x[1].Trim()}))
                {
                    Metadata[keyValue.Key] = metadataConverter.ContainsKey(keyValue.Key)
                                                ? metadataConverter[keyValue.Key](keyValue.Value)
                                                : keyValue.Value;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}