using EinsLivePlaylistCrawler.Response;
using EinsLiveSpotifyImporter.Core.Spotify;
using GalaSoft.MvvmLight;

namespace EinsLiveSpotifyImporter.Model
{
    public class SearchResult : ObservableObject
    {
        private PlaylistItem playlistItem;

        public PlaylistItem PlaylistItem
        {
            get { return playlistItem; }
            set { Set(() => PlaylistItem, ref playlistItem, value); }
        }

        private Track spotifyTrack;

        public Track SpotifyTrack
        {
            get { return spotifyTrack; }
            set { Set(() => SpotifyTrack, ref spotifyTrack, value); }
        }
    }
}
