using Newtonsoft.Json;

namespace DonutChecker.Configuration
{
    public class ApiConfiguration
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
