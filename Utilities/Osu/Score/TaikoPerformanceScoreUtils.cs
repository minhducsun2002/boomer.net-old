using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace Pepper.Utilities.Osu
{
    public class TaikoPerformanceScoreUtils : PerformanceScoreUtils
    {
        public override Dictionary<HitResult, int> PrepareHitResults(
            double accuracy,
            IEnumerable<HitObject> hitObjects,
            int miss, int? meh, int? good, int? katu)
        {
            int totalHitCount = hitObjects.Count();
            int great;
            if (good.HasValue)
                great = totalHitCount - (int) good - miss;
            else
            {
                // great -> x2
                // good -> x1
                var totalAccuracyPoints = (int)Math.Round(accuracy * totalHitCount * 2);

                great = totalAccuracyPoints - (totalHitCount - miss);
                good = totalHitCount - great - miss;
            }

            return new Dictionary<HitResult, int>
            {
                { HitResult.Great, great },
                { HitResult.Ok, (int) good },
                { HitResult.Meh, 0 },
                { HitResult.Miss, miss }
            };
        }
    }
}