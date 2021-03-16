using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstSvtCard")]
    public class MstSvtCard : MasterDataRecord
    {
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("cardId")] public int CardId;
        [BsonElement("normalDamage")] public int[] NormalDamage;
        [BsonElement("singleDamage")] public int[] SingleDamage;
        [BsonElement("trinityDamage")] public int[] TrinityDamage;
        [BsonElement("unisonDamage")] public int[] UnisonDamage;
        [BsonElement("grandDamage")] public int[] GrandDamage;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstSvtCard[]> GetMstSvtCardBySvtId(Region region, int svtId)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstSvtCard>();
            EnsureCollectionName(collectionName);
            
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(svtId)}_{svtId}")) return mappings[$"{nameof(svtId)}_{svtId}"] as MstSvtCard[];
            
            var responses = (await _client.GetDatabase(regionCode).GetCollection<MstSvtCard>(collectionName).FindAsync(
                card => card.SvtId == svtId
            )).ToList().ToArray();

            foreach (var limit in responses)
            {
                limit.Region = region;
                limit.PopulateFields(this);                
            }
            
            return (MstSvtCard[]) (mappings[$"{nameof(svtId)}_{svtId}"] = responses);
        }
    }
}