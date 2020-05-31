using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public class Hudson : JenkinsObject
    {
        public override string ClassName => "hudson.model.Hudson";

        public Assignedlabel[] assignedLabels { get; set; }
        public string mode { get; set; }
        public string nodeDescription { get; set; }
        public string nodeName { get; set; }
        public int numExecutors { get; set; }
        public object description { get; set; }
        public Overallload overallLoad { get; set; }
        public Primaryview primaryView { get; set; }

        [JsonProperty("jobs")]
        public Job[] Jobs { get; set; }
        public bool quietingDown { get; set; }
        public int slaveAgentPort { get; set; }
        public Unlabeledload unlabeledLoad { get; set; }
        public string url { get; set; }
        public bool useCrumbs { get; set; }
        public bool useSecurity { get; set; }
        public View[] views { get; set; }
    }

    public class Overallload
    {
    }

    public class Primaryview
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Unlabeledload
    {
        public string _class { get; set; }
    }

    public class Assignedlabel
    {
        public string name { get; set; }
    }

    public class Job : ApiLink
    {
        public string _class { get; set; }
        public string name { get; set; }

        public string Url { get; set; }
    }

    public class View
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

}
