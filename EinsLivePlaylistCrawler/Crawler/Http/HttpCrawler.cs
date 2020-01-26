using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EinsLivePlaylistCrawler.Crawler.Http
{
    /// <summary>
    /// An actual HttpCrawler.
    /// <see cref="IHttpCrawler"/>
    /// </summary>
    public class HttpCrawler : IHttpCrawler
    {
        private const string Endpoint = @"https://www1.wdr.de/radio/1live/on-air/1live-playlist/index.jsp";

        private const string DateParam = "playlistSearch_date";
        private const string HoursParam = "playlistSearch_hours";
        private const string MinutesParam = "playlistSearch_minutes";

        private const string UserAgent = "EinslivePlaylistCrawler/C#";

        public async Task<string> GetHtmlAsync(DateTime time)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(DateParam, time.ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>(HoursParam, time.ToString("HH")),
                new KeyValuePair<string, string>(MinutesParam, time.ToString("mm")),
                new KeyValuePair<string, string>("submit", "suchen") // workaround
            };

            var postContent = new FormUrlEncodedContent(postData);
            var response = await client.PostAsync(Endpoint, postContent).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return content;
        }
    }
}
