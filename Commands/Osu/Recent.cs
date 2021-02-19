using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.External.Osu;
using Pepper.Utilities;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Recent : PepperCommand
    {
        public OsuClient Client { get; set; }

        [Command("recent", "rp")]
        [PrefixCategory("osu")]
        [Category("osu!")]
        [Description("Show recent plays of an user.")]
        public async Task Exec(
            [Flag("/")] GameMode gameMode = GameMode.Osu,
            [Flag("/limit=")] int limit = 20,
            [Flag("/failed", "/f")] bool failed = false,
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

            if (limit < 1 || limit > 50) limit = 20;

            var userLink = $"https://osu.ppy.sh/users/{user.Id}";
            var fields = await (failed ? RecentApiMode(user, gameMode, limit) : RecentNormalMode(user, gameMode, limit));
            
            var fieldChunks = fields.ChunkBy(5).ToArray();
            var embeds = fieldChunks
                .Select((fieldChunk, index) =>
                    new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name = user.Username,
                                Url = userLink,
                                IconUrl = user.AvatarUrl
                            },
                            Title = $"Recent plays",
                            Url = userLink,
                            Fields = fieldChunk.ToList(),
                            Footer = new EmbedFooterBuilder
                            {
                                Text = $"Page {index + 1}/{fieldChunks.Length} | All times are in UTC"
                            }
                        }
                        .Build()
                )
                .ToArray();
            await HandlePagedOutput(
                embeds,
                new EmbedBuilder()
                    .WithDescription($"No recent play found for user [**{username}**](${userLink})")
                    .Build()
            );
        }

        private async Task<EmbedFieldBuilder[]> RecentNormalMode(External.Osu.User user, GameMode gameMode, int limit)
        {
            var scores = await OsuClient.GetRecent(user.Id, gameMode, limit, Math.Min(limit, 50));
            return scores
                .Select(score => SerializationUtilities.SerializeScoreToEmbedField(score, gameMode))
                .ToArray();
        }
        
        private async Task<EmbedFieldBuilder[]> RecentApiMode(External.Osu.User user, GameMode gameMode, int limit)
        {
            var recents = await Client.GetRecentPlay(user.Username, gameMode, limit);
            return recents
                .Select(score => SerializationUtilities.SerializeScoreToEmbedField(score, gameMode))
                .Select(scoreTask => scoreTask.Result)
                .ToArray();
        }
    }
}