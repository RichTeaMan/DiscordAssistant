using Newtonsoft.Json;

namespace DiscordAssistant.Models
{
    public interface ApiLink
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
