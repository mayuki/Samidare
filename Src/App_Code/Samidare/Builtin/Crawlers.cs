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
    public class Crawlers
    {
        public static void Register(SamidareEngine engine)
        {
            engine.Crawlers["Entries"] = (samidareEngine) => samidareEngine.LoadEntries(Path.Combine(samidareEngine.RootDirectory, "Entries"));
        }
    }
}
