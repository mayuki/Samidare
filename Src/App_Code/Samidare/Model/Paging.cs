using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Samidare.Model
{
    /// <summary>
    /// ページングのための処理を提供するクラスです
    /// </summary>
    public class Paging
    {
        public Paging(Int32 totalItemCount, Int32 itemCountPerPage, Int32 currentPage = 1)
        {
            TotalItemCount = totalItemCount;
            ItemCountPerPage = itemCountPerPage;
            CurrentPage = currentPage;
        }

        /// <summary>
        /// 現在のページ (1オリジン)を取得します
        /// </summary>
        public Int32 CurrentPage { get; private set; }

        /// <summary>
        /// ページ数(1オリジン)を取得します
        /// </summary>
        public Int32 TotalPages { get { return (Int32)Math.Ceiling(TotalItemCount/(Double)ItemCountPerPage); } }

        /// <summary>
        /// 一ページあたりのアイテムの数を取得します
        /// </summary>
        public Int32 ItemCountPerPage { get; private set; }

        /// <summary>
        /// 全アイテムの数を取得します
        /// </summary>
        public Int32 TotalItemCount { get; private set; }

        /// <summary>
        /// 次のページがあるかどうかを取得します
        /// </summary>
        public Boolean HasNext { get { return CurrentPage < TotalPages; } }

        /// <summary>
        /// 前のページがあるかどうかを取得します
        /// </summary>
        public Boolean HasPrevious { get { return CurrentPage > 1; } }
    }

    /// <summary>
    /// ジェネリック型シーケンスのページングのための処理を提供するクラスです
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Paging<T> : Paging, IEnumerable<T>
    {
        private Lazy<T[]> _pagedItems;

        public Paging(IEnumerable<T> items, Int32 itemCountPerPage, Int32 currentPage = 1)
            : base(items.Count(), itemCountPerPage, currentPage)
        {
            _pagedItems = new Lazy<T[]>(() => items.Skip(ItemCountPerPage * (CurrentPage - 1)).Take(ItemCountPerPage).ToArray(), isThreadSafe: false);
        }

        /// <summary>
        /// ページングしたアイテムのシーケンスを取得します
        /// </summary>
        public T[] Items
        {
            get { return _pagedItems.Value; }
        }

        public static Paging<T> CreateFromParameters(IEnumerable<T> items, Int32 itemCountPerPage, NameValueCollection parameters, String pageParameterName = "page")
        {
            Int32 page = 1;
            if (!Int32.TryParse(parameters[pageParameterName], out page) || page < 1)
            {
                page = 1;
            }
            return new Paging<T>(items, itemCountPerPage, page);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
