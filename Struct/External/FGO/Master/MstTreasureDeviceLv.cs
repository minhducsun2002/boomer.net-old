using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstTreasureDeviceLv")]
    public class MstTreasureDeviceLv : MasterDataRecord
    {
        [BsonElement("funcId")] public int[] FuncId;
        [BsonElement("lv")] public int Level;
        [BsonElement("qp")] public int Qp;

        [BsonElement("tdPoint")] public int TreasureDevicePoint;
        [BsonElement("tdPointDef")] public int TreasureDevicePointDef;
        [BsonElement("tdPointQ")] public int TreasureDevicePointQuick;
        [BsonElement("tdPointA")] public int TreasureDevicePointArts;
        [BsonElement("tdPointB")] public int TreasureDevicePointBuster;
        [BsonElement("tdPointEx")] public int TreasureDevicePointExtra;

        [BsonElement("svals")] public string[] Svals;
        [BsonElement("svals2")] public string[] Svals2;
        [BsonElement("svals3")] public string[] Svals3;
        [BsonElement("svals4")] public string[] Svals4;
        [BsonElement("svals5")] public string[] Svals5;

        [BsonElement("treaureDeviceId")] public int TreasureDeviceId;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstTreasureDeviceLv[]> GetMstTreasureDeviceLvByTreasureDeviceId(Region region, int tdId)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstTreasureDeviceLv>();
            EnsureCollectionName(collectionName);
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(tdId)}_{tdId}"))
                if ((mappings[$"{nameof(tdId)}_{tdId}"] as MstTreasureDeviceLv[]).Length != 0)
                    return (MstTreasureDeviceLv[]) mappings[$"{nameof(tdId)}_{tdId}"];
            
            var raw = (await _client.GetDatabase(regionCode).GetCollection<MstTreasureDeviceLv>(collectionName).FindAsync(
                lv => lv.TreasureDeviceId == tdId,
                new FindOptions<MstTreasureDeviceLv, MstTreasureDeviceLv> {Limit = 1}
            )).ToList().ToArray();

            foreach (var _ in raw)
            {
                _.Region = region;
                _.PopulateFields(this);
            }
            
            return (MstTreasureDeviceLv[]) (mappings[$"{nameof(tdId)}_{tdId}"] = raw);
        }
    }
}