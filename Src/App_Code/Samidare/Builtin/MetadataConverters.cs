using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Samidare.Builtin
{
    public class MetadataConverters
    {
        public static void Register(SamidareEngine engine)
        {
            engine.MetadataConverters.Add("Categories", x => x.Split(' '));
            engine.MetadataConverters.Add("Tags", x => x.Split(' '));
            engine.MetadataConverters.Add("Published", x => { Boolean published; return Boolean.TryParse(x, out published) && published; });
            engine.MetadataConverters.Add("CreatedAt", x => { DateTime createdAt; return DateTime.TryParse(x, out createdAt) ? (Object)createdAt : null; });
            engine.MetadataConverters.Add("ModifiedAt", x => { DateTime modifiedAt; return DateTime.TryParse(x, out modifiedAt) ? (Object)modifiedAt : null; });
        }
    }
}
