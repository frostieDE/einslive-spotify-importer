using EinsLivePlaylistCrawler.Response;
using System.Collections.Generic;

namespace EinsLivePlaylistCrawler.Crawler.Html
{
    /// <summary>
    /// Any class implementing this interface is expected to convert the HTML
    /// body into a list of <see cref="PlaylistItem"/>s.
    /// </summary>
    public interface IHtmlPlaylistParser
    {
        /// <summary>
        /// Parses the given HTML body into <see cref="PlaylistItem"/>s. 
        /// </summary>
        /// <param name="html">The HTML body from the webserver.</param>
        /// <returns></returns>
        IReadOnlyList<PlaylistItem> ParseHtmlPlaylist(string html);
    }
}
