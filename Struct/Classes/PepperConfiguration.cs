using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pepper.Classes
{
    class PepperConfiguration
    {
        [JsonProperty("prefix")]
        public Dictionary<string, string[]> Prefix = new Dictionary<string, string[]>();
    }
}