using EinsLivePlaylistCrawler.Crawler.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EinsLivePlaylistCrawler.Test.Crawler.Html
{
    [TestClass]
    public class HtmlPlaylistParserTest
    {
        [TestMethod]
        public void ParseOneElement()
        {
            var html =
@"<div id='searchPlaylistResult'>
    <div class='table'>
        <table>
            <tbody>
                <tr>
                    <th>26.11.2017,<br>15.25 Uhr</th>
                    <td>Title</td>
                    <td>Artist</td>
                </tr>
            </tbody>
        </table>
    </div>
</div>";
            var parser = new HtmlPlaylistParser();
            var playlist = parser.ParseHtmlPlaylist(html);

            Assert.AreEqual(1, playlist.Count, "Playlist.Count is 1");

            var item = playlist.First();

            Assert.AreEqual("Artist", item.Artist, "Artist is correct");
            Assert.AreEqual("Title", item.Title, "Title is correct");
            Assert.AreEqual(new DateTime(2017, 11, 26, 15, 25, 00), item.Time, "Time is correct");
        }

        [TestMethod]
        public void ParseOneElementWithHtmlEntities()
        {
            var html =
@"<div id='searchPlaylistResult'>
    <div class='table'>
        <table>
            <tbody>
                <tr>
                    <th>26.11.2017,<br>15.25 Uhr</th>
                    <td>Title &amp; Appendix</td>
                    <td>Artist</td>
                </tr>
            </tbody>
        </table>
    </div>
</div>";
            var parser = new HtmlPlaylistParser();
            var playlist = parser.ParseHtmlPlaylist(html);

            Assert.AreEqual(1, playlist.Count, "Playlist.Count is 1");

            var item = playlist.First();

            Assert.AreEqual("Title & Appendix", item.Title, "Title is correct");
        }

        [TestMethod]
        public void ParseMultipleElements()
        {
            var html =
@"<div id='searchPlaylistResult'>
    <div class='table'>
        <table>
            <tbody>
                <tr>
                    <th>26.11.2017,<br>15.25 Uhr</th>
                    <td>Title</td>
                    <td>Artist</td>
                </tr>
                <tr>
                    <th>26.11.2017,<br>15.30 Uhr</th>
                    <td>Title2</td>
                    <td>Artist2</td>
                </tr>
                <tr>
                    <th>26.11.2017,<br>15.35 Uhr</th>
                    <td>Title3</td>
                    <td>Artist3</td>
                </tr>
            </tbody>
        </table>
    </div>
</div>";
            var parser = new HtmlPlaylistParser();
            var playlist = parser.ParseHtmlPlaylist(html);

            Assert.AreEqual(3, playlist.Count, "Playlist.Count is 3");

            var firstItem = playlist[0];

            Assert.AreEqual("Artist", firstItem.Artist, "Artist is correct");
            Assert.AreEqual("Title", firstItem.Title, "Title is correct");
            Assert.AreEqual(new DateTime(2017, 11, 26, 15, 25, 00), firstItem.Time, "Time is correct");

            var secondItem = playlist[1];

            Assert.AreEqual("Artist2", secondItem.Artist, "Artist is correct");
            Assert.AreEqual("Title2", secondItem.Title, "Title is correct");
            Assert.AreEqual(new DateTime(2017, 11, 26, 15, 30, 00), secondItem.Time, "Time is correct");

            var thirdItem = playlist[2];

            Assert.AreEqual("Artist3", thirdItem.Artist, "Artist is correct");
            Assert.AreEqual("Title3", thirdItem.Title, "Title is correct");
            Assert.AreEqual(new DateTime(2017, 11, 26, 15, 35, 00), thirdItem.Time, "Time is correct");
        }
    }
}
