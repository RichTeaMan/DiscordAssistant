using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public class WorkflowRun : JenkinsObject
    {
        public override string ClassName => "org.jenkinsci.plugins.workflow.job.WorkflowRun";
        public WorkflowRunAction[] actions { get; set; }
        public object[] artifacts { get; set; }
        public bool building { get; set; }
        public object description { get; set; }
        public string displayName { get; set; }
        public int duration { get; set; }
        public int estimatedDuration { get; set; }
        public object executor { get; set; }
        public string fullDisplayName { get; set; }
        public string id { get; set; }
        public bool keepLog { get; set; }
        public int number { get; set; }
        public int queueId { get; set; }
        public string result { get; set; }

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixEpochMillisecondJsonConverter))]
        public DateTimeOffset Timestamp { get; set; }
        public string url { get; set; }
        public object[] changeSets { get; set; }
        public object[] culprits { get; set; }
        public Nextbuild nextBuild { get; set; }
        public Previousbuild previousBuild { get; set; }
    }

    public class Nextbuild
    {
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Previousbuild
    {
        public int number { get; set; }
        public string url { get; set; }
    }

    public class WorkflowRunAction
    {
        public string _class { get; set; }
        public Caus[] causes { get; set; }
        public Parameter[] parameters { get; set; }
        public Buildsbybranchname buildsByBranchName { get; set; }
        public Lastbuiltrevision lastBuiltRevision { get; set; }
        public string[] remoteUrls { get; set; }
        public string scmName { get; set; }
    }

    public class Buildsbybranchname
    {
        public RefsRemotesOriginMaster refsremotesoriginmaster { get; set; }
    }

    public class RefsRemotesOriginMaster
    {
        public string _class { get; set; }
        public int buildNumber { get; set; }
        public object buildResult { get; set; }
        public Marked marked { get; set; }
        public Revision revision { get; set; }
    }

    public class Marked
    {
        public string SHA1 { get; set; }
        public Branch[] branch { get; set; }
    }

    public class Branch
    {
        public string SHA1 { get; set; }
        public string name { get; set; }
    }

    public class Revision
    {
        public string SHA1 { get; set; }
        public Branch1[] branch { get; set; }
    }

    public class Branch1
    {
        public string SHA1 { get; set; }
        public string name { get; set; }
    }

    public class Lastbuiltrevision
    {
        public string SHA1 { get; set; }
        public Branch2[] branch { get; set; }
    }

    public class Branch2
    {
        public string SHA1 { get; set; }
        public string name { get; set; }
    }

    public class Caus
    {
        public string _class { get; set; }
        public string shortDescription { get; set; }
    }

    public class Parameter
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

}
