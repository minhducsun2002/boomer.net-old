using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstSvtTreasureDevice")]
    public class MstSvtTreasureDevice : MasterDataRecord
    {
        [BsonElement("svtId")] public int SvtId;
        [BsonElement("treasureDeviceId")] public int TreasureDeviceId;
        [BsonElement("num")] public int Num;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstSvtTreasureDevice[]> GetMstSvtTreasureDeviceBySvtId(Region region, int svtId)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstSvtTreasureDevice>();
            EnsureCollectionName(collectionName);
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(svtId)}_{svtId}"))
                if ((mappings[$"{nameof(svtId)}_{svtId}"] as MstSvtTreasureDevice[]).Length != 0)
                    return mappings[$"{nameof(svtId)}_{svtId}"] as MstSvtTreasureDevice[];
            
            var raw = (await _client.GetDatabase(regionCode)
                .GetCollection<MstSvtTreasureDevice>(collectionName)
                .FindAsync(svtTdMapping => svtTdMapping.SvtId == svtId)).ToList().ToArray();

            foreach (var limit in raw)
            {
                limit.Region = region;
                limit.PopulateFields(this);                
            }

            return (MstSvtTreasureDevice[]) (mappings[$"{nameof(svtId)}_{svtId}"] = raw);
        }
    }
}