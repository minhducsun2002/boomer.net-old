using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Pepper.Classes;
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
    
    public class MasterDataService
    {
        private readonly LogService _log;
        public readonly MongoClient Client;

        public MasterDataService(IServiceProvider serv)
        {
            _log = serv.GetRequiredService<LogService>();
            var config = serv.GetRequiredService<PepperConfiguration>();

            // TODO : change configuration files
            // since mongoose reads DB name from URI, it was necessary to have separate entries
            // mongo on its own doesn't, we only need a single entry
            Client = new MongoClient(config.Database.FGO.MasterDataConnectionUri.Values.First());
        }

        public async void PrintMasterDataStatistics()
        {
            foreach (var regionCode in Enum.GetNames(typeof(Region)))
            {
                var region = Enum.Parse<Region>(regionCode);
                if (region == Region.None) continue;

                var db = Client.GetDatabase(regionCode);
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
    }
}