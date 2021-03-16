using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Pepper.Classes;
using Pepper.External.FGO.Master;
using Pepper.Services.Monitoring;
using Pepper.Services.Monitoring.Log;
using Pepper.Utilities;

namespace Pepper.Services.FGO
{
    public enum Region
    {
        None = 0,
        JP = 1<<0,
        NA = 1<<1 
    }
    
    public partial class MasterDataService
    {
        private readonly LogService _log;
        private readonly MongoClient _client;

        private Dictionary<Region, Dictionary<string, Dictionary<string, object>>> cache =
            new Dictionary<Region, Dictionary<string, Dictionary<string, object>>>();

        public MasterDataService(IServiceProvider serv)
        {
            _log = serv.GetRequiredService<LogService>();
            var config = serv.GetRequiredService<PepperConfiguration>();

            // always ignore extra elements
            ConventionRegistry.Register(
                "IgnoreExtraElements",
                new ConventionPack { new IgnoreExtraElementsConvention(true) },
                type => true
            );
            // TODO : change configuration files
            // since mongoose reads DB name from URI, it was necessary to have separate entries
            // mongo on its own doesn't, we only need a single entry
            _client = new MongoClient(config.Database.FGO.MasterDataConnectionUri.Values.First());
        }

        public async void PrintMasterDataStatistics()
        {
            foreach (var regionCode in Enum.GetNames(typeof(Region)))
            {
                var region = Enum.Parse<Region>(regionCode);
                if (region == Region.None) continue;
                cache[region] = new Dictionary<string, Dictionary<string, object>>();

                var db = _client.GetDatabase(regionCode);
                var collections = await db.ListCollectionsAsync();
                await collections.MoveNextAsync();
                var collectionCount = collections.Current.Count();

                _log.Write(
                    LogType.Success,
                    new LogEntry
                    {
                        Tags = new []{ new LogTag { Name = $"F/GO {regionCode} master data" } },
                        Content = $"Found {collectionCount} collection{StringUtilities.Plural(collectionCount)}."
                    }
                );
            }
        }

        private void EnsureCollectionName(string name)
        {
            foreach (var region in cache.Keys.Where(region => !cache[region].ContainsKey(name)))
                cache[region][name] = new Dictionary<string, object>();
        }
        
        public static string ResolveRegionCode(Region region) => Enum.GetName(typeof(Region), region);
    }
}