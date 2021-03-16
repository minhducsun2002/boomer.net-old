using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Pepper.External.FGO.Master;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstClass")]
    public class MstClass : MasterDataRecord
    {
        [BsonElement("id")] public int ID;
        [BsonElement("attri")] public int Attribute;
        [BsonElement("name")] public string Name;
        [BsonElement("attackRate")] public int AttackRate;
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstClass> GetMstClassById(Region region, int id)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstClass>();
            EnsureCollectionName(collectionName);
            
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(id)}_{id}")) return mappings[$"{nameof(id)}_{id}"] as MstClass;
            
            var responses = (await _client.GetDatabase(regionCode).GetCollection<MstClass>(collectionName).FindAsync(
                @class => @class.ID == id,
                new FindOptions<MstClass, MstClass> {Limit = 1}
            )).First();

            responses.Region = region;
            responses.PopulateFields(this);
            
            return (MstClass) (mappings[$"{nameof(id)}_{id}"] = responses);
        }
    }
}