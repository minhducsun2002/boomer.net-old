using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using Qmmands;
using Pepper.Classes.Command;
using Pepper.Utilities;

namespace Pepper.Classes
{
    public abstract class PepperCommand : ModuleBase<PepperCommandContext>
    {
        public InteractivityService InteractivityService { get; set; }
        protected async Task HandlePagedOutput(Embed[] embeds, Embed noEmbed)
        {
            if (embeds.Length > 1)
            {
                var paginator = EmbedUtilities.PagedEmbedBuilder()
                    .WithPages(embeds.Select(PageBuilder.FromEmbed))
                    .Build();
                await InteractivityService.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromSeconds(20));
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, embeds.Any() ? embeds[0] : noEmbed);
            }
        }
    }

    namespace Command
    {
        /// <summary>
        ///     Specify the category for this command to be used when determining prefixes.
        /// </summary>
        /// <remarks>
        ///     The configuration file specifies prefixes for commands grouped under certain categories.
        ///     To set the category of a command to be used for determining prefixes, use this attribute.
        ///     This has nothing to do with the Category attribute.
        /// </remarks>
        public class PrefixCategoryAttribute : Attribute
        {
            public string PrefixCategory { get; }
            public PrefixCategoryAttribute(string category)
            {
                PrefixCategory = category;
            }
        }

        /// <summary>
        ///     Specify the category for this command to be used when determining prefixes.
        /// </summary>
        /// <remarks>
        ///     The configuration file specifies prefixes for commands grouped under certain categories.
        ///     Upon execution request, this property may be checked to see if it's allowed to execute with a certain prefix.
        /// </remarks>
        public class CategoryAttribute : Attribute
        {
            public string Category { get; }
            public CategoryAttribute(string category)
            {
                Category = category;
            }
        }

        /// <summary>
        /// Used on arguments to specify flags (a certain prefix before parameter values).
        /// </summary>
        /// <example>>
        /// <code>
        /// public async Task Execute(string arg1, [Flag("/f=")] string arg2) { ... }
        /// </code>
        /// In that case, if the user calls the command with the raw arguments <code>value1 /f=value2</code>,
        /// <code>arg1</code> would be value1, and <code>arg2</code> would be value2. 
        /// </example>
        public class FlagAttribute : Attribute
        {
            public string[] Flags { get; }

            public FlagAttribute(params string[] flags)
            {
                Flags = flags;
            }
        }
    }
}