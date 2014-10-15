using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Samidare.Model
{
    /// <summary>
    /// ルーティングした結果を保持するクラス
    /// </summary>
    public class RouteDispatchResult
    {
        public String ViewName { get; set; }
        public Entry[] Entries { get; set; }
        public Object ViewData { get; set; }

        public RouteDispatchResult(String viewName, Entry[] entries, Object viewData = null)
        {
            ViewName = viewName;
            Entries = entries;
            ViewData = viewData;
        }
    }
}