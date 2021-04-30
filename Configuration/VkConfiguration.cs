using Newtonsoft.Json;

namespace DonutChecker.Configuration
{
    public class VkConfiguration : ApiConfiguration
    {
        [JsonProperty("groupId")]
        public ulong GroupId { get; set; }

        [JsonProperty("conversationId")]
        public long ConversationId { get; set; }
    }
}
