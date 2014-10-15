using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Samidare.Builtin
{
    public class PostProcessors
    {
        public static void Register(SamidareEngine engine)
        {
            engine.PostProcessors.Add("Builtin/Published",
                entries => entries.Where(entry =>
                    !(
                        entry.Metadata.ContainsKey("published") &&
                        !((Boolean) entry.Metadata["published"])
                    )));

            engine.PostProcessors.Add("Builtin/CreatedAt", entries => entries.Select(entry =>
            {
                // FrontMatterのcreatedatがある
                if (entry.Metadata.ContainsKey("CreatedAt"))
                {
                    entry.CreatedAt = (DateTime) entry.Metadata["CreatedAt"];
                    return entry;
                }

                var m = Regex.Match(entry.Path, @"(\d{4}[/\\-]\d{1,2}[/\\-]\d{1,2})[/\\-]");
                if (m.Success)
                {
                    DateTime parsed;
                    if (DateTime.TryParseExact(m.Groups[1].Value, new[] { "yyyy/MM/dd", "yyyy-MM-dd" }, null, System.Globalization.DateTimeStyles.AssumeLocal, out parsed))
                    {
                        entry.CreatedAt = parsed;
                    }
                }

                return entry;
            }));

            engine.PostProcessors.Add("Order",
                entries => entries.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Path));
        }
    }
}
