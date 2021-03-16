using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pepper.Services.FGO;

namespace Pepper.External.FGO.Master
{
    public abstract class MasterDataRecord
    {
        private string _masterCollectionName = string.Empty;
        public Region Region;
        
        [BsonId] public ObjectId Id;
        private string MasterCollectionName
        {
            get
            {
                if (_masterCollectionName != string.Empty) return _masterCollectionName;
                var name = GetType().GetCustomAttributes(typeof(MasterCollectionNameAttribute), false)
                    .OfType<MasterCollectionNameAttribute>().First().MasterCollectionName;
                return _masterCollectionName = name;
            }            
        }

        public static string GetMasterCollectionName<T>() where T : MasterDataRecord, new()
        {
            return new T().MasterCollectionName;
        }

        public virtual async void PopulateFields(MasterDataService service) {}
    }

    public class MasterCollectionNameAttribute : Attribute
    {
        public string MasterCollectionName { get; }
        public MasterCollectionNameAttribute(string collectionName)
        {
            MasterCollectionName = collectionName;
        }
    }
}