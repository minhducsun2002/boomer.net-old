using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;

namespace Pepper.Utilities.Osu
{
    public abstract class PerformanceScoreUtils
    {
        public abstract Dictionary<HitResult, int> PrepareHitResults(
            double accuracy,
            IEnumerable<HitObject> hitObjects,
            int miss,
            int? meh = null, int? good = null, int? katu = null
        );
        
        public double CalculateScoreAccuracy(
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
    }
    
    internal class ScoreDecoder : LegacyScoreDecoder
    {
        protected override Ruleset GetRuleset(int rulesetId) => null;
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => null;

        public new double CalculateAccuracy(ScoreInfo score)
        {
            base.CalculateAccuracy(score);
            return score.Accuracy;
        }
    }
}