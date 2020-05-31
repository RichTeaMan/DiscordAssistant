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

        public string CouchDbUrl { get; }

        public string CouchDbUsername { get; }

        public string CouchDbPassword { get; }

        public Config(string jenkinsUsername, string jenkinsKey, string discordToken, string couchDbUrl, string couchDbUsername, string couchDbPassword)
        {
            JenkinsUsername = jenkinsUsername ?? throw new ArgumentNullException(nameof(jenkinsUsername));
            JenkinsKey = jenkinsKey ?? throw new ArgumentNullException(nameof(jenkinsKey));
            DiscordToken = discordToken ?? throw new ArgumentNullException(nameof(discordToken));
            CouchDbUrl = couchDbUrl ?? throw new ArgumentNullException(nameof(couchDbUrl));
            CouchDbUsername = couchDbUsername ?? throw new ArgumentNullException(nameof(couchDbUsername));
            CouchDbPassword = couchDbPassword ?? throw new ArgumentNullException(nameof(couchDbPassword));
        }
    }
}
