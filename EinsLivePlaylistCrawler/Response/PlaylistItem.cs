using System;

namespace EinsLivePlaylistCrawler.Response
{
    public class PlaylistItem
    {
        public DateTime Time { get; }

        public string Title { get; }

        public string Artist { get; }

        public PlaylistItem(DateTime time, string title, string artist)
        {
            Time = time;
            Title = title;
            Artist = artist;
        }
    }
}
