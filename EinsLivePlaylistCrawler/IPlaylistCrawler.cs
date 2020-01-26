using EinsLivePlaylistCrawler.Request;
using EinsLivePlaylistCrawler.Response;
using System.Threading.Tasks;

namespace EinsLivePlaylistCrawler
{
    public interface IPlaylistCrawler
    {
        Task<Playlist> CrawlAsync(PlaylistRequest request);
    }
}
