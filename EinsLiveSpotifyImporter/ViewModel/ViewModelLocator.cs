using Autofac;
using EinsLivePlaylistCrawler;
using EinsLivePlaylistCrawler.Crawler.Html;
using EinsLivePlaylistCrawler.Crawler.Http;
using EinsLiveSpotifyImporter.Core.Spotify;
using GalaSoft.MvvmLight.Messaging;

namespace EinsLiveSpotifyImporter.ViewModel
{
    public class ViewModelLocator
    {
        private static IContainer container;

        static ViewModelLocator()
        {
            RegisterServices();
        }

        public static void RegisterServices()
        {
            var builder = new ContainerBuilder();


            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();

            builder.RegisterType<Spotify>().As<ISpotify>().SingleInstance();

            builder.RegisterType<PlaylistCrawler>().As<IPlaylistCrawler>().SingleInstance();
            builder.RegisterType<HtmlPlaylistParser>().As<IHtmlPlaylistParser>().SingleInstance();
            builder.RegisterType<HttpCrawler>().As<IHttpCrawler>().SingleInstance();

            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();

            container = builder.Build();
        }

        public IMessenger Messenger { get { return container.Resolve<IMessenger>(); } }

        public MainViewModel Main { get { return container.Resolve<MainViewModel>(); } }
    }
}
