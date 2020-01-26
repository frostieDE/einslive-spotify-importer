using System.Collections.Generic;
using System.Threading.Tasks;

namespace EinsLiveSpotifyImporter.Core.Spotify
{
    public interface ISpotify
    {
        /// <summary>
        /// Flag which indicates whether the user is authorized against Spotify API.
        /// </summary>
        bool IsAuthorized { get; }

        /// <summary>
        /// Holds the profile information about the currently logged in user
        /// </summary>
        Profile Profile { get; }

        /// <summary>
        /// Authorizes the application against the Spotify API.
        /// </summary>
        /// <returns></returns>
        Task AuthorizeAsync();

        /// <summary>
        /// Returns the user's playlists.
        /// </summary>
        /// <returns></returns>
        Task<List<Playlist>> GetPlaylistsAsync();

        /// <summary>
        /// Searches for a track in the Spotify database.
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<Track> SearchOneTrackAsync(string artist, string title);

        /// <summary>
        /// Updates the given playlist with the given tracks. All previous tracks are removed.
        /// </summary>
        /// <param name="playlist">Playlist which is to be updated.</param>
        /// <param name="tracks">Tracks which are added to the playlist.</param>
        /// <returns></returns>
        Task UpdatePlaylistTracksAsync(Playlist playlist, List<Track> tracks);
    }
}
