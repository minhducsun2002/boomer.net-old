using System;
using Newtonsoft.Json;

namespace Pepper.External.Osu
{
    public class Beatmap : UserExtra.ScoreBeatmap
    {
        [JsonProperty("mode")] public string GameMode;
        [JsonProperty("mode_int")] public GameMode GameModeInt;
        
        [JsonProperty("convert")] public bool? IsConvert;
        [JsonProperty("total_length")] public uint Length;
        [JsonProperty("hit_length")] public uint DrainLength;

        [JsonProperty("playcount")] public ulong PlayCount;
        [JsonProperty("passcount")] public ulong PassCount;
        [JsonProperty("max_combo")] public ulong MaxCombo;
    }

    public class Beatmapset : UserExtra.ScoreBeatmapset
    {
        [JsonProperty("submitted_date")] public DateTime SubmittedDate;
        [JsonProperty("last_updated")] public DateTime LastUpdatedDate;
        [JsonProperty("ranked_date")] public DateTime? RankedDate;

        [JsonProperty("bpm")] public float BeatsPerMinute;
        
        [JsonProperty("tags")] public string Tags;
        [JsonProperty("preview_url")] public string PreviewUrl;
        
        [JsonProperty("video")] public bool HasVideo;
        [JsonProperty("storyboard")] public bool HasStoryboard;

        [JsonProperty("ranked")] public int RankedStatus;
        [JsonProperty("status")] public string Status;

        [JsonProperty("user_id")] public ulong CreatorId;

        [JsonProperty("beatmaps")] public Beatmap[] Beatmaps;
        [JsonProperty("converts")] public Beatmap[] Converts;
    }
}