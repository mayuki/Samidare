using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using Samidare.Model;

namespace Samidare.ViewModel
{
    /// <summary>
    /// </summary>
    public class EntriesViewModel
    {
        public SamidareEngine Engine { get; set; }
        public Entry[] Entries { get; set; }
        public IDictionary<String, ILookup<String, Entry>> EntryIndexes
        {
            get { return Engine.EntryIndexes; }
        }
        public SamidareContext Context { get; set; }
        public WebPageContext ParentPageContext { get; set; }
        public Paging<Entry> Paging { get; set; }

        public String Title { get; set; }
        public dynamic Data { get; set; }
    }

    /// <summary>
    /// </summary>
    public class EntriesViewModel<T> : EntriesViewModel
    {
        public T Data
        {
            get { return (T)base.Data; }
            set { base.Data = value; }
        }
    }
}
