using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps.Legacy;

namespace Pepper.External.Osu
{
    namespace LegacyApi
    {
        public class RecentPlay
        {
            [JsonProperty("beatmap_id")] public ulong BeatmapId;
            [JsonProperty("score")] public ulong Score;
            [JsonProperty("maxcombo")] public ulong MaxCombo;
            [JsonProperty("count50")] public int Count50;
            [JsonProperty("count100")] public int Count100;
            [JsonProperty("count300")] public int Count300;
            [JsonProperty("countmiss")] public int CountMiss;
            [JsonProperty("countkatu")] public int CountKatu;
            [JsonProperty("countgeki")] public int CountGeki;
            [JsonProperty("perfect")] public uint Perfect;
            [JsonProperty("enabled_mods")] public LegacyMods EnabledMods;
            [JsonProperty("user_id")] public ulong UserId;
            [JsonProperty("date")] public DateTime Date;
            [JsonProperty("rank")] public string Rank;
        }
    }
    
    namespace UserExtra
    {
        public struct UserExtra
        {
            [JsonProperty("scoresBest")] public Score[] BestScores;
        }
        
        public class Score
        {
            [JsonProperty("id")] public ulong Id;
            [JsonProperty("user_id")] public ulong UserId;
            [JsonProperty("accuracy")] public float Accuracy;
            [JsonProperty("mods")] public string[] Mods;
            [JsonProperty("score")] public ulong TotalScore;
            [JsonProperty("perfect")] public bool Perfect;
            [JsonProperty("pp")] public float PerformancePoint;
            [JsonProperty("rank")] public string Rank;
            [JsonProperty("created_at")] public DateTime CreatedAt;
            [JsonProperty("max_combo")] public ulong MaxCombo;

            [JsonProperty("beatmap")] public ScoreBeatmap Beatmap;
            [JsonProperty("beatmapset")] public ScoreBeatmapset Beatmapset;

            [JsonProperty("statistics")] public ScoreStatistics Statistics;
        }

        public class ScoreBeatmap
        {
            [JsonProperty("version")] public string Version;
            [JsonProperty("id")] public ulong Id;
            [JsonProperty("beatmapset_id")] public ulong SetId;
            [JsonProperty("difficulty_rating")] public float StarRating;
            [JsonProperty("bpm")] public float BeatPerMinute;
            
            [JsonProperty("cs")] public float CircleSize;
            [JsonProperty("drain")] public float DrainRate;
            [JsonProperty("accuracy")] public float OverallDifficulty;
            [JsonProperty("ar")] public float ApproachRate;
        }

        public class ScoreBeatmapset
        {
            [JsonProperty("id")] public ulong Id;
            [JsonProperty("title")] public string Title;
            [JsonProperty("artist")] public string Artist;
            [JsonProperty("source")] public string Source;
            [JsonProperty("creator")] public string Creator;
        }

        public class ScoreStatistics
        {
            [JsonProperty("count_50")] public ulong Count50;
            [JsonProperty("count_100")] public ulong Count100;
            [JsonProperty("count_300")] public ulong Count300;
            [JsonProperty("count_geki")] public ulong CountGeki;
            [JsonProperty("count_katu")] public ulong CountKatu;
            [JsonProperty("count_miss")] public ulong CountMiss;
        }
    }
}