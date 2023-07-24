using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Schema;
using Kimi.Commands;
using Kimi.Logging;
using Kimi.Services.Core;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;
using Info = Kimi.Services.Core.Info;
using KimiData = Kimi.Services.Core.KimiData;
using Log = Kimi.Logging.Log;
using Settings = Kimi.Services.Core.Settings;

namespace Kimi.Core
{
    internal class Program
    {
        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            Debug.Assert(Info.IsDebug = true);

            Console.Title = Info.IsDebug ? "Milkshake Simulator [DEBUG]" : "Milkshake Simulator";

            Console.WriteLine("test");
            await Task.Delay(20000);

            if (!Directory.Exists(Info.AppDataPath))
            {
                var token = Environment.GetEnvironmentVariable("TOKEN");
                var connection = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                if (token is not null)
                {
                    var firstSettings = new Settings()
                    {
                        ConnectionString = connection ?? "unknown"
                    };

                    new KimiData(firstSettings).LoadSettings();

                    Token.SetToken(token);

                    Console.Write("\nThe bot data can be found at ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Info.AppDataPath);
                    Console.ResetColor();
                    Console.WriteLine(".\n");
                    Console.WriteLine();
                }
                else await SetUpAsync();
            }

            var config = new ConfigurationBuilder()
                .Build();
            
            var logger = LoggerService.LoggerConfiguration(Info.AppDataPath);

            using var sr = new StreamReader($@"{Info.AppDataPath}\settings.kimi");
            var settings = (JObject) await JToken.ReadFromAsync(new JsonTextReader(sr));

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder => builder
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None)
                    .AddFilter("System.Net", LogLevel.None)
                    .AddFilter("Discord", LogLevel.None))
                .ConfigureServices((_, services) =>
                services
                .AddHttpClient()
                .AddSingleton(logger)
                .AddSingleton(config)
                .AddSingleton<Settings>(x =>
                {
                    try
                    {
                        var data = new KimiData();
                        return data.LoadSettings();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    throw new Exception("aaaa");
                })
                .AddSingleton(x =>
                {
                    var client = new DiscordSocketClient(new DiscordSocketConfig
                    {
                        GatewayIntents = (GatewayIntents)0xFEBF,
                        //GatewayIntents = GatewayIntents.All,
                        AlwaysDownloadUsers = false,
                    });
                    
                    client.Log += Log.Write;

                    return client;
                })
                .AddSingleton(x =>
                {
                    var interaction = new InteractionService(x.GetRequiredService<DiscordSocketClient>());

                    interaction.Log += Log.Write;

                    return interaction;
                })
                .AddSingleton(x => new CommandService())
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggerService>()
                //.ConfigureLavaPlayer())
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
                .AddSingleton(new LavalinkNodeOptions()
                {
                    RestUri = "http://lavalink:2333/",
                    WebSocketUri = "ws://lavalink:2333",
                    Password = "youshallnotpass"
                }))
                .Build();

            //service.AddSingleton<IAudioService, LavalinkNode>();
            //service.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>();

            //service.AddSingleton(new LavalinkNodeOptions()
            //{
            //    WebSocketUri = "ws://localhost:2333",
            //    Password = "youshallnotpass"
            //});

            //service.AddSingleton(new AudioService(service.BuildServiceProvider()));

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            //provider.GetRequiredService<LoggerService>().LoggerConfiguration(Info.AppDataPath);

            var client = provider.GetRequiredService<DiscordSocketClient>();
            var sCommands = provider.GetRequiredService<InteractionService>();
            
            
            var settings = provider.GetRequiredService<Settings>();
            
            await provider.GetRequiredService<CommandHandler>().InitializeSlashAsync();
            await provider.GetRequiredService<CommandHandler>().InitializePrefixAsync();

            //_client.Log += Log.Write;
            //sCommands.Log += Log.Write;
            //milkshake.Logger += Log.Write;
            client.Ready += async () =>
            {
                await Log.Write($"Revision {Info.Version}");

                var profile = settings.Profile;
                await client.SetGameAsync(profile?.Status, profile?.Link, profile.ActivityType);
                await client.SetStatusAsync(profile.UserStatus);

                var state = new Commands.Modules.Utils.CommandInfo(sCommands);
                await Log.Write(await state.HandleSlashCommandsTable());

                await Log.Write($"Logged in as <@{client.CurrentUser.Username}#{client.CurrentUser.Discriminator}>!");
                await Log.Write($"{profile.UserStatus} - {profile.ActivityType} {profile.Status}");

                try
                {
                    var audio = provider.GetRequiredService<IAudioService>();
                    await audio.InitializeAsync();
                    Console.WriteLine("Initialized");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            };

            await client.LoginAsync(TokenType.Bot, Token.GetToken());

            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task SetUpAsync()
        {
            Console.WriteLine("This seems to be the first time this program is run, " +
                              "or at least no data folder was found.\n" +
                              "Please input the requested data.");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Remember the program doesn't check whether the Connection String or Token are correct.");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Insert the database Connection String: ");
            Console.ResetColor();
            var connection = Console.ReadLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Insert the Discord API token: ");
            Console.ResetColor();
            var token = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(connection) || string.IsNullOrWhiteSpace(token))
                Environment.Exit(1);

            var firstSettings = new Settings
            {
                ConnectionString = connection
            };

            Console.WriteLine("\nCreating settings file.");
            new KimiData(firstSettings).LoadSettings();

            Console.WriteLine("Creating token file.");
            Token.SetToken(token);

            Console.Write("\nThe bot data can be found at ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Info.AppDataPath);
            Console.ResetColor();
            Console.WriteLine(".\n");

            Console.WriteLine("Press any key to proceed.");
            Console.ReadKey();

            Console.Clear();
        }
    }
}