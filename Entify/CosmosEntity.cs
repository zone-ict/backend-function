using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Com.ZoneIct
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UserSession
    {
        public string id { get; set; }
        public string previous { get; set; }
        public string command { get; set; }
        public string language { get; set; } = "ja";
        public string name { get; set; }
        public string talkId { get; set; }
        public string talkLanguage { get; set; } = "ja";
    }
}
