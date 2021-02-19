using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FuzzySharp;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.Utilities;

namespace Pepper.Commands
{
    public class Help : PepperCommand
    {
        [Command("help")]
        [Category("General")]
        [Description("Where everything starts.")]
        public async Task Exec([Remainder] string query = "")
        {
            var commandService = Context.CommandService;
            // try to parse as category
            var categories = commandService.CommandsByCategory.Keys.ToArray();
            var categoryMatches = Process.ExtractTop(
                query.ToLowerInvariant(),
                categories,
                s => s.ToLowerInvariant(),
                limit: 1
            ).ToArray();

            if (categoryMatches[0].Score >= 60)
            {
                HandleCategory(categoryMatches[0].Value);
                return;
            }

            HandleCategoryList();
        }

        private async void HandleCategory(string category)
        {
            var commands = Context.CommandService.CommandsByCategory[category];
            var prefixConfiguration = Context.CommandService.PrefixConfiguration;
            await Context.Channel.SendMessageAsync(
                "",
                false,
                new EmbedBuilder
                    {
                        Description = $"The following command{StringUtilities.Plural(commands.Length)} belong to the **{category}** category :",
                        Fields = commands.Select(command =>
                        {
                            var applicablePrefixes = prefixConfiguration.ContainsKey(command.PrefixCategory())
                                ? prefixConfiguration[command.PrefixCategory()]
                                : new [] {Context.Prefix};
                            var mainPrefix = applicablePrefixes[0];
                            return new EmbedFieldBuilder
                            {
                                Name = $"`{mainPrefix}{command.Aliases[0]}`"
                                       + (command.Aliases.Count > 1
                                           ? $@" ({string.Join(
                                               ", ",
                                               command.Aliases.Skip(1).Select(al => $"`{mainPrefix}{al}`")
                                           )})"
                                           : ""),
                                Value = $"{(string.IsNullOrWhiteSpace(command.Description) ? "N/A" : command.Description)}"
                            };
                        }).ToList()
                    }
                    .Build()
            );
        }

        private async void HandleCategoryList()
        {
            var categories = Context.CommandService.CommandsByCategory;
            await Context.Channel.SendMessageAsync(
                "",
                false,
                new EmbedBuilder
                    {
                        Description = $"Run **`{Context.Prefix}{Context.Alias}`** again with "
                                      + "a **category name** or **command alias** to get respective info."
                                      + $"\nAvailable categor{StringUtilities.PluralY(categories.Count)} :",
                        Fields = categories.Select(field =>
                            new EmbedFieldBuilder
                            {
                                Name = field.Key,
                                Value = $"{field.Value.Length} command{StringUtilities.Plural(field.Value.Length)}",
                                IsInline = true
                            }).ToList()
                    }
                    .Build()
            );
        }
    }
}