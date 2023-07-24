using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Kimi.Commands.Modules.Generic
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        //private MilkshakeService _milkshake = new MilkshakeService();

        [SlashCommand("help", "Milkshake comes in clutch!")]
        public async Task HandleHelpCommand([Summary("help", "Find more specific help"), Autocomplete] string? search = null)
        {
            if(search is null)
                await RespondAsync(embed: await HelpEmbed());

            //var help = new HelpModule(Context);
            //var user = Context.Interaction.User.Id.ToString();

            //_ = search switch
            //{
            //    "generate" => help.HandleEmptyHelp(),
            //    "template" => help.HandleTemplateHelp(user, "false"),
            //    "source" => help.HandleSourceHelp(user, "false"),
            //    "toppings" => help.HandleToppingHelp(user, "false"),
            //    "vip" => help.HandleEmptyHelp(),

            //    "help" => RespondAsync(embed: await HelpEmbed()),
            //    "info" => help.HandleEmptyHelp(),
            //    "ping" => help.HandleEmptyHelp(),

            //    _ => help.HandleUnknownHelp()
            //};
        }

        private static async Task<Embed> HelpEmbed()
        {
            var author = new EmbedAuthorBuilder()
                .WithName("Milkshake comes in clutch!")
                .WithIconUrl("https://cdn.discordapp.com/emojis/783328274193448981.webp");

            var embed = new EmbedBuilder()
                .WithColor(0xf1c3c7)
                .WithAuthor(author)
                .WithDescription("For more specific information, type `/help <command>`\nFurther questions? Ping `@netty`")
                .AddField(info =>
                {
                    info.WithIsInline(true);
                    info.WithName("INFO");
                    info.WithValue("`help` *this embed*\n" +
                                   "`info` *info about the bot*\n" +
                                   "`ping` *gets the latency*");
                })
                .AddField(milkshake =>
                {
                    milkshake.WithIsInline(true);
                    milkshake.WithName("Milkshake");
                    milkshake.WithValue("`generate` *makes a random shitpost*\n" +
                                "`template`\n" +
                                "`source`\n" +
                                "`vip` *manages VIP users*");
                })
                .Build();

            await Task.CompletedTask;
            return embed;
        }

        [AutocompleteCommand("help", "help")]
        public async Task Autocomplete()
        {
            // ReSharper disable once UseNegatedPatternMatching
            var context = (Context.Interaction as SocketAutocompleteInteraction);

            if (context is null)
                return;

            var userInput = context.Data.Current.Value.ToString() ?? "";

            var commands = new[]
            {
                "generate",
                "template",
                "source",
                "toppings",
                "vip",
                "help",
                "info",
                "ping"
            };

            var preResult = commands
                .Select(command => new AutocompleteResult(command, command))
                .ToList();


            var results = new List<AutocompleteResult>(preResult)
                .Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));

            await context.RespondAsync(results.Take(25));
        }
    }
}
