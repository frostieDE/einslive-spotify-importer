using System;
using System.Collections.Generic;

namespace EinsLivePlaylistCrawler.Response
{
    public class Playlist
    {
        public DateTime Start { get; }

        public DateTime End { get; }

        public IReadOnlyList<PlaylistItem> Items { get; }

        public Playlist(DateTime start, DateTime end, IReadOnlyList<PlaylistItem> items)
        {
            Start = start;
            End = end;
            Items = items;
        }
    }
}
