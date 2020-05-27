using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant.Models
{
    public class UnknownJenkinsObject : JenkinsObject
    {
        public override string ClassName => _class;
    }
}
