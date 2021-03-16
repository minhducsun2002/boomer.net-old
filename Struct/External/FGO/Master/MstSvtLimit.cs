using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstSvtLimit")]
    public class MstSvtLimit : MasterDataRecord
    {
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("rarity")] public int Rarity;
        [BsonElement("lvMax")] public int LvMax;
        [BsonElement("criticalWeight")] public int CriticalWeight;
        
        // stats
        [BsonElement("power")] public int Power;
        [BsonElement("defense")] public int Defense;
        [BsonElement("agility")] public int Agility;
        [BsonElement("magic")] public int Magic;
        [BsonElement("luck")] public int Luck;
        [BsonElement("treasureDevice")] public int NP;

        [BsonElement("hpBase")] public int HpBase;
        [BsonElement("hpMax")] public int HpMax;
        [BsonElement("atkBase")] public int AtkBase;
        [BsonElement("atkMax")] public int AtkMax;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstSvtLimit[]> GetMstSvtLimitBySvtId(Region region, int svtId)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstSvtLimit>();
            EnsureCollectionName(collectionName);
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(svtId)}_{svtId}"))
                if ((mappings[$"{nameof(svtId)}_{svtId}"] as MstSvtLimit[]).Length != 0)
                    return mappings[$"{nameof(svtId)}_{svtId}"] as MstSvtLimit[];
            
            var responses = (await _client.GetDatabase(regionCode).GetCollection<MstSvtLimit>(collectionName).FindAsync(limit => limit.SvtId == svtId))
                .ToList().ToArray();

            foreach (var limit in responses)
            {
                limit.Region = region;
                limit.PopulateFields(this);                
            }

            return (MstSvtLimit[]) (mappings[$"{nameof(svtId)}_{svtId}"] = responses);
        }
    }
}