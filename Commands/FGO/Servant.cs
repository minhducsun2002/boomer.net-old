using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FgoExportedConstants;
using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.Classes.Command.Result;
using Pepper.External.FGO.Master;
using Pepper.Services.FGO;
using Qmmands;

namespace Pepper.Commands.FGO
{
    public class Servant : PepperCommand
    {
        public MasterDataService MasterData { get; set; }
        public TraitService Trait { get; set; }
        
        [Command("s", "servant")]
        [PrefixCategory("fgo")]
        [Category("F/GO")]
        public async Task<PepperCommandResult> Exec([Remainder] string query = "")
        {
            var collectionNo = Convert.ToInt32(query);
            MstSvt mstSvt = await MasterData.GetMstSvt(Region.JP, null, collectionNo),
                mstSvtNA = await MasterData.GetMstSvt(Region.NA, null, collectionNo);
            var limits = await MasterData.GetMstSvtLimitBySvtId(Region.JP, mstSvt.BaseSvtId);
            var @class = await MasterData.GetMstClassById(Region.NA, mstSvt.ClassId);
            var treasureDeviceMapping = (await MasterData.GetMstSvtTreasureDeviceBySvtId(Region.JP, mstSvt.BaseSvtId))
                .First(mapping => mapping.Num == 1);
            var tdBaseLv = await MasterData.GetMstTreasureDeviceLvByTreasureDeviceId(
                Region.JP, treasureDeviceMapping.TreasureDeviceId);
            var damageDistributions = (await MasterData.GetMstSvtCardBySvtId(Region.JP, mstSvt.ID))
                .OrderBy(card => card.CardId).Select(card => card.NormalDamage).ToArray();
            var cards = mstSvt.CardIds.Distinct()
                .ToImmutableDictionary(card => (int) card, card => mstSvt.CardIds.Count(c => c == card))
                // extra card
                .Add(4, 1);

            return new EmbedResult
            {
                Embeds = new []
                {
                    GetBaseServantEmbedBuilder(limits, @class, mstSvt, mstSvtNA.Name)
                        .WithFields(
                            new EmbedFieldBuilder
                            {
                                Name = "HP/ATK",
                                Value = $"- Base : {limits[0].HpBase}/{limits[0].AtkBase}\n- Maximum : {limits[0].HpMax}/{limits[0].AtkMax}",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "NP generation",
                                Value = $"Per hit : **{(float) tdBaseLv[0].TreasureDevicePoint / 100:F2}**%"
                                    + $"\nWhen attacked : **{((float) tdBaseLv[0].TreasureDevicePointDef) / 100:F2}**%",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Critical stars",
                                Value = $"Weight : **{limits[0].CriticalWeight}**\nGeneration : **{(float) mstSvt.StarRate / 10:F1}**%",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Gender",
                                Value = $"{Trait.ResolveTrait(mstSvt.GenderType)}",
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Cards / Damage distribution by %",
                                Value = string.Join('\n',
                                    "```",
                                    "   Card   | Hit counts",
                                    string.Join(
                                    '\n',
                                    cards.Select(card
                                        => string.Join(
                                            "",
                                            $@"{card.Value}x ",
                                            Trait.ResolveTrait(card.Key + 4000).PadRight(
                                                cards.Keys.Select(k => Trait.ResolveTrait(k + 4000).Length).Max()
                                            ),
                                            $" | {damageDistributions[card.Key - 1].Length}", $" ({string.Join('-', damageDistributions[card.Key - 1])})")
                                        )
                                    ),
                                    "```"
                                )
                            }
                        )
                        .Build(),
                    GetBaseServantEmbedBuilder(limits, @class, mstSvt, mstSvtNA.Name)
                        .WithFields(new EmbedFieldBuilder
                        {
                            Name = "Traits",
                            Value = string.Join(
                                '\n',
                                mstSvt.Individuality
                                    // ignore class - self - gender traits
                                    .Where(i => !new []{ mstSvt.ID, mstSvt.GenderType, mstSvt.ClassId + 99 }.Contains(i))
                                    .Select(i => $"* **{Trait.ResolveTrait(i)}**")
                            )
                        })
                        .Build()
                }
            };
        }

        private static EmbedBuilder GetBaseServantEmbedBuilder(IEnumerable<MstSvtLimit> limits, MstClass @class, MstSvt mstSvt, string nameOverwrite = "") => new EmbedBuilder
        {
            Author = new EmbedAuthorBuilder
            {
                Name = $"{string.Join('-', limits.Select(_ => _.Rarity).Distinct().OrderBy(_ => _))}☆ "
                       + $"{@class.Name}",
            },
            Title =
                $"{mstSvt.CollectionNo}. **{(string.IsNullOrWhiteSpace(nameOverwrite) ? mstSvt.Name : nameOverwrite)}** (`{mstSvt.BaseSvtId}`)",
            Url = $"https://apps.atlasacademy.io/db/#/JP/servant/{mstSvt.CollectionNo}",
            ThumbnailUrl = $"https://assets.atlasacademy.io/GameData/JP/Faces/f_{mstSvt.BaseSvtId}0.png"
        };
    }
}