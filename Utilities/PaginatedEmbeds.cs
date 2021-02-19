using System.Collections.Generic;
using Discord;
using Interactivity;
using Interactivity.Pagination;

namespace Pepper.Utilities
{
    static class EmbedUtilities
    {
        public static StaticPaginatorBuilder PagedEmbedBuilder() => new StaticPaginatorBuilder()
            .WithEmotes(
                new Dictionary<IEmote, PaginatorAction>
                {
                    {new Emoji("\u2B05"), PaginatorAction.Backward},
                    {new Emoji("\u27A1"), PaginatorAction.Forward}
                })
            .WithCancelledEmbed(null)
            .WithTimoutedEmbed(null)
            .WithDeletion(DeletionOptions.None)
            .WithFooter(PaginatorFooter.None);

        public static EmbedBuilder ErrorEmbedBuilder() => new EmbedBuilder
        {
            Color = Color.Red
        };
    }
}