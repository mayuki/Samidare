using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Samidare.Model;

namespace Samidare.Builtin
{
    public class Formatters
    {
        public static void Register(SamidareEngine engine)
        {
            engine.Formatters.Add(".txt", new TextFormatter());
            engine.Formatters.Add(".md", new MarkdownFormatter());
        }

        public class TextFormatter : IEntryFormatter
        {
            public void Process(Entry entry)
            {
                // タイトルとファイル名が同じだったら一行目をタイトルにして本文をそれ以降にする(歴史的経緯対応)
                if (entry.Title == Path.GetFileName(entry.Path))
                {
                    var firstLineEnd = entry.Content.IndexOf('\n');
                    if (firstLineEnd != -1)
                    {
                        entry.Title = entry.Content.Substring(0, firstLineEnd).Trim();
                        entry.Content = entry.Content.Substring(firstLineEnd);
                    }
                }
            }
        }

        public class MarkdownFormatter : IEntryFormatter
        {
            public Boolean EnableServerSideSyntaxHighlight { get; set; }

            public MarkdownFormatter()
            {
                EnableServerSideSyntaxHighlight = true;
            }

            public void Process(Entry entry)
            {
                String content;
                if (EnableServerSideSyntaxHighlight)
                {
                    content = new Kiwi.Markdown.MarkdownService(null).ToHtml(entry.Content);
                }
                else
                {
                    content = new MarkdownSharp.Markdown(new MarkdownSharp.MarkdownOptions
                    {
                        AutoHyperlink = true,
                        AutoNewLines = false,
                        EncodeProblemUrlCharacters = true,
                        LinkEmails = true
                    }).Transform(entry.Content);
                }
                entry.Content = content;
            }
        }
    }
}
