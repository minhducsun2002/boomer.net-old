using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using FgoExportedConstants;
using MongoDB.Driver;
using Pepper.External.FGO.Master;
using Pepper.Services.FGO;

namespace Pepper.External.FGO.Master
{
    [MasterCollectionName("mstSvt")]
    public class MstSvt : MasterDataRecord
    {
        [BsonElement("name")] public string Name;
        [BsonElement("ruby")] public string Ruby;
        [BsonElement("battleName")] public string BattleName;
        [BsonElement("cost")] public int Cost;
        [BsonElement("type")] public SvtType.Type Type;
        [BsonElement("sellMana")] public int SellMana;
        [BsonElement("sellRarePri")] public int SellRarePri;
        [BsonElement("sellQp")] public int SellQp;
        [BsonElement("collectionNo")] public int CollectionNo;
        [BsonElement("cardIds")] public BattleCommand.TYPE[] CardIds;
        [BsonElement("genderType")] public int GenderType;
        [BsonElement("id")] public int ID;
        [BsonElement("baseSvtId")] public int BaseSvtId;
        [BsonElement("relateQuestIds")] public int[] RelateQuestIds; 
        [BsonElement("starRate")] public int StarRate;
        [BsonElement("rewardLv")] public int RewardLv;
        [BsonElement("classId")] public int ClassId;
        [BsonElement("attri")] public int Attribute;
        [BsonElement("individuality")] public int[] Individuality;
        [BsonElement("classPassive")] public int[] ClassPassiveIds;
        
        // extension fields
        public MstSkill[] ClassPassive;

        public override async void PopulateFields(MasterDataService service)
        {
            ClassPassive = await Task.WhenAll(ClassPassiveIds.Select(async passiveId => await service.GetMstSkillById(Region, passiveId)));
            base.PopulateFields(service);
        }
    }
}

namespace Pepper.Services.FGO
{
    public partial class MasterDataService
    {
        public async Task<MstSvt> GetMstSvt(Region region, int? id, int? collectionNo)
        {
            string regionCode = ResolveRegionCode(region),
                collectionName = MasterDataRecord.GetMasterCollectionName<MstSvt>();
            EnsureCollectionName(collectionName);
            var mappings = cache[region][collectionName];
            
            if (mappings.ContainsKey($"{nameof(id)}_{id}")) return mappings[$"id_{id}"] as MstSvt;
            if (mappings.ContainsKey($"{nameof(collectionNo)}_{collectionNo}")) return mappings[$"{nameof(collectionNo)}_{collectionNo}"] as MstSvt;
            
            if (!id.HasValue && !collectionNo.HasValue) throw new ArgumentNullException(nameof(id));
            
            IAsyncCursor<MstSvt> raw;
            var options = new FindOptions<MstSvt> {Limit = 1};
            if (id.HasValue)
                raw = await _client.GetDatabase(regionCode).GetCollection<MstSvt>(collectionName).FindAsync(svt => svt.BaseSvtId == id, options);
            else
                raw = await _client.GetDatabase(regionCode).GetCollection<MstSvt>(collectionName).FindAsync(svt => svt.CollectionNo == collectionNo, options);

            var response = raw.First();
            response.Region = region;
            response.PopulateFields(this);
            return (MstSvt) (mappings[$"{nameof(id)}_{response.BaseSvtId}"] = mappings[$"{nameof(collectionNo)}_{response.CollectionNo}"] = response);
        }
    }
}