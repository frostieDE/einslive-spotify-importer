using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EinsLiveSpotifyImporter.Core.Spotify
{
    public class Spotify : ISpotify
    {
        private const string ClientId = "2655e2af70054249980f1773cc34ffc5";
        private const int NumberOfMaxTracksToQueryPerRequest = 20;
        private const string SpotifyMarketToSearchIn = "DE";
        private const int AddArtistsToQueryThreshold = 1000;

        /// <summary>
        /// Special words that must be included in both Spootify's and 1LIVE's tracks (if present)
        /// </summary>
        private static string[] SpecialTitleWords = new string[] { "live", "remix", "dub", "mix", "edit", "version", "acoustic", "instrumental" };

        #region Properties

        public bool IsAuthorized { get; private set; }

        public Profile Profile { get; private set; }

        #endregion

        private SpotifyWebAPI spotify;

        public Task AuthorizeAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            var auth = new ImplicitGrantAuth(ClientId, "http://localhost:4002", "http://localhost:4002", Scope.UserReadPrivate | Scope.PlaylistReadPrivate | Scope.UserLibraryRead |
                Scope.PlaylistModifyPrivate | Scope.PlaylistModifyPublic | Scope.PlaylistReadCollaborative);

            auth.AuthReceived += async (_, payload) =>
            {
                auth.Stop();

                spotify = new SpotifyWebAPI()
                {
                    TokenType = payload.TokenType,
                    AccessToken = payload.AccessToken
                };

                IsAuthorized = true;
                var profile = await spotify.GetPrivateProfileAsync();

                Profile = new Profile
                {
                    Id = profile.Id,
                    Username = profile.DisplayName
                };

                tcs.SetResult(true);
            };

            auth.Start();
            auth.OpenBrowser();

            return tcs.Task;
        }

        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            if (spotify == null)
            {
                throw new InvalidOperationException("You must authorize first!");
            }

            Func<SimplePlaylist, Playlist> select = (SimplePlaylist x) =>
            {
                return new Playlist
                {
                    Id = x.Id,
                    Uri = x.Uri,
                    Name = x.Name,
                    OwnerId = x.Owner.Id
                };
            };

            Paging<SimplePlaylist> playlists = await spotify.GetUserPlaylistsAsync(Profile.Id);
            List<Playlist> list = playlists.Items.Select(select).ToList();

            while (playlists.Next != null)
            {
                playlists = await spotify.GetUserPlaylistsAsync(Profile.Id, 20, playlists.Offset + playlists.Limit);
                list.AddRange(playlists.Items.Select(select).ToList());
            }

            return list;
        }

        public async Task<Track> SearchOneTrackAsync(string artist, string title)
        {
            /**
             * This is the hard part - unfortunately.
             * 
             * Here is the idea on how it could work:
             * 1) Only search for the title of the track
             * 2) Compare the title from 1LIVE with spotify's title (either one of them need to be included in the other one)
             * 3) Compare the artists of the first <see cref="NumberOfTracksToQuery"/> results with the artist given
             */

            var artists = GetArtists(artist);
            title = NormalizeTitle(title);
            var query = title; // First try: only search for the title of the track
            var isArtistAddedToQuery = false;

            Track track = null;
            var offset = 0;
            var hasNextPage = false;

            do
            {
                // Step 1
                var results = await spotify.SearchItemsAsync(query, SearchType.Track, NumberOfMaxTracksToQueryPerRequest, offset, SpotifyMarketToSearchIn).ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine($"Handle {offset} of {results.Tracks.Total} track(s)");

                await Task.Run(() =>
                {
                    foreach (var spotifyTrack in results.Tracks.Items)
                    {
                        // Step 2
                        if (spotifyTrack.Name.ToLower().Contains(title.ToLower()) || title.ToLower().Contains(spotifyTrack.Name.ToLower()))
                        {
                            /**
                             * Special words policy: if the title of either the spotify or 1LIVE track contains any special words,
                             * this word must be present in the other title as well. <see cref="SpecialTitleWords"/> and <see cref="GetSpecialWords(string)"/>
                             */
                            var specialWordsInEinsLiveTrack = GetSpecialWords(title);
                            var specialWordsInSpotifyTrack = GetSpecialWords(spotifyTrack.Name);
                            var specialWordsIntersect = specialWordsInEinsLiveTrack.Intersect(specialWordsInSpotifyTrack).ToArray();

                            if(specialWordsIntersect.Length != specialWordsInEinsLiveTrack.Length || specialWordsIntersect.Length != specialWordsInSpotifyTrack.Length)
                            {
                                continue;
                            }

                            // Step 3
                            foreach (var spotifyArtist in spotifyTrack.Artists)
                            {
                                if (artist.ToLower().Contains(spotifyArtist.Name.ToLower()))
                                {
                                    track = new Track
                                    {
                                        Id = spotifyTrack.Id,
                                        Uri = spotifyTrack.Uri,
                                        Artist = string.Join(", ", spotifyTrack.Artists.Select(x => x.Name)),
                                        Title = spotifyTrack.Name
                                    };
                                }
                            }
                        }
                    }
                }).ConfigureAwait(false);

                if (results.Tracks.Total > AddArtistsToQueryThreshold && !isArtistAddedToQuery)
                {
                    offset = 0;
                    hasNextPage = true;
                    query = $"{artists[0]} - {title}";
                    isArtistAddedToQuery = true;
                }
                else
                {
                    hasNextPage = results.Tracks.HasNextPage();
                    offset += NumberOfMaxTracksToQueryPerRequest;
                }

            } while (track == null && hasNextPage);

            return track;
        }

        /// <summary>
        /// Splits up all the artists when 1LIVE uses "feat." and other artist connecting characters.
        /// TODO: More normalizing.
        /// </summary>
        /// <param name="inputArtist">The artist from 1LIVE website</param>
        private string[] GetArtists(string inputArtist)
        {
            return inputArtist.Split(new string[] { "feat.", "&", " x " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }

        private string NormalizeTitle(string title)
        {
            return title.Replace("?t", "'t");
        }

        public async Task UpdatePlaylistTracksAsync(Playlist playlist, List<Track> tracks)
        {
            var uris = tracks.Select(track => track.Uri).ToList();

            // Step One: Replace tracks (first 100 tracks)
            await spotify.ReplacePlaylistTracksAsync(playlist.Id, uris).ConfigureAwait(false);

            // Step Two: Add remaining tracks (as we are only able to replace/add 100 tracks
            uris.RemoveRange(0, Math.Min(uris.Count, 100));
            while (uris.Count > 100)
            {
                await spotify.AddPlaylistTracksAsync(playlist.Id, uris).ConfigureAwait(false);
                uris.RemoveRange(0, Math.Min(uris.Count, 100));
            }
        }

        private string[] GetSpecialWords(string title)
        {
            var foundWords = new List<string>();

            foreach (var word in SpecialTitleWords)
            {
                var patterns = new string[]
                {
                    "[" + word,
                    "(" + word,
                    word + "]",
                    word + ")",
                    " " + word,
                    word + " "
                };

                foreach (var pattern in patterns)
                {
                    if (title.ToLower().Contains(pattern))
                    {
                        foundWords.Add(word);
                        break;
                    }
                }
            }

            return foundWords.ToArray();
        }
    }
}
