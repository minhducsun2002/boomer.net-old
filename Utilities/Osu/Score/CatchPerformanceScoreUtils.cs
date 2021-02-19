using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace Pepper.Utilities.Osu
{
    public class CatchPerformanceScoreUtils : PerformanceScoreUtils
    {
        public override Dictionary<HitResult, int> PrepareHitResults(
            double accuracy,
            IEnumerable<HitObject> hitObjects,
            int miss, int? meh, int? good, int? katu)
        {
            var hits = hitObjects.ToArray();
            var juiceHits = hits.OfType<JuiceStream>().ToArray();
            var maxCombo = hits.Count(hitObject => hitObject is Fruit)
                           + juiceHits.SelectMany(juice => juice.NestedHitObjects)
                               .Count(hitObject => !(hitObject is TinyDroplet));

            
            var maxTinyDroplets = juiceHits.Sum(juice => juice.NestedHitObjects.OfType<TinyDroplet>().Count());
            var maxDroplets = juiceHits.Sum(juice => juice.NestedHitObjects.Count(hitObject => hitObject is Droplet)) - maxTinyDroplets;
            var maxFruits  = hits.OfType<Fruit>().Count() 
                             + 2 * juiceHits.Length
                             + juiceHits.Sum(juice => juice.RepeatCount);

            var countDroplets = good ?? Math.Max(0, maxDroplets - miss);
            
            // (maxDroplet - countDroplet) is the missed droplet count
            // the number of fruits should be the total fruits minus remaining misses
            var countFruits = maxFruits - (miss - (maxDroplets - countDroplets));

            var countTinyDroplets =
                meh ?? (int)Math.Round(accuracy * (maxCombo + maxTinyDroplets)) - countFruits - countDroplets;

            var countTinyMiss = katu ?? maxTinyDroplets - countTinyDroplets;

            return new Dictionary<HitResult, int>
            {
                { HitResult.Great, countFruits },
                { HitResult.LargeTickHit, countDroplets },
                { HitResult.SmallTickHit, countTinyDroplets },
                { HitResult.SmallTickMiss, countTinyMiss },
                { HitResult.Miss, miss }
            };
        }
    }
}