using System;
using System.Collections.Generic;
using System.Linq;

namespace EinsLivePlaylistCrawler.Response
{
    internal class PlaylistBuilder
    {
        private DateTime start;

        private DateTime end;

        private List<PlaylistItem> items = new List<PlaylistItem>();

        public PlaylistBuilder SetStart(DateTime start)
        {
            this.start = start;
            return this;
        }

        public PlaylistBuilder SetEnd(DateTime end)
        {
            this.end = end;
            return this;
        }

        public PlaylistBuilder AddItem(PlaylistItem item)
        {
            this.items.Insert(0, item);
            return this;
        }

        public bool HasItem(DateTime dateTime, string title, string artist)
        {
            return items.Any(item => item.Time == dateTime && item.Title == title && item.Artist == artist);
        }

        public Playlist GetPlaylist()
        {
            var playlist = new Playlist(start, end, items.AsReadOnly());
            return playlist;
        }
    }
}
