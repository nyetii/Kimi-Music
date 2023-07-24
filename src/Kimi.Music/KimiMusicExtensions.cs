using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.DependencyInjection;

namespace Kimi.Music
{
    public static class KimiMusicExtensions
    {
        public static IServiceCollection ConfigureLavaPlayer(this IServiceCollection service)
        {
            service.AddSingleton<IAudioService, LavalinkNode>();
            service.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

            service.AddSingleton(new LavalinkNodeOptions()
            {
                WebSocketUri = "ws://localhost:2333",
                Password = "youshallnotpass"
            });

            service.AddSingleton(new AudioService(service.BuildServiceProvider()));
            return service;
        }

    }
}