using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.External.Osu;
using Beatmap = osu.Game.Beatmaps.Beatmap;
using Decoder = osu.Game.Beatmaps.Formats.Decoder;

namespace Pepper.Utilities.Osu
{
    public class ProcessorWorkingBeatmap : WorkingBeatmap
    {
        private readonly Beatmap _beatmap;
        internal ProcessorWorkingBeatmap(Beatmap beatmap, int? beatmapId = null) : base(beatmap.BeatmapInfo, null)
        {
            _beatmap = beatmap;

            beatmap.BeatmapInfo.Ruleset = ParsingUtilities.GetRulesetFromRulesetId((GameMode) beatmap.BeatmapInfo.RulesetID).RulesetInfo;
            if (beatmapId.HasValue) beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
        }

        protected override IBeatmap GetBeatmap() => _beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetBeatmapTrack() => null;
    }
    
    public static class ParsingUtilities
    {
        public static ProcessorWorkingBeatmap GetWorkingBeatmap(string inputOsuFile)
        {
            using var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(inputOsuFile));
            using var streamReader = new LineBufferedReader(fileStream);
            var beatmap = Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
            return new ProcessorWorkingBeatmap(beatmap);
        }
        
        public static Ruleset GetRulesetFromRulesetId(GameMode id)
        {
            switch (id)
            {
                case GameMode.Osu: return new OsuRuleset();
                case GameMode.Taiko: return new TaikoRuleset();
                case GameMode.Catch: return new CatchRuleset();
                case GameMode.Mania: return new ManiaRuleset();
                default: throw new ArgumentException($"Ruleset id {id} is not supported!");
            }
        }

        public static PerformanceCalculator GetPerformanceCalculator(GameMode rulesetId, DifficultyAttributes beatmapAttributes, ScoreInfo score)
        {
            switch (rulesetId)
            {
                case GameMode.Osu: return new OsuPerformanceCalculator(GetRulesetFromRulesetId(rulesetId), beatmapAttributes, score);
                case GameMode.Taiko: return new TaikoPerformanceCalculator(GetRulesetFromRulesetId(rulesetId), beatmapAttributes, score);
                case GameMode.Catch: return new CatchPerformanceCalculator(GetRulesetFromRulesetId(rulesetId), beatmapAttributes, score);
                case GameMode.Mania: return new ManiaPerformanceCalculator(GetRulesetFromRulesetId(rulesetId), beatmapAttributes, score);
                default: throw new ArgumentException($"Ruleset id {rulesetId} is not supported!");
            }
        }

        public static PerformanceScoreUtils GetPerformanceScoreUtilsFromRulesetId(GameMode rulesetId)
        {
            switch (rulesetId)
            {
                case GameMode.Osu: return new OsuPerformanceScoreUtils();
                case GameMode.Taiko: return new TaikoPerformanceScoreUtils();
                case GameMode.Catch: return new CatchPerformanceScoreUtils();
                case GameMode.Mania: return new ManiaPerformanceScoreUtils();
                default: throw new ArgumentException($"Ruleset id {rulesetId} is not supported!");
            }
        }

        public static double CalculateScoreAccuracy(
            Ruleset ruleset,
            int count50, int count100, int count300, int countMiss, int countGeki, int countKatu
        )
        {
            var scoreInfo = new ScoreInfo { Ruleset = ruleset.RulesetInfo };
            scoreInfo.SetCount50(count50);
            scoreInfo.SetCount100(count100);
            scoreInfo.SetCount300(count300);
            scoreInfo.SetCountGeki(countGeki);
            scoreInfo.SetCountKatu(countKatu);
            scoreInfo.SetCountMiss(countMiss);

            return new ScoreDecoder().CalculateAccuracy(scoreInfo);
        }

        public static Mod[] ModsFromModbits(Ruleset ruleset, LegacyMods mods)
        {
            if ((mods & LegacyMods.Nightcore) == LegacyMods.Nightcore)
                mods |= LegacyMods.DoubleTime;
            return ruleset.ConvertFromLegacyMods(mods).ToArray();
        }
        
        public static LegacyMods LegacyModsFromModString(Ruleset ruleset, IEnumerable<string> inputMods)
        {
            var mods = ModsFromModString(ruleset, inputMods);
            var legacyMod = ruleset.ConvertToLegacyMods(mods.ToArray());
            if ((legacyMod & LegacyMods.Nightcore) == LegacyMods.Nightcore)
                legacyMod |= LegacyMods.DoubleTime;
            return legacyMod;
        }
        
        public static Mod[] ModsFromModString(Ruleset ruleset, IEnumerable<string> inputMods)
        {
            var mods = new List<Mod>();
            var modStrings = inputMods as string[] ?? inputMods.ToArray();
            if (!modStrings.Any()) return new Mod[0];

            var availableMods = ruleset.GetAllMods().ToList();
            foreach (var modString in modStrings)
            {
                var resolvedMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.InvariantCultureIgnoreCase));
                if (resolvedMod == null) continue;
                mods.Add(resolvedMod);
            }

            return mods.ToArray();
        }
    }
}