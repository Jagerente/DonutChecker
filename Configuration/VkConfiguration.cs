using Newtonsoft.Json;

namespace DonutChecker.Configuration
{
    public class VkConfiguration : ApiConfiguration
    {
        [JsonProperty("groupid")]
        public string GroupId { get; set; }

    }
}
