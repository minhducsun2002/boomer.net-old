using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using dotenv.net;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pastel;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.Services.FGO;
using Pepper.Services.Monitoring;
using Pepper.Services.Monitoring.Log;
using Pepper.External.Osu;
using Pepper.Utilities;

using Color = System.Drawing.Color;

namespace Pepper
{
    class Pepper
    {
        private readonly PepperClient _client = new PepperClient();
        private IServiceProvider _services;

        public static void Main() => new Pepper().Entry().GetAwaiter().GetResult();
        
        private async Task Entry()
        {
            // construct service instances
            DotEnv.Config(false);
            _services = ConfigureServices();

            // initialize services
            _services.GetRequiredService<Services.Main.CommandService>().PrepareCommands();
            _services.GetRequiredService<MasterDataService>().PrintMasterDataStatistics();
            _services.GetRequiredService<TraitService>().LoadMapping();

            // log into Discord
            _client.Ready += OnReadyEvent;
            
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await _client.StartAsync();
            await Task.Delay(-1);
        }
    
        private Task OnReadyEvent()
        {
            // unregister handler
            _client.Ready -= OnReadyEvent;
            
            var entry = new LogEntry
            {
                Tags = new[] {new LogTag {Name = "Client", ForegroundColor = "#000", BackgroundColor = "#00fff7"}},
                Content = (
                    $"I am now logged in as {$"{_client.CurrentUser}".Pastel(Color.Yellow).PastelBg(Color.Blue)}."
                    + $"\nI will serve in {_client.Guilds.Count} guild{StringUtilities.Plural(_client.Guilds.Count)}."
                    + "\n"
                    + string.Join(
                        "\n",
                        _client.Guilds.Select(
                            guild => (
                                $"* {guild.Id.ToString().Pastel(Color.Black).PastelBg(Color.Cyan)} "
                                + "=>".Pastel(Color.Green)
                                + $" {guild.Name.Pastel(Color.Black).PastelBg(Color.Yellow)}"
                                + $"\n    {"@".Pastel(Color.Magenta)} {guild.Owner.ToString().Pastel(Color.Black).PastelBg(Color.OrangeRed)}"
                                + $" ({guild.OwnerId})"
                            )
                        )
                    )
                )
            };
            _services.GetRequiredService<LogService>().Write(LogType.Success, entry);
            return Task.CompletedTask;
        }

        private static PepperConfiguration Configure()
        {
            var cwd = Directory.GetCurrentDirectory();
            var configFile = "config/production.json";
            return JsonConvert.DeserializeObject<PepperConfiguration>(File.ReadAllText(Path.Join(cwd, configFile)));
        }
        
        private IServiceProvider ConfigureServices()
        {
            var commandService = new CommandService(new CommandServiceConfiguration
                {
                    DefaultRunMode = RunMode.Parallel,
                    IgnoresExtraArguments = true,
                    StringComparison = StringComparison.InvariantCultureIgnoreCase,
                    DefaultArgumentParser = new PepperArgumentParser()
                }
            );
            var interactivity = new InteractivityService(_client);
            var osuClient = new OsuClient(Environment.GetEnvironmentVariable("OSU_API_KEY"));
            return new ServiceCollection()
                .AddSingleton(Configure())
                .AddSingleton(_client)
                .AddSingleton<TraitService>()
                .AddSingleton<LogService>()
                .AddSingleton<MasterDataService>()
                .AddSingleton(commandService)
                .AddSingleton(interactivity)
                .AddSingleton<Services.Main.CommandService>()
                .AddSingleton(osuClient)
                .BuildServiceProvider();
        }
    }
}
