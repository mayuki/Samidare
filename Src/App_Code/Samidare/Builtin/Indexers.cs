using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Samidare.Builtin
{
    public class Indexers
    {
        public static void Register(SamidareEngine engine)
        {
            engine.Indexers.Add("Tags",
                xs => xs.Where(x => x.Metadata.ContainsKey("Tags"))
                        .SelectMany(x => (x.Metadata["Tags"] as String[]), (x, k) => new { Tag = k, Entry = x })
                        .ToLookup(k => k.Tag, v => v.Entry));

            engine.Indexers.Add("Category",
                xs => xs.Where(x => x.Metadata.ContainsKey("Category") || x.Metadata.ContainsKey("Categories"))
                        .SelectMany(x =>
                        {
                            var category = (x.Metadata.ContainsKey("Category") ? x.Metadata["Category"] as String[] : null) ?? new String[0];
                            var categories = (x.Metadata.ContainsKey("Categories") ? x.Metadata["Categories"] as String[] : null) ?? new String[0];

                            return Enumerable.Concat(category, categories);
                        }, (x, k) => new { Tag = k, Entry = x })
                        .ToLookup(k => k.Tag, v => v.Entry));

            engine.Indexers.Add("Title",
                xs => xs.ToLookup(k => k.Title, v => v));

            engine.Indexers.Add("Path",
                xs => xs.SelectMany(x =>
                {
                    var path = x.Path.Split('/');
                    return x.Path.Split('/').Select((y, i) => String.Join("/", path.Take(i + 1))).Select(y => new { Path = y, Entry = x });
                })
                .ToLookup(k => k.Path, v => v.Entry));

            engine.Indexers.Add("ByMonth",
                xs => xs.ToLookup(k => new DateTime(k.CreatedAt.Year, k.CreatedAt.Month, 1).ToString("yyyy/MM"), v => v));

        }
    }
}
