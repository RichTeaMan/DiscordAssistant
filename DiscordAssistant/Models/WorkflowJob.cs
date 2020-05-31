using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public class WorkflowJob : JenkinsObject, ApiLink
    {
        public override string ClassName => "org.jenkinsci.plugins.workflow.job.WorkflowJob";

        public Action[] actions { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public object displayNameOrNull { get; set; }
        public string fullDisplayName { get; set; }
        public string fullName { get; set; }
        public string name { get; set; }
        public string Url { get; set; }
        public bool buildable { get; set; }
        public Build[] builds { get; set; }
        public string color { get; set; }
        public Firstbuild firstBuild { get; set; }
        public Healthreport[] healthReport { get; set; }
        public bool inQueue { get; set; }
        public bool keepDependencies { get; set; }
        public Lastbuild lastBuild { get; set; }
        public Lastcompletedbuild lastCompletedBuild { get; set; }
        public Lastfailedbuild lastFailedBuild { get; set; }
        public Laststablebuild lastStableBuild { get; set; }
        public Lastsuccessfulbuild lastSuccessfulBuild { get; set; }
        public object lastUnstableBuild { get; set; }
        public Lastunsuccessfulbuild lastUnsuccessfulBuild { get; set; }
        public int nextBuildNumber { get; set; }
        public Property1[] property { get; set; }
        public object queueItem { get; set; }
        public bool concurrentBuild { get; set; }
        public bool resumeBlocked { get; set; }
    }

    public class Firstbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Lastbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Lastcompletedbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Lastfailedbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Laststablebuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Lastsuccessfulbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Lastunsuccessfulbuild
    {
        public string _class { get; set; }
        public int number { get; set; }
        public string url { get; set; }
    }

    public class Action
    {
        public string _class { get; set; }
    }

    public class Build : ApiLink
    {
        public string _class { get; set; }
        public int number { get; set; }

        public string Url { get; set; }
    }

    public class Healthreport
    {
        public string description { get; set; }
        public string iconClassName { get; set; }
        public string iconUrl { get; set; }
        public int score { get; set; }
    }

    public class Property1
    {
        public string _class { get; set; }
        public Parameterdefinition[] parameterDefinitions { get; set; }
    }

    public class Parameterdefinition
    {
        public string _class { get; set; }
        public Defaultparametervalue defaultParameterValue { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Defaultparametervalue
    {
        public string _class { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }

}
