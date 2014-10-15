using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Samidare.Model;
using Samidare.ViewModel;

namespace Samidare.Builtin
{
    public class Routes
    {
        public static void Register(SamidareEngine engine)
        {
            engine.Routes.Add(@"Tag/(.*)",
                (match, samidareEngine) =>
                {
                    var tag = match.Groups[1].Value ?? "";
                    return new RouteDispatchResult("Entries", samidareEngine.EntryIndexes["Tags"][tag].ToArray(), new FilteredViewModel { FilteredBy = "Tag", FilteredByValue = tag });
                });

            engine.Routes.Add(@"Feed",
                (match, samidareEngine) =>
                {
                    return new RouteDispatchResult("Feed", samidareEngine.Entries);
                });

            engine.Routes.Add(@"(.+)",
                (match, samidareEngine) =>
                {
                    var path = match.Groups[1].Value.TrimEnd('/');
                    var entries = samidareEngine.EntryIndexes["Path"][path].ToArray();

                    // 個別ページかリストページか
                    if (entries.Count() == 1 && entries.First().Path == path)
                    {
                        return new RouteDispatchResult("Entry", entries);
                    }
                    else
                    {
                        return new RouteDispatchResult("Entries", entries);
                    }
                });

            // default route
            engine.Routes.Add(@"", (match, samidareEngine) => new RouteDispatchResult("Entries", samidareEngine.Entries));
        }
    }
}
