using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Pepper.External.Osu.LegacyApi;
using Pepper.External.Osu.UserExtra;

namespace Pepper.External.Osu
{
    public class OsuClient
    {
        private string _v1ApiKey;
        private static readonly HttpClient HttpClient = new HttpClient();
        public OsuClient(string apiKey)
        {
            _v1ApiKey = apiKey;
        }

        public async Task<RecentPlay[]> GetRecentPlay(string username, GameMode gameMode, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("No username provided");
            
            var raw = await
                HttpClient.GetStringAsync(new Uri(
                    $"https://osu.ppy.sh/api/get_user_recent?u={HttpUtility.UrlEncode(username)}&k={_v1ApiKey}&m={(int) gameMode}&limit={limit}"
                ));
            return JsonConvert.DeserializeObject<RecentPlay[]>(raw);
        }
        
        public static async Task<(User User, UserExtra.UserExtra Extra)> GetUser(string username, GameMode gameMode)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("No username provided");
            var raw = await
                HttpClient.GetStringAsync(new Uri($"https://osu.ppy.sh/u/{HttpUtility.UrlPathEncode(username)}/{gameMode.ToUrlPath()}"));
            var document = new HtmlDocument();
            document.LoadHtml(raw);
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            return (
                User: JsonConvert.DeserializeObject<User>(document.GetElementbyId("json-user").InnerText, serializerSettings),
                Extra: JsonConvert.DeserializeObject<UserExtra.UserExtra>(document.GetElementbyId("json-extras").InnerText, serializerSettings)
            );
        }

        /// <summary>
        /// Get beatmapset data.
        /// </summary>
        /// <param name="id">ID of the map(set) to get</param>
        /// <param name="isSetId">Whether the ID provided is a set ID</param>
        public static async Task<Beatmapset> GetMapset(ulong id, bool isSetId = false)
        {
            var raw = await
                HttpClient.GetStringAsync(new Uri($"https://osu.ppy.sh/{(isSetId ? "beatmapsets" : "beatmaps")}/{id}"));
            var document = new HtmlDocument();
            document.LoadHtml(raw);
            var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<Beatmapset>(document.GetElementbyId("json-beatmapset").InnerText, serializerSettings);
        }

        public static Task<Score[]> GetBest(long userId, GameMode mode, int maxResult = 50, int maxSingle = 50)
            => GetScoreset("best", userId, mode, maxResult, maxSingle);
        
        public static Task<Score[]> GetRecent(long userId, GameMode mode, int maxResult = 50, int maxSingle = 50)
            => GetScoreset("recent", userId, mode, maxResult, maxSingle);

        public static (ulong SetId, ulong MapId, bool pointToSet) ParseBeatmapUrl(string url)
        {
            ulong setId = 0, mapId = 0;
            var pointToSet = false;
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (uri.Host != "osu.ppy.sh") throw new ArgumentException($"The hostname isn't osu.ppy.sh!");
            if (uri.Segments.Length < 3) throw new ArgumentException($"Not enough URI segments!");

            if (!ulong.TryParse(uri.Segments[2].EndsWith('/') ? uri.Segments[2][..^1] : uri.Segments[2], out var id))
                throw new ArgumentException($"The map/set ID is not present or invalid!");
            
            switch (uri.Segments[1])
            {
                case "beatmaps/":
                case "b/":
                    mapId = id;
                    break;
                case "beatmapsets/":
                    setId = id;
                    // detect fragments after the hash (for beatmapsets URLs)
                    if (uri.Fragment.Length != 0)
                    {
                        try
                        {
                            var split = uri.Fragment[1..].Split('/');
                            if (!ulong.TryParse(split[1], out mapId))
                            {
                                // mode = split[0];
                                pointToSet = true;
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else pointToSet = true;
                    
                    break;
                default:
                    throw new ArgumentException($"Not a beatmap/set URI!");
            }

            return (setId, mapId, pointToSet);
        }

        /// <summary>
        /// Fetch scoresets from path ("recent"/"best")
        /// </summary>
        /// <param name="path">Path to fetch from</param>
        /// <param name="userId">User ID to fetch scores</param>
        /// <param name="mode">Gamemode to fetch from</param>
        /// <param name="maxResult">Maximum amount of scores returned</param>
        /// <param name="maxSingle">Maximum amount of scores fetched in a single call. Should be no more than 50.</param>
        /// <returns></returns>
        private static async Task<Score[]> GetScoreset(string path, long userId, GameMode mode, int maxResult = 50, int maxSingle = 50)
        {
            var scores = new List<Score>();
            var fetched = 0;
            while (fetched < maxResult)
            {
                var raw = await HttpClient.GetStringAsync(
                    $"https://osu.ppy.sh/users/{userId}/scores/{path}?mode={mode.ToUrlPath()}"
                    + $"&offset={fetched}&limit={Math.Min(maxSingle, maxResult - fetched)}"
                );
                
                var serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var batch = JsonConvert.DeserializeObject<Score[]>(raw, serializerSettings);
                scores.AddRange(batch);
                fetched += maxSingle;
                if (batch.Length == 0) break;
            }

            return (scores.Count > maxResult ? scores.GetRange(0, maxResult) : scores).ToArray();
        }

        public static async Task<string> GetBeatmapFile(ulong id)
        {
            return await HttpClient.GetStringAsync($"https://osu.ppy.sh/osu/{id}");
        }
    }
}