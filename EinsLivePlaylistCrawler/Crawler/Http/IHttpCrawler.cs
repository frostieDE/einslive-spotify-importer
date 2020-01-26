using System;
using System.Threading.Tasks;

namespace EinsLivePlaylistCrawler.Crawler.Http
{
    /// <summary>
    /// Interface for any HttpCrawler. Any class implementing this interface is expected
    /// to get the HTML body of the website which contains the playlist.
    /// </summary>
    public interface IHttpCrawler
    {
        /// <summary>
        /// Returns the HTML of the playlist website for the given time.
        /// </summary>
        /// <param name="time">The time which is submitted to the playlist website.</param>
        /// <returns></returns>
        Task<string> GetHtmlAsync(DateTime time);
    }
}
