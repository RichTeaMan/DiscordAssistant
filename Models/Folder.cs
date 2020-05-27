using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public class Folder : JenkinsObject
    {
        public override string ClassName => "com.cloudbees.hudson.plugins.folder.Folder";

        public FolderAction[] actions { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public object displayNameOrNull { get; set; }
        public string fullDisplayName { get; set; }
        public string fullName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public FolderHealthreport[] healthReport { get; set; }
        
        [JsonProperty("jobs")]
        public FolderJob[] Jobs { get; set; }
        public FolderPrimaryview primaryView { get; set; }
        public FolderView[] views { get; set; }
    }

    public class FolderPrimaryview
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class FolderAction
    {
        public string _class { get; set; }
    }

    public class FolderHealthreport
    {
        public string description { get; set; }
        public string iconClassName { get; set; }
        public string iconUrl { get; set; }
        public int score { get; set; }
    }

    public class FolderJob : Job
    {
        public string color { get; set; }
    }

    public class FolderView
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

}
