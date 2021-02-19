using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace Pepper.Utilities.Osu
{
    public class OsuPerformanceScoreUtils : PerformanceScoreUtils
    {
        public override Dictionary<HitResult, int> PrepareHitResults(
            double accuracy,
            IEnumerable<HitObject> hitObjects,
            int miss, int? meh, int? good, int? katu)
        {
            int totalHitCounts = hitObjects.Count(), great;
            if (meh.HasValue || good.HasValue)
                great = totalHitCounts - (meh ?? 0) - (good ?? 0) - miss;
            else
            {
                // great (300) -> x6
                // good (100) -> x2
                // meh (50) -> x1
                var totalAccuracyPoints = (int) Math.Round(totalHitCounts * accuracy * 6) ;
                
                // meh + good     + great     = (total hitcount - miss)
                // meh + good * 2 + great * 6 = (total accuracy points)
                // we are assuming that the great (300) count is maximum possible.
                
                // this delta equals to (good + great * 5)
                var delta = totalAccuracyPoints - (totalHitCounts - miss);
                
                // maximizing (great) effectively means (good) will never be greater than 5
                great = delta / 5;
                good = delta % 5;
                meh = totalHitCounts - great - good - miss;
            }

            return new Dictionary<HitResult, int>
            {
                { HitResult.Great, great },
                { HitResult.Ok, good ?? 0 },
                { HitResult.Meh, meh ?? 0 },
                { HitResult.Miss, miss }
            };
        }
    }
}