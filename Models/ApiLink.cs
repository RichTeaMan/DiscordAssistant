using Newtonsoft.Json;

namespace DiscordAssistant.Models
{
    public class ApiLink
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
