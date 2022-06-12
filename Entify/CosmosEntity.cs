using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Com.ZoneIct
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserSession
    {
        public string Id { get; set; }
        public string Previous { get; set; }
        public string Command { get; set; }
        public string Language { get; set; }
    }
}
