using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Scoring;
using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.External.Osu;
using Pepper.External.Osu.UserExtra;
using Pepper.Utilities;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class PP : PepperCommand
    {
        [Command("pp")]
        [Category("osu!")]
        public async Task Exec(
            ulong mapId,
            [Flag("/mod:", "/mod=")] string mods = "",
            double accuracy = 100.0,
            // 0 effectively means max combo 
            int combo = 0,
            int miss = 0
        )
        {
            var rawBeatmapFile = await OsuClient.GetBeatmapFile(mapId);
            var workingBeatmap = ParsingUtilities.GetWorkingBeatmap(rawBeatmapFile);
            
            var rulesetId = (GameMode) workingBeatmap.BeatmapInfo.RulesetID;
            
            var beatmapRuleset = ParsingUtilities.GetRulesetFromRulesetId(rulesetId);
            var modList = ParsingUtilities.ModsFromModString(
                beatmapRuleset,
                mods.ChunkBy(2).Select(m => string.Join("", m)).ToArray()
            );
            
            var beatmapDifficultyAttributes = beatmapRuleset.CreateDifficultyCalculator(workingBeatmap).Calculate(modList);
            
            var maxCombo = combo < 1 ? beatmapDifficultyAttributes.MaxCombo : Math.Min(combo, beatmapDifficultyAttributes.MaxCombo);
            miss = miss >= 0 ? miss : 0;
            var performanceCalculator = ParsingUtilities.GetPerformanceCalculator(
                rulesetId,
                beatmapDifficultyAttributes,
                new ScoreInfo
                {
                    Mods = modList,
                    MaxCombo = maxCombo,
                    Statistics = ParsingUtilities
                        .GetPerformanceScoreUtilsFromRulesetId(rulesetId)
                        .PrepareHitResults(accuracy / 100, workingBeatmap.Beatmap.HitObjects, miss),
                    Accuracy = accuracy / 100,
                }
            );

            var beatmapInfo = workingBeatmap.BeatmapInfo;
            var beatmapMeta = beatmapInfo.Metadata;

            var baseDifficulty = new ScoreBeatmap
            {
                CircleSize = beatmapInfo.BaseDifficulty.CircleSize,
                DrainRate = beatmapInfo.BaseDifficulty.DrainRate,
                OverallDifficulty = beatmapInfo.BaseDifficulty.OverallDifficulty,
                ApproachRate = beatmapInfo.BaseDifficulty.ApproachRate
            };
            
            // aim rating + speed rating for osu!standard
            var extraStarRating = "";
            if (beatmapDifficultyAttributes is OsuDifficultyAttributes diffAttributes)
                extraStarRating =
                    $"(**{diffAttributes.AimStrain:F3}** aim | **{diffAttributes.SpeedStrain:F3}** speed)";
            
            var mapStats = SerializationUtilities.SerializeStats(baseDifficulty);
            
            await Context.Channel.SendMessageAsync(
                "",
                false,
                new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder
                        {
                            Name = beatmapMeta.Author.Username 
                        },
                        ImageUrl = $"https://assets.ppy.sh/beatmaps/{beatmapInfo.BeatmapSet.ID}/covers/cover@2x.jpg",
                        Title = $"{beatmapMeta.Artist} - {beatmapMeta.TitleUnicode ?? beatmapMeta.Title} [{beatmapInfo.Version}]",
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = $@"Difficulty {
                                    (modList.Length > 0
                                        ? $"(before {string.Join("", modList.Select(mod => mod.Acronym.ToUpperInvariant()))})"
                                        : "")   
                                }",
                                Value = extraStarRating.Length > 0
                                    ? $"{mapStats}\n**{beatmapDifficultyAttributes.StarRating:F3}**★ {extraStarRating}"
                                    : $"**{beatmapDifficultyAttributes.StarRating:F3}**★ | {mapStats}"
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "PP",
                                Value = $"{performanceCalculator.Calculate():F4}",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Statistics",
                                Value = string.Join(
                                    " - ",
                                    new List<string>
                                    {
                                        $@"**{maxCombo}**x{(
                                            maxCombo == beatmapDifficultyAttributes.MaxCombo
                                                ? $"/**{beatmapDifficultyAttributes.MaxCombo}**x"
                                                : " (FC)"
                                        )}",
                                        $"**{accuracy / 100 * 100:F2}**%",
                                        $"{miss} miss{StringUtilities.Plural(miss, "es")}"
                                    }
                                ),
                                IsInline = true
                            }
                        }
                    }
                    .Build()
            );
        }
    }
}