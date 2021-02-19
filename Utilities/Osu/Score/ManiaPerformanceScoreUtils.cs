using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace Pepper.Utilities.Osu
{
    public class ManiaPerformanceScoreUtils : PerformanceScoreUtils
    {
        public override Dictionary<HitResult, int> PrepareHitResults(
            double accuracy,
            IEnumerable<HitObject> hitObjects,
            int miss, int? meh, int? good, int? katu)
        {
            return null;
        }
    }
}