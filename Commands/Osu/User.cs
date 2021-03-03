using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.External.Osu;
using Pepper.Utilities.Osu;

namespace Pepper.Commands.Osu
{
    public class User : PepperCommand
    {
        [Command("user", "u")]
        [PrefixCategory("osu")]
        [Category("osu!")]
        [Description("Show information about a player.")]
        public async Task Exec(
            [Flag("/")] GameMode gameMode = GameMode.Osu,
            [Remainder] string username = ""
        )
        {
            var (user, extra) = await OsuClient.GetUser(username, gameMode);
            var stats = user.Statistics;

            // check for global ranks
            // if the rank equals the default value, it means the field hasn't been assigned (null values in the source JSON)
            var rank = stats.GlobalRank.Equals(default)
                ? ""
                : $" (#**{stats.GlobalRank}** globally | #**{stats.CountryRank}** in :flag_{user.Country.Code.ToLowerInvariant()}:)";
            var grades = stats.GradeCounts; 
            var playTime = new TimeSpan((long) stats.PlayTime * (long) 1e7);

            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Scores",
                    Value = $"{stats.RankedScore} ranked\n{stats.TotalScore} total",
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Ranks",
                    Value =
                        $"**{grades.XH}** XH | **{grades.X}** X\n**{grades.SH}** SH | **{grades.S}** S\n**{grades.A}** A",
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Play time",
                    Value = $"{stats.PlayCount} times | "
                            + $"{playTime.Days / 7}w {playTime.Days % 7}d {playTime.Hours}h {playTime.Minutes}m {playTime.Seconds}s"
                }
            };

            // users might not have a top score
            if (extra.BestScores.Length > 0)
            {
                var score = extra.BestScores[0];
                var map = score.Beatmap;
                var mapset = score.Beatmapset;
                
                fields.Add(
                    new EmbedFieldBuilder
                    {
                        Name = "Best performance",
                        Value = $"[**{score.Rank}**] **{score.PerformancePoint}**pp "
                            + $"(**{(score.Accuracy * 100):0.000}**% | **{score.MaxCombo}**x)" + (score.Perfect ? " (FC)" : "")
                            + $"\n[{mapset.Artist} - {mapset.Title} [{map.Version}]](https://osu.ppy.sh/beatmaps/{map.Id})"
                            + (score.Mods.Length > 0 ? $"+{string.Join("", score.Mods)}" : "")
                            + $"\n{map.StarRating} :star: - "
                            + $"{SerializationUtilities.SerializeStats(map)}"
                    }
                );
            }


            await Context.Channel.SendMessageAsync(
                "",
                false,
                new EmbedBuilder
                    {
                        Title = $"[{stats.Level.Current}] {user.Username}",
                        Url = $"https://osu.ppy.sh/users/{user.Id}",
                        ThumbnailUrl = Uri.IsWellFormedUriString(user.AvatarUrl, UriKind.Absolute) ? user.AvatarUrl : null,
                        Color = Color.Green,
                        Description = (
                                $"**{stats.PerformancePoints}**pp{rank}."
                                + $"\nTotal accuracy : **{stats.HitAccuracy:0.000}%** | Max combo : **{stats.MaximumCombo}**x" 
                                + $"\nJoined {user.JoinedDate.ToUniversalTime().ToString("dd/MM/yyyy, hh:mm:ss tt 'UTC'", CultureInfo.InvariantCulture)}."
                            ),
                        Fields = fields
                    }
                    .Build()
            );
        }
    }
}