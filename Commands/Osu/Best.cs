using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using osu.Game.Beatmaps.Legacy;
using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.External.Osu;
using Pepper.Utilities;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Best : PepperCommand
    {

        [Command("best", "top")]
        [PrefixCategory("osu")]
        [Category("osu!")]
        [Description("Show top plays of an user.")]
        public async Task Exec(
            [Flag("/")] GameMode gameMode = GameMode.Osu,
            [Flag("/limit=", "/limit:")] int limit = 50,
            [Flag("/mod=", "/mod:")] string mods = "",
            [Remainder] string username = ""
        )

        {
            External.Osu.User user;
            try
            {
                user = (await OsuClient.GetUser(username, gameMode)).User;
            }
            catch
            {
                throw new ArgumentException($"Error determining user ID of user {username}");
            }

            if (limit < 0 || limit > 50) limit = 20;

            var ruleset = ParsingUtilities.GetRulesetFromRulesetId(gameMode);
            var scores = await OsuClient.GetBest(user.Id, gameMode, limit, Math.Min(limit, 50));
            
            var modFilterStrings = mods.ChunkBy(2)
                .Select(chunk => new string(chunk.ToArray()))
                .ToArray();
            var modFilterBitmask  = ParsingUtilities.LegacyModsFromModString(ruleset, modFilterStrings);
            var modFilterCleanString = string.Join(
                "",
                ParsingUtilities.ModsFromModString(ruleset, modFilterStrings)
                    .Select(mod => mod.Acronym)
            );
            
            var scoreChunks = scores
                // filtering for mods 
                .Where(score =>
                {
                    if (modFilterBitmask == 0) return true;
                    var scoreModBitmask = ParsingUtilities.LegacyModsFromModString(ruleset, score.Mods);
                    return (modFilterBitmask & scoreModBitmask) == modFilterBitmask;
                })
                .Select(score => SerializationUtilities.SerializeScoreToEmbedField(score, gameMode))
                .ToList().ChunkBy(5).ToArray();
            var userLink = $"https://osu.ppy.sh/users/{user.Id}";
            var embeds = scoreChunks
                .Select((chunk, index) => new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = user.Username,
                        Url = userLink,
                        IconUrl = user.AvatarUrl
                    },
                    Title = $"Best performance"
                            + (modFilterBitmask == 0 ? "" : $" with {string.Join("", modFilterCleanString)}"),
                    Url = userLink,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = $"Page {index + 1}/{scoreChunks.Length} | All times are UTC",
                    },
                    Fields = chunk.ToList()
                }.Build());
            
            await HandlePagedOutput(
                embeds.ToArray(),
                new EmbedBuilder
                {
                    Description = $"No top play found for user [**{username}**](https://osu.ppy.sh/users/{user.Id})"
                }.Build());
        }
    }
}