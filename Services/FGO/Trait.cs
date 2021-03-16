using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Services.Monitoring;
using Pepper.Services.Monitoring.Log;

namespace Pepper.Services.FGO
{
    public class TraitService
    {
        private Dictionary<int, string> _traits = new Dictionary<int, string>();
        private readonly LogService _log;

        public TraitService(IServiceProvider serv)
        {
            _log = serv.GetRequiredService<LogService>();
        }
        public void LoadMapping()
        {
            var path = Environment.GetEnvironmentVariable("FGO_TRAIT_MAPPINGS");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("URI to trait mapping for F/GO is not defined");

            var mappings = new WebClient().DownloadString(path);
            _traits = JsonConvert.DeserializeObject<Dictionary<int, string>>(mappings);
            
            _log.Write(LogType.Success, new LogEntry
            {
                Content = $"Loaded {_traits.Keys.Count} traits.",
                Tags = new [] { new LogTag { Name = "F/GO trait mapping"} }
            });
        }

        public string ResolveTrait(int trait) => _traits.ContainsKey(trait) ? _traits[trait] : trait.ToString();
    }
}