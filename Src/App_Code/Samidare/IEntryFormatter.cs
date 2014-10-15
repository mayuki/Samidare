using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Samidare.Model;

namespace Samidare
{
    /// <summary>
    /// IEntryFormatter
    /// </summary>
    public interface IEntryFormatter
    {
        /// <summary>
        /// フォーマットを行う処理。この処理を実行するとContentをフォーマットした状態に置き換えます。
        /// </summary>
        /// <param name="entry"></param>
        void Process(Entry entry);
    }
}