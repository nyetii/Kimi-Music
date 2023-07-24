using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;

namespace Kimi.Commands.Modules.Music
{
    public class PlayerHandler : LavalinkPlayer
    {
        public static Queue<LavalinkTrack> Queue { get; set; } = new();
        public static TimeSpan TimeStamp { get; set; }
        public SocketInteractionContext Context { get; set; } = null!;
        
        public override async Task PlayAsync(LavalinkTrack track, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = false)
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Próxima faixa")
                .WithTitle($"{track.Title}")
                .WithDescription($"{FieldDescription(track)}")
                .WithFooter($"Duração Total - {GetTotalDuration()}")
                .WithColor(0xfe8a7b)
                .Build();
            
            await Context.Channel.SendMessageAsync(embed: embed);

            Context.Client.UserVoiceStateUpdated += HandleVoiceStateUpdateAsync;
            await base.PlayAsync(track, startTime, endTime, noReplace);
        }

        public override async Task OnTrackEndAsync(TrackEndEventArgs eventArgs)
        {
            Context.Client.UserVoiceStateUpdated -= HandleVoiceStateUpdateAsync;

            TimeStamp -= Queue.First().Duration;
            Queue.Dequeue();
            
            if(Queue.Count > 0)
                await PlayAsync(Queue.FirstOrDefault());
            else
                await base.OnTrackEndAsync(eventArgs);
            //await base.OnTrackEndAsync(eventArgs);
        }

        public override async Task OnTrackExceptionAsync(TrackExceptionEventArgs eventArgs)
        {
            await Client.SendVoiceUpdateAsync(Context.Guild.Id, (Context.User as IGuildUser)?.VoiceChannel.Id, true, true);
            await base.OnTrackExceptionAsync(eventArgs);
        }

        public override async Task OnTrackStuckAsync(TrackStuckEventArgs eventArgs)
        {
            await Client.SendVoiceUpdateAsync(Context.Guild.Id, (Context.User as IGuildUser)?.VoiceChannel.Id, false, true);
            await base.OnTrackStuckAsync(eventArgs);
        }

        public async Task HandleVoiceStateUpdateAsync(SocketUser user, SocketVoiceState state1, SocketVoiceState state2)
        {
            if (user.Id != Context.Client.CurrentUser.Id)
                return;

            if (state1.VoiceChannel == null)
                return;

            if (state2.VoiceChannel == null)
            {
                Queue = new Queue<LavalinkTrack>();
                TimeStamp = TimeSpan.Zero;
                return;
            }

            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            var bot = (Context.Guild.GetUser(Context.Client.CurrentUser.Id));

            
            var player = this;

            await Context.Channel.SendMessageAsync($"{state1.VoiceChannel} vs {state2.VoiceChannel}");

            if (state1.IsMuted != state2.IsMuted)
                if (state2.IsMuted)
                    await player.PauseAsync();
                else
                    await player.ResumeAsync();

            if (bot.IsMuted || bot.IsDeafened)
                await player.PauseAsync();
            else
                await player.ResumeAsync();

        }

        public static string FieldDescription(LavalinkTrack track)
        {
            var duration = track.Duration.TotalHours >= 1
                ? track.Duration.ToString(@"hh\:mm\:ss")
                : track.Duration.ToString(@"mm\:ss");

            var description = $"by {track.Author} on {track.SourceName}";

            if (description.Length > 33)
                description = description.Replace($"on {track.SourceName}", "");
            if (description.Length > 33)
                description = description.Remove(31).Insert(31, "...");

            return $"⏰ `{duration}`_ㅤㅤ_🔗 [Acessar]({track.Uri.AbsoluteUri})_ㅤㅤ_👤 `{description}`";
        }

        public static string GetTotalDuration()
        {
            return PlayerHandler.TimeStamp.TotalHours >= 1
                ? PlayerHandler.TimeStamp.ToString(@"hh\:mm\:ss")
                : PlayerHandler.TimeStamp.ToString(@"mm\:ss");
        }
    }
}
