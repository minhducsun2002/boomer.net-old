using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Pepper.External.Osu
{
    public struct UserCountry
    {
        /// <summary>
        /// (seemingly) ISO 3166 country code
        /// </summary>
        [JsonProperty("code")] public string Code;
        /// <summary>
        /// Country name
        /// </summary>
        [JsonProperty("name")] public string Name;
    }

    public struct UserLevel
    {
        /// <summary>
        /// Current level
        /// </summary>
        [JsonProperty("current")] public uint Current;
        /// <summary>
        /// Progress to the next level (i.e. Current+1), in percentages
        /// </summary>
        [JsonProperty("progress")] public uint Progress;
    }

    /// <summary>
    /// Rank counts in all ranked plays. 
    /// </summary>
    public struct UserScoreGrades
    {
        [JsonProperty("ss")] public long X;
        [JsonProperty("ssh")] public long XH;
        [JsonProperty("s")] public long S;
        [JsonProperty("sh")] public long SH;
        [JsonProperty("a")] public long A;
    }

    public struct UserStatistics
    {
        /// <summary>
        /// Level info
        /// </summary>
        [JsonProperty("level")] public UserLevel Level;
        /// <summary>
        /// Total performance points
        /// </summary>
        [JsonProperty("pp")] public float PerformancePoints;
        /// <summary>
        /// Total ranked score
        /// </summary>
        [JsonProperty("ranked_score")] public ulong RankedScore;
        /// <summary>
        /// Total accuracy
        /// </summary>
        [JsonProperty("hit_accuracy")] public float HitAccuracy;
        /// <summary>
        /// Total play count
        /// </summary>
        [JsonProperty("play_count")] public long PlayCount;
        /// <summary>
        /// Total play time, in seconds
        /// </summary>
        [JsonProperty("play_time")] public long PlayTime;
        /// <summary>
        /// Total score (including unranked score)
        /// </summary>
        [JsonProperty("total_score")] public ulong TotalScore;
        /// <summary>
        /// Total hit count
        /// </summary>
        [JsonProperty("total_hits")] public ulong TotalHits;
        /// <summary>
        /// Maximum combo reached in any play
        /// </summary>
        [JsonProperty("maximum_combo")] public ulong MaximumCombo;
        /// <summary>
        /// Whether user is active (counted in leaderboards) 
        /// </summary>
        [JsonProperty("is_ranked")] public bool IsRanked;
        /// <summary>
        /// Counts of S(S)(H)/A plays.
        /// </summary>
        [JsonProperty("grade_counts")] public UserScoreGrades GradeCounts;
        /// <summary>
        /// Ranking in global/country leaderboards.
        /// </summary>
        [JsonProperty("global_rank")] public ulong GlobalRank;
        [JsonProperty("country_rank")] public ulong CountryRank;
    }
    
    public struct User
    {
        [JsonProperty("id")] public long Id;
        [JsonProperty("username")] public string Username;
        [JsonProperty("join_date")] public DateTime JoinedDate;
        [JsonProperty("country")] public UserCountry Country;
        [JsonProperty("avatar_url")] public string AvatarUrl;
        
        [JsonProperty("location")] public string Location;
        [JsonProperty("last_visit")] [DefaultValue(null)] public string LastVisit;
        [JsonProperty("is_online")] public bool IsOnline;

        [JsonProperty("statistics")] public UserStatistics Statistics;
    }
}