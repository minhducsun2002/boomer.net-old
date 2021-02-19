using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.External.Osu;
using Pepper.External.Osu.LegacyApi;
using Pepper.External.Osu.UserExtra;
using Score = Pepper.External.Osu.UserExtra.Score;

namespace Pepper.Utilities.Osu
{
    public static class SerializationUtilities
    {
        public static string SerializeTimeLength(uint time)
            => $"{time / 60}".PadLeft(2, '0') + ":" + $"{time % 60}".PadLeft(2, '0');

        public static string SerializeTimeLengthFriendly(uint time)
        {
            uint minute = time / 60, second = time % 60;
            var ret = $"{second} second{(second > 1 ? "s" : "")}";
            if (minute > 0)
                ret = $"{minute} minute{(minute > 1 ? "s" : "")} " + ret;
            return ret;
        }
        
        public static string SerializeStats(ScoreBeatmap map)
            => $"`AR`**{map.ApproachRate}** `CS`**{map.CircleSize}** `OD`**{map.OverallDifficulty}** `HP`**{map.DrainRate}**";

        public static async Task<EmbedFieldBuilder> SerializeScoreToEmbedField(RecentPlay score, GameMode gameMode)
        {
            var beatmapset = await OsuClient.GetMapset(score.BeatmapId);
            var beatmap = beatmapset.Beatmaps.Concat(beatmapset.Converts).FirstOrDefault(map => map.Id == score.BeatmapId);

            var ruleset = ParsingUtilities.GetRulesetFromRulesetId(gameMode);
            var mods = ParsingUtilities.ModsFromModbits(ruleset, score.EnabledMods);

            var accuracy = ParsingUtilities.GetPerformanceScoreUtilsFromRulesetId(gameMode)
                .CalculateScoreAccuracy(
                    ruleset,
                    score.Count50, score.Count100, score.Count300, score.CountMiss, score.CountGeki, score.CountKatu
                );
            
            return new EmbedFieldBuilder
            {
                Name = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.Version}]"
                       + (mods.Length > 0 ? $"+{string.Join("", mods.Select(mod => mod.Acronym.ToUpperInvariant()))}" : ""),
                Value = $"[**{score.Rank}**] **{accuracy * 100:F3}**% ({score.Count300}/{score.Count100}/{score.Count50}/{score.CountMiss})"
                        + $" - **{score.MaxCombo}**x{(score.Perfect == 1 ? " (FC)" : "")}"
                        + $"\n@ **{score.Date.ToUniversalTime().ToString("dd/MM/yyyy, hh:mm:ss tt 'UTC'", CultureInfo.InvariantCulture)}**"
                        + $"\n{beatmap.StarRating} :star: - {SerializeStats(beatmap)} - **{beatmap.BeatPerMinute}** BPM"
                        + $"\n[[**Beatmap**]](https://osu.ppy.sh/b/{beatmap.Id})"
            };
        }
        
        public static EmbedFieldBuilder SerializeScoreToEmbedField(Score score, GameMode gameMode)
        {
            var beatmapset = score.Beatmapset;
            var beatmap = score.Beatmap;

            var counts = score.Statistics;
            return new EmbedFieldBuilder
            {
                Name = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.Version}]"
                       + (score.Mods.Length > 0 ? $"+{string.Join("", score.Mods)}" : ""),
                Value = $"[**{score.Rank}**] "
                        + (score.PerformancePoint > 0
                            ? $"**{score.PerformancePoint}**pp (**{score.Accuracy * 100:F3}**% | **{score.MaxCombo}**x)"
                            : $"**{score.Accuracy * 100:F3}**% - **{score.MaxCombo}**x")
                        + (score.Perfect ? " (FC)" : "")
                        + $"\n{beatmap.StarRating} :star: - {SerializeStats(beatmap)} - **{beatmap.BeatPerMinute}** BPM"
                        + $"\n[**{counts.Count300}**/**{counts.Count100}**/**{counts.Count50}**/**{counts.CountMiss}**]"
                        + $" @ **{score.CreatedAt.ToUniversalTime().ToString("dd/MM/yyyy, hh:mm:ss tt 'UTC'", CultureInfo.InvariantCulture)}**"
                        + $"\n[[**Beatmap**]](https://osu.ppy.sh/b/{beatmap.Id})"
                        + $" [[**Score**]](https://osu.ppy.sh/scores/{gameMode.ToUrlPath()}/{score.Id})"
            };
        }
    }
}