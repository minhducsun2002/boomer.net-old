using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Qmmands;
using QmmandsCommandService = Qmmands.CommandService;

using Pepper.Classes.Command;
using Pepper.Classes.Command.Result;
using Pepper.Classes;
using Pepper.Services.Monitoring;
using Pepper.Services.Monitoring.Log;
using Pepper.External.Osu;
using Pepper.Utilities;

namespace Pepper.Services.Main
{
    public class CommandService
    {
        // dependencies
        private readonly PepperClient _client;
        public readonly QmmandsCommandService InnerCommandService;
        private readonly IServiceProvider _services;
        private readonly LogService _log;
        private readonly InteractivityService _interactivityService;

        // command prefixes
        // prefix => prefixCategory[]
        private readonly ImmutableDictionary<string, string[]> _prefixes;
        public readonly ImmutableDictionary<string, string[]> PrefixConfiguration;
        public ImmutableDictionary<string, Command[]> CommandsByCategory { get; private set; }

        public CommandService(IServiceProvider serv)
        {
            /*
             * Populate required fields
             */
            
            _services = serv;
            _client = serv.GetRequiredService<PepperClient>();
            InnerCommandService = serv.GetRequiredService<QmmandsCommandService>();
            PrefixConfiguration = serv.GetRequiredService<PepperConfiguration>().Prefix.ToImmutableDictionary();
            _log = serv.GetRequiredService<LogService>();
            _interactivityService = serv.GetRequiredService<InteractivityService>();

            /*
             * prepare command prefixes
             */

            // convert this from (category - prefix[]) to a list of (prefix -> category)
            var mappings = PrefixConfiguration
                .SelectMany(pair =>
                {
                    var (prefixCategory, prefixes) = pair;
                    return prefixes.Select(prefix => (prefix, prefixCategory));
                });

            // build a dictionary
            var prefixToCategoryList = new Dictionary<string, List<string>>();
            foreach (var (prefix, prefixCategory) in mappings)
                if (!prefixToCategoryList.ContainsKey(prefix))
                    prefixToCategoryList[prefix] = new List<string> {prefixCategory};
                else
                    prefixToCategoryList[prefix].Add(prefixCategory);
            
            // convert into an immutable dictionary
            _prefixes = prefixToCategoryList
                .Select(pair => new KeyValuePair<string, string[]>(pair.Key, pair.Value.ToArray()))
                .ToImmutableDictionary();
            
            /*
             * Other registrations
             */
            
            // register handlers
            InnerCommandService.CommandExecutionFailed += HandleErrors;
            InnerCommandService.CommandExecuted += HandleResult;

            // register parsers
            InnerCommandService.AddTypeParser(GameModeTypeParser.Instance, true);
        }

        public void PrepareCommands()
        {
            // load commands
            InnerCommandService.AddModules(Assembly.GetEntryAssembly());
            // sort into category
            var sortedCategory = new Dictionary<string, List<Command>>();
            foreach (var command in InnerCommandService.GetAllCommands())
                if (sortedCategory.ContainsKey(command.Category()))
                    sortedCategory[command.Category()].Add(command);
                else
                    sortedCategory[command.Category()] = new List<Command> {command};
            CommandsByCategory = sortedCategory
                .Select(pair => new KeyValuePair<string, Command[]>(pair.Key, pair.Value.ToArray()))
                .ToImmutableDictionary();
            
            var commandCount = InnerCommandService.GetAllCommands().Count;
            _log.Write(
                LogType.Success,
                new LogEntry
                {
                    Tags = new [] { new LogTag { Name = "Commands", ForegroundColor = "#000", BackgroundColor = "#ff70ff" } },
                    Content = $"Discovered {commandCount} command{StringUtilities.Plural(commandCount)}."
                }
            );

            // registering handler
            _client.MessageReceived += msg => Task.Run(() => HandleMessage(msg));
        }

        /// <summary>
        ///     Handle incoming messages from Discord.
        ///     Check if a message is a valid (from another bot/user) command invocation &amp; invoke the matching command.
        /// </summary>
        /// <param name="socketIncomingMsg">Message to handle</param>
        public async Task HandleMessage(SocketMessage socketIncomingMsg)
        {
            // ignore system messages
            if (!(socketIncomingMsg is SocketUserMessage msg)) return;
            
            // ignore self messages
            if (msg.Author.Id == _client.CurrentUser.Id) return;
            
            // check for prefixes
            var execTargetPosition = 0;
            var prefix = "";
            var matchingCategories = _prefixes
                .FirstOrDefault(
                    mapping =>
                    {
                        // check if matches prefix
                        execTargetPosition = 0;
                        prefix = mapping.Key;
                        return msg.HasStringPrefix(prefix, ref execTargetPosition,
                            // ignore case
                            StringComparison.InvariantCultureIgnoreCase);
                    }
                )
                /*
                 * the value (in this case, a list of applicable categories)
                 * is nullable - we might be deconstructing from the default value of KeyValuePair.
                 * 
                 * However, a prefix must live under a prefixCategory in the configuration file to be able to appear here.
                 * As such, it is possible to assume that a prefix will always map to a list of categories.
                 */
                .Value;

            // search for command
            var alias = msg.Content.Substring(execTargetPosition).Split(new[] {' '}, 2)[0];
            var commandMatches = InnerCommandService.FindCommands(alias);
            if (!commandMatches.Any()) return;
            
            // check for category matches
            var applicableCommand = commandMatches
                .FirstOrDefault(
                    match =>
                    {
                        var prefixCategory = (PrefixCategoryAttribute)
                            match.Command.Attributes.FirstOrDefault(attrib => attrib is PrefixCategoryAttribute);
                        return
                            // On commands that specify no category,
                            // the default behavior is to accept all prefixes.
                            prefixCategory == null || matchingCategories.Any(category => prefixCategory.PrefixCategory == category);
                    }
                );

            // execute command
            if (applicableCommand == null) return;

            var result = await InnerCommandService.ExecuteAsync(
                applicableCommand.Command,
                msg.Content.Substring(execTargetPosition + alias.Length),
                new PepperCommandContext(_services)
                {
                    Client = _client,
                    CommandService = this,
                    Channel = msg.Channel,
                    Message = msg,
                    Author = msg.Author,
                    Prefix = prefix,
                    Alias = alias
                }
            );

            if (result is ArgumentParseFailedResult parseFailedResult)
                await msg.Channel.SendMessageAsync(
                    "",
                    false,
                    new EmbedBuilder
                        {
                            Title = $"Error occurred executing command `{parseFailedResult.Command.Name}` :",
                            Description = $"```{parseFailedResult.Reason}```"
                        }
                        .Build()
                );
        }

        private async Task HandleResult(CommandExecutedEventArgs executedEventArgs)
        {
            var result = executedEventArgs.Result;
            var context = executedEventArgs.Context as PepperCommandContext;
            switch (result)
            {
                case EmbedResult embedResult:
                {
                    var embeds = embedResult.Embeds;
                    if (embeds.Length > 1)
                        await _interactivityService.SendPaginatorAsync(
                            EmbedUtilities.PagedEmbedBuilder()
                                .WithPages(embeds.Select(PageBuilder.FromEmbed))
                                .Build(),
                            context.Channel,
                            TimeSpan.FromSeconds(20)
                        );
                    
                    else
                        await context.Channel.SendMessageAsync("", false, embeds.Any() ? embeds[0] : embedResult.NoEmbed);
                    
                    break;
                }
            }
        }
        
        private static async Task HandleErrors(CommandExecutionFailedEventArgs failed)
        {
            var context = (PepperCommandContext) failed.Context;
            var result = failed.Result;
            await context.Channel.SendMessageAsync(
                "",
                true,
                EmbedUtilities.ErrorEmbedBuilder()
                    .WithTitle($"Apologize, there was an error trying to execute command `{result.Command.Name}` : ")
                    .WithDescription($"```{result.Reason}```\n```{result.Exception.Message}\n{result.Exception.StackTrace}```")
                    .WithTimestamp(DateTimeOffset.Now)
                    .Build()
            );
        }
    }
}
