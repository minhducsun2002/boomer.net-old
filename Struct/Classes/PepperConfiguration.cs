using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pepper.Classes
{
    public class FGOConfiguration
    {
        private FGOConfiguration() {}

        [JsonProperty("masterData")]
        public Dictionary<string, string> MasterDataConnectionUri = new Dictionary<string, string>();

        [JsonProperty("complementary")]
        public Dictionary<string, string> ComplementaryDataConnectionUri = new Dictionary<string, string>();
        
        [JsonProperty("servant_aliases")]
        public string ServantAliasConnectionUri = string.Empty;
    }

    public class DatabaseConfiguration
    {
        private DatabaseConfiguration() {}

        [JsonProperty("fgo")] public FGOConfiguration FGO;
    }
    
    public class PepperConfiguration
    {
        [JsonProperty("prefix")]
        public Dictionary<string, string[]> Prefix = new Dictionary<string, string[]>();

        [JsonProperty("database")] public DatabaseConfiguration Database;
    }
}