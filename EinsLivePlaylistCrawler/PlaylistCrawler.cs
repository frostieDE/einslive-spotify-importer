using EinsLivePlaylistCrawler.Response;
using EinsLivePlaylistCrawler.Crawler.Html;
using EinsLivePlaylistCrawler.Crawler.Http;
using EinsLivePlaylistCrawler.Request;
using System;
using System.Threading.Tasks;

namespace EinsLivePlaylistCrawler
{
    public class PlaylistCrawler : IPlaylistCrawler
    {
        private IHtmlPlaylistParser parser;
        private IHttpCrawler httpCrawler;

        public PlaylistCrawler(IHtmlPlaylistParser parser, IHttpCrawler crawler)
        {
            this.parser = parser;
            this.httpCrawler = crawler;
        }

        public async Task<Playlist> CrawlAsync(PlaylistRequest request)
        {
            var playlistBuilder = new PlaylistBuilder()
                .SetStart(request.Start)
                .SetEnd(request.End);

            PlaylistItem lastItem = null;
            DateTime end = request.End;

            do
            {
                System.Diagnostics.Debug.WriteLine($"Start request for time: {end.ToString()}");
                var html = await httpCrawler.GetHtmlAsync(end);
                var items = parser.ParseHtmlPlaylist(html);

                foreach (var item in items)
                {
                    System.Diagnostics.Debug.WriteLine($"Handle item at ${item.Time.ToString()}");
                    if (item.Time >= request.Start
                        && item.Time < request.End
                        && !playlistBuilder.HasItem(item.Time, item.Title, item.Artist))
                    {
                        playlistBuilder.AddItem(item);
                        lastItem = item;
                    }

                    end = item.Time;
                }
            } while (lastItem != null && end >= request.Start);

            return playlistBuilder.GetPlaylist();
        }
    }
}
