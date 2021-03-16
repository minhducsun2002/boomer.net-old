using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstSkill")]
    public class MstSkill : MasterDataRecord
    {
        [BsonElement("id")] public int ID;
        [BsonElement("actIndividuality")] public int[] ActIndividuality;
        [BsonElement("name")] public string Name;
        [BsonElement("ruby")] public string Ruby;
        [BsonElement("maxLv")] public int MaxLv;
        [BsonElement("type")] public int Type;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstSkill> GetMstSkillById(Region region, int id)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstSkill>();
            EnsureCollectionName(collectionName);
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(id)}_{id}")) return mappings[$"{nameof(id)}_{id}"] as MstSkill;
            
            var responses = (await _client.GetDatabase(regionCode).GetCollection<MstSkill>(collectionName).FindAsync(
                svt => svt.ID == id,
                new FindOptions<MstSkill, MstSkill> {Limit = 1}
            )).First();

            responses.Region = region;
            responses.PopulateFields(this);
            
            return (MstSkill) (mappings[$"{nameof(id)}_{id}"] = responses);
        }
    }
}