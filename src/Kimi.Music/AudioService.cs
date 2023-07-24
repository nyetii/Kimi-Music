using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.DependencyInjection;

namespace Kimi.Music
{
    public class AudioService
    {
        private readonly IServiceProvider _service;
        private readonly IAudioService _audioService;

        public AudioService(IServiceProvider service)
        {
            _service = service;
            _audioService = _service.GetRequiredService<IAudioService>();
        }

        public async Task InitializeAsync()
        {
            await _audioService.InitializeAsync();
        }

        public async Task PlayAsync(string query, ulong guild, ulong vc)
        {
            var player = _audioService.GetPlayer<LavalinkPlayer>(guild)
                         ?? await _audioService.JoinAsync<LavalinkPlayer>(guild, vc);

            var track = await _audioService.GetTrackAsync("drake bell found a way", SearchMode.YouTube);

            await player.PlayAsync(track);
        }
    }
}
