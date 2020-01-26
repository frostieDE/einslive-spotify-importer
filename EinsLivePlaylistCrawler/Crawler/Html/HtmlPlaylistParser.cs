using EinsLivePlaylistCrawler.Response;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace EinsLivePlaylistCrawler.Crawler.Html
{
    public class HtmlPlaylistParser : IHtmlPlaylistParser
    {
        private const string TableRowsXPath = "//*[@id=\"searchPlaylistResult\"]/div/table/tbody/tr";
        private const string TimeFormat = "dd.MM.yyyy,HH.mm";

        public IReadOnlyList<PlaylistItem> ParseHtmlPlaylist(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var rows = document.DocumentNode.SelectNodes(TableRowsXPath);
            var culture = CultureInfo.InvariantCulture;
            var playlist = new List<PlaylistItem>();

            foreach (var row in rows)
            {
                var thTime = row.Descendants("th").FirstOrDefault();
                var tdTitle = row.Descendants("td").ElementAt(0);
                var tdArtist = row.Descendants("td").ElementAt(1);

                var timeText = thTime.InnerText;
                var time = DateTime.ParseExact(timeText.Replace("Uhr", "").Trim(), TimeFormat, culture);
                var title = tdTitle.InnerText.Trim();
                var artist = tdArtist.InnerText.Trim();

                title = HttpUtility.HtmlDecode(title);
                artist = HttpUtility.HtmlDecode(artist);

                var item = new PlaylistItem(time, title, artist);
                playlist.Add(item);
            }

            return playlist.AsReadOnly();
        }
    }
}
