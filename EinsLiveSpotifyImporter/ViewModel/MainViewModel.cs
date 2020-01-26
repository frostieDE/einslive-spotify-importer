using EinsLivePlaylistCrawler;
using EinsLivePlaylistCrawler.Request;
using EinsLiveSpotifyImporter.Core.Spotify;
using EinsLiveSpotifyImporter.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace EinsLiveSpotifyImporter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private bool isBusy = false;

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                Set(() => IsBusy, ref isBusy, value);

                ConnectCommand?.RaiseCanExecuteChanged();
                LoadPlaylistsCommand?.RaiseCanExecuteChanged();
                SyncPlaylistCommand?.RaiseCanExecuteChanged();
            }
        }

        public double ProgressValue { get { return Progress.HasValue ? Progress.Value : 0; } }

        public bool HasProgress { get { return Progress.HasValue; } }

        private double? progress = 0;

        public double? Progress
        {
            get { return progress; }
            set
            {
                Set(() => Progress, ref progress, value);

                RaisePropertyChanged(() => ProgressValue);
                RaisePropertyChanged(() => HasProgress);
            }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set { Set(() => Status, ref status, value); }
        }

        #region Playlists

        private Playlist playlist;

        public Playlist Playlist
        {
            get { return playlist; }
            set
            {
                Set(() => Playlist, ref playlist, value);

                SyncPlaylistCommand?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<Playlist> Playlists { get; } = new ObservableCollection<Playlist>();

        #endregion

        #region Dates

        private DateTime? startTime;

        public DateTime? StartTime
        {
            get { return startTime; }
            set
            {
                Set(() => StartTime, ref startTime, value);

                SyncPlaylistCommand?.RaiseCanExecuteChanged();
            }
        }

        private DateTime? endTime;

        public DateTime? EndTime
        {
            get { return endTime; }
            set
            {
                Set(() => EndTime, ref endTime, value);

                SyncPlaylistCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        public ObservableCollection<SearchResult> SearchResults { get; } = new ObservableCollection<SearchResult>();

        #endregion

        #region Commands

        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand LoadPlaylistsCommand { get; private set; }
        public RelayCommand SyncPlaylistCommand { get; private set; }

        #endregion

        #region Services

        private ISpotify spotify;
        private IPlaylistCrawler crawler;

        #endregion

        public MainViewModel(ISpotify spotify, IPlaylistCrawler crawler)
        {
            this.spotify = spotify;
            this.crawler = crawler;

            StartTime = DateTime.Now.AddHours(-4);
            EndTime = DateTime.Now;

            ConnectCommand = new RelayCommand(Connect, CanConnect);
            LoadPlaylistsCommand = new RelayCommand(LoadPlaylists, CanLoadPlaylists);
            SyncPlaylistCommand = new RelayCommand(SyncPlaylist, CanSyncPlaylist);
        }

        private async void SyncPlaylist()
        {
            IsBusy = true;
            var selectedPlaylist = Playlist;

            // Step One: Download playlist from 1LIVE
            Status = "(1/3) Lade 1LIVE Playlist herunter";
            Progress = null;

            var request = new PlaylistRequest
            {
                Start = StartTime.Value,
                End = EndTime.Value
            };

            var playlist = await crawler.CrawlAsync(request);

            // Step Two: Search tracks at Spotify

            Progress = 0;

            var i = 0;
            var tracks = new List<Track>();
            SearchResults.Clear();

            foreach (var item in playlist.Items)
            {
                Status = $"(2/3) Suche {item.Artist} - {item.Title}";

                var track = await spotify.SearchOneTrackAsync(item.Artist, item.Title);

                if (track != null)
                {
                    tracks.Add(track);
                }

                var result = new SearchResult
                {
                    SpotifyTrack = track,
                    PlaylistItem = item
                };

                SearchResults.Add(result);

                i++;
                Progress = ((double)i / playlist.Items.Count) * 100;
            }

            // Step Three: Replace playlist tracks with new track list
            Progress = null;
            Status = "(3/3) Aktualisiere Playlist";

            await spotify.UpdatePlaylistTracksAsync(selectedPlaylist, tracks);

            Progress = 0;
            Status = "Fertig 😊";

            IsBusy = false;
        }

        private bool CanSyncPlaylist() => !IsBusy
            && spotify.IsAuthorized
            && StartTime != null
            && EndTime != null
            && StartTime.Value < EndTime.Value
            && Playlist != null;

        private async void LoadPlaylists()
        {
            IsBusy = true;
            Progress = null;
            Status = "Lade Playlists...";

            var playlists = await spotify.GetPlaylistsAsync();

            Playlists.Clear();

            foreach (var playlist in playlists)
            {
                Playlists.Add(playlist);
            }

            Status = string.Empty;
            Progress = 0;
            IsBusy = false;
        }

        private bool CanLoadPlaylists() => !IsBusy && spotify.IsAuthorized;

        private async void Connect()
        {
            try
            {
                IsBusy = true;
                await spotify.AuthorizeAsync();
                LoadPlaylists();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanConnect() => !isBusy && !spotify.IsAuthorized;
    }
}
