using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Interactivity;
using Interactivity.Pagination;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.External.Osu;
using Pepper.Utilities;
using Pepper.Utilities.Osu;

namespace Pepper.Commands.Osu
{
    public class Beatmap : PepperCommand
    {
        private const int MaximumDifficultyPerPage = 7;

        [Command("beatmap", "beatmapset", "map", "mapset")]
        [PrefixCategory("osu")]
        [Category("osu!")]
        [Description("Show information of a beatmap(set).")]
        public async Task Exec(
            string beatmapId,
            [Flag("/set")] bool isMapset = false
        )
        {
            // try parsing as map ID
            var mapIdSuccessful = ulong.TryParse(beatmapId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id);
            if (!mapIdSuccessful)
            {
                // map ID failed.
                // try parsing as URL
                var (setId, mapId, pointToSet) = OsuClient.ParseBeatmapUrl(beatmapId);
                switch (pointToSet)
                {
                    case false:
                        id = mapId;
                        break;
                    case true:
                        isMapset = true;
                        id = setId;
                        break;
                }
            }
            
            // fetch
            var mapset = await OsuClient.GetMapset(id, isMapset);
            
            // embed title
            var embedTitle = $"{mapset.Artist} - {mapset.Title}";
            var imageUrl = $"https://assets.ppy.sh/beatmaps/{mapset.Id}/covers/cover@2x.jpg";
            
            // date of importance depending on ranked status 
            var lastChangedDate = (mapset.RankedDate ?? mapset.LastUpdatedDate)
                .ToUniversalTime().ToString("dd/MM/yyyy, hh:mm:ss tt 'UTC'",
                    CultureInfo.InvariantCulture);

            // sentence case for ranked status
            var status = mapset.Status;
            var invariantTextInfo = CultureInfo.InvariantCulture.TextInfo;
            status = invariantTextInfo.ToUpper(status[0]) + invariantTextInfo.ToLower(status[1..]);
            
            switch (isMapset)
            {
                case true:
                {
                    // sorting maps into modes
                    var maps = new Dictionary<GameMode, List<External.Osu.Beatmap>>();
                    foreach (var map in mapset.Beatmaps.Concat(mapset.Converts))
                        if (!maps.ContainsKey(map.GameModeInt))
                            maps[map.GameModeInt] = new List<External.Osu.Beatmap> { map };
                        else
                            maps[map.GameModeInt].Add(map);
                    
                    // constructing embeds
                    var embeds = maps.OrderBy(pair => pair.Key)
                        // sort by modes
                        .SelectMany((record, currentGameModeIndex) =>
                        {
                            var (gameMode, beatmaps) = record;
                            var friendlyMode = gameMode.ToFriendlyName();
                            
                            // sort beatmaps by star rating
                            beatmaps.Sort((map1, map2) => map1.StarRating.CompareTo(map2.StarRating));

                            // split beatmaps into chunks to stay under Discord's limit
                            var mapChunks = beatmaps.ChunkBy(MaximumDifficultyPerPage).ToImmutableArray();
                            
                            // mapset length is assumed to be the length of the longest map
                            var length = beatmaps.Select(map => map.Length).Max();
                            
                            return mapChunks
                                .Select((mapChunk, chunkIndex) =>
                                    new EmbedBuilder
                                    {
                                        Author = new EmbedAuthorBuilder
                                        {
                                            Name = mapset.Creator,
                                            Url = $"https://osu.ppy.sh/users/{mapset.CreatorId}",
                                            IconUrl = $"https://a.ppy.sh/{mapset.CreatorId}"
                                        },
                                        Title = embedTitle,
                                        Url = $"https://osu.ppy.sh/beatmapsets/{id}",
                                        ImageUrl = imageUrl,
                                        Description =
                                            $"\nLength : **{SerializationUtilities.SerializeTimeLengthFriendly(length)}** - **{mapset.BeatsPerMinute}** BPM"
                                            + (mapset.RankedDate.HasValue ? "\nRanked" : $"{status}\nLast updated")
                                            + $" **{lastChangedDate}**."
                                            + $"\nDownload : [[**main site**]](https://osu.ppy.sh/beatmapsets/{id}/download) "
                                            + $"[[**ripple.moe**]](https://storage.ripple.moe/d/{id}) "
                                            + $"[[**chimu.moe**]](https://chimu.moe/d/{id})",
                                        Fields = mapChunk
                                            .Select(map => new EmbedFieldBuilder
                                            {
                                                Name = map.Version,
                                                Value = (
                                                    $"[[**Link**]](https://osu.ppy.sh/beatmaps/{map.Id}) "
                                                    + $"{map.StarRating:F2} :star:{(map.MaxCombo > 0 ? $" | **{map.MaxCombo}**x" : "")} | "
                                                    + $"{SerializationUtilities.SerializeStats(map)}"
                                                )
                                            })
                                            .ToList(),
                                        Footer = new EmbedFooterBuilder
                                        {
                                            Text =
                                                $"Mode : {friendlyMode} ({currentGameModeIndex + 1}/{maps.Count}) | Page {chunkIndex + 1}/{mapChunks.Count()}"
                                        }
                                    }.Build()
                                );
                        });
                    var paginator = EmbedUtilities.PagedEmbedBuilder()
                        .WithPages(embeds.Select(PageBuilder.FromEmbed))
                        .Build();
                    await InteractivityService.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromSeconds(20));
                    break;
                }
                case false:
                {
                    var map = mapset.Beatmaps.Concat(mapset.Converts).First(beatmap => beatmap.Id == id);
                    var embed = new EmbedBuilder
                        {
                            Title = $"{embedTitle} [{map.Version}]",
                            Url = $"https://osu.ppy.sh/beatmapsets/{mapset.Id}#{map.GameModeInt.ToUrlPath()}/{id}",
                            ImageUrl = imageUrl,
                            Description =
                                $"Mapped by **[{mapset.Creator}](https://osu.ppy.sh/users/{mapset.CreatorId})**. "
                                + (mapset.RankedDate.HasValue ? "\nRanked" : $"{status}\nLast updated")
                                + $" **{lastChangedDate}**.",
                            Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                {
                                    Name = "Difficulty",
                                    Value = $"{map.StarRating} :star: - {SerializationUtilities.SerializeStats(map)} - {map.BeatPerMinute} BPM"
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Length",
                                    Value = $"{(map.MaxCombo > 0 ? $"**{map.MaxCombo}**x | " : "")} {SerializationUtilities.SerializeTimeLength(map.DrainLength)}",
                                    IsInline = true
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Game mode",
                                    Value = map.GameModeInt.ToFriendlyName(),
                                    IsInline = true
                                }
                            }
                        }
                        .Build();
                    await Context.Channel.SendMessageAsync("", false, embed);
                    break;
                }
            }
        }

        
    }
}