using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public abstract class JenkinsObject
    {
        public string _class { get; set; }

        [JsonIgnore]
        public abstract string ClassName { get; }
    }
}
