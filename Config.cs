using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant
{
    public class Config
    {
        public string JenkinsUsername { get; }
        public string JenkinsKey { get; }

        public string DiscordToken { get; }

        public Config(string jenkinsUsername, string jenkinsKey, string discordToken)
        {
            JenkinsUsername = jenkinsUsername ?? throw new ArgumentNullException(nameof(jenkinsUsername));
            JenkinsKey = jenkinsKey ?? throw new ArgumentNullException(nameof(jenkinsKey));
            DiscordToken = discordToken ?? throw new ArgumentNullException(nameof(discordToken));
        }
    }
}
