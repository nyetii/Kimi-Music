using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kimi.Logging;

namespace Kimi.Commands
{
    internal static class ExceptionHandler
    {
        public static async Task DeferExceptionAsync(this Exception ex, IInteractionContext context)
        {
            await Log.Write(ex.ToString(), Severity.Error);
            var application = await context.Client.GetApplicationInfoAsync();
            //var channel = await application.Owner.CreateDMChannelAsync();


            var embed = new EmbedBuilder()
                .WithAuthor(application.Name, context.Client.CurrentUser.GetAvatarUrl())
                .WithTitle($"Exception - {ex.Source}")
                .WithDescription($"```\n{ex}\n```")
                .WithCurrentTimestamp()
                .WithFooter($"#{context.Channel.Name} in {context.Guild.Name}")
                .WithColor(0xFF0000)
                .Build();




            await application.Owner.SendMessageAsync(embed: embed);
            await context.Channel.SendMessageAsync("Sorry, something went wrong.");
        }
    }
}
