using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kimi.Commands.Modules.Music
{
    public class Player : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IAudioService _audio;

        public Player(IAudioService audio)
        {
            _audio = audio;
        }

        [SlashCommand("play", "play")]
        public async Task HandlePlayCommand(string query)
        {
            await DeferAsync();

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            var user = (Context.Guild.GetUser(Context.Client.CurrentUser.Id));

            if (channel is null)
            {
                await FollowupAsync("entra no canal filho da puta");
                return;
            }

            //var player = new PlayerHandler(Context);
            
            var player = _audio.GetPlayer<PlayerHandler>(channel!.GuildId)
                         ?? await _audio.JoinAsync<PlayerHandler>(channel.GuildId, channel.Id);

            player.Context = Context;
            // Context.Client.UserVoiceStateUpdated += HandleVoiceStateUpdateAsync;

            var first = PlayerHandler.Queue.Count is 0;


            if (query.Contains("playlist?list="))
            {
                var tracks = await _audio.GetTracksAsync(query, SearchMode.YouTube);
                tracks.ToList().ForEach(PlayerHandler.Queue.Enqueue);

                foreach (var item in PlayerHandler.Queue)
                {
                    PlayerHandler.TimeStamp += item.Duration;
                }

                await FollowupAsync("Playlist adicionada");
            }
            else
            {
                var track = await _audio.GetTrackAsync(query, SearchMode.YouTube);
                //await player.PlayAsync(track);
                PlayerHandler.Queue.Enqueue(track);

                PlayerHandler.TimeStamp += track.Duration;

                var embed = new EmbedBuilder()
                    .WithAuthor("Faixa adicionada")
                    .WithTitle($"{track.Title}")
                    .WithDescription($"{PlayerHandler.FieldDescription(track)}")
                    .WithFooter($"Duração Total - {PlayerHandler.GetTotalDuration()}")
                    .Build();
                
                await FollowupAsync(embed: embed);

                if(first)
                    await DeleteOriginalResponseAsync();
            }

            if (first)
                await player.PlayAsync(PlayerHandler.Queue.FirstOrDefault()!);
            //await player.OnTrackEndAsync(new TrackEndEventArgs(player, track.TrackIdentifier, TrackEndReason.Finished));
        }

        [SlashCommand("nowplaying", "play")]
        public async Task HandleCurrentCommand()
        {
            await DeferAsync();

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            var player = _audio.GetPlayer<LavalinkPlayer>(channel!.GuildId)
                         ?? await _audio.JoinAsync<LavalinkPlayer>(channel.GuildId, channel.Id);

            var embed = new EmbedBuilder()
                .WithAuthor("Faixa atual")
                .WithTitle($"{player.CurrentTrack.Title}")
                .WithDescription($"{PlayerHandler.FieldDescription(player.CurrentTrack)}")
                .WithFooter($"Duração Total - {PlayerHandler.GetTotalDuration()}")
                .Build();

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("skip", "skip")]
        public async Task HandleSkipCommand()
        {
            await DeferAsync();

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            var user = (Context.Guild.GetUser(Context.Client.CurrentUser.Id));

            if (channel is null)
            {
                await FollowupAsync("entra no canal filho da puta");
                return;
            }

            //var player = new PlayerHandler(Context);

            var player = _audio.GetPlayer<PlayerHandler>(channel!.GuildId);

            await FollowupAsync("Pulando faixa...");

            await player.StopAsync();
            
            //await player.OnTrackEndAsync(new TrackEndEventArgs(player, TrackQueue.Queue.FirstOrDefault().TrackIdentifier,
            //    TrackEndReason.Replaced));
        }

        [SlashCommand("queue", "queue")]
        public async Task QueueCommand(int page = 1) => await HandleQueueCommand(page);
        

        public async Task HandleQueueCommand(int page = 1, bool defer = true)
        {
            if(defer)
                await DeferAsync();
            //var tracks = PlayerHandler.Queue.Take(10).ToList();

            var tracks = Take(page).ToList();

            var fields = tracks.ConvertAll(BuildField);

            var embed = new EmbedBuilder()
                .WithTitle($"Página {page} de {FetchPages()}")
                .WithAuthor("Fila")
                .WithFields(fields)
                .WithFooter($"Duração Total - {PlayerHandler.GetTotalDuration()}")
                .Build();


            if(PlayerHandler.Queue.Count is 0)
            {
                embed = new EmbedBuilder()
                    .WithAuthor("Idiota")
                    .WithTitle("Não consigo ler nada!!!")
                    .WithDescription("Não tem nada na fila. Bote alguma faixa para que apareça algo aqui.")
                    .WithFooter($"Duração Total - {PlayerHandler.GetTotalDuration()}")
                    .Build();

                await FollowupAsync(embed: embed);
                return;
            }

            if (tracks.Count is 0)
            {
                await HandleQueueCommand(1, false);
                return;
            }

            await FollowupAsync(embed: embed);
        }

        private IEnumerable<LavalinkTrack> Take(int page)
        {
            page--;
            var queue = PlayerHandler.Queue.ToList();

            var tracks = queue.ToDictionary(queue.IndexOf);
            
            return tracks
                .Where(x => x.Key >= page * 10 && x.Key < page * 10 + 10)
                .Select(x => x.Value)
                .AsEnumerable();
        }

        private int FetchPages() => PlayerHandler.Queue.Count / 10 + 1;

        private EmbedFieldBuilder BuildField(LavalinkTrack track)
        {
            var duration = track.Duration.TotalHours >= 1
                ? track.Duration.ToString(@"hh\:mm\:ss")
                : track.Duration.ToString(@"mm\:ss");

            var description = $"by {track.Author} on {track.SourceName}";

            if (description.Length > 33)
                description = description.Replace($"on {track.SourceName}", "");
            if (description.Length > 33)
                description = description.Remove(31).Insert(31, "...");
            
            var value = $"⏰ `{duration}`_ㅤㅤ_🔗 [Acessar]({track.Uri.AbsoluteUri})_ㅤㅤ_👤 `{description}`";

            Console.WriteLine(value);

            return new EmbedFieldBuilder()
                .WithName($"{PlayerHandler.Queue.ToList().IndexOf(track)+1}. {track.Title}")
                .WithValue(value)
                .WithIsInline(false);
        }

        
    }
}
