using DiscordAssistant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DiscordAssistant
{
    public class JenkinsDeserialiser
    {
        private Dictionary<string, Type> typeMap = null;

        public JenkinsObject Deserialise(string json)
        {
            JObject o = JObject.Parse(json);

            string className = (string)o.SelectToken("_class");

            Type jenkinsObjectType;
            
            if (!fetchTypeMap().TryGetValue(className, out jenkinsObjectType))
            {
                Console.WriteLine($"Unknown type '{className}'.");
                jenkinsObjectType = typeof(UnknownJenkinsObject);
            }

            JenkinsObject jenkinsObject = (JenkinsObject)JsonConvert.DeserializeObject(json, jenkinsObjectType);

            return jenkinsObject;
        }

        private Dictionary<string, Type> fetchTypeMap()
        {
            if (this.typeMap == null)
            {
                var typeMap = new Dictionary<string, Type>();
                foreach (Type type in Assembly.GetAssembly(typeof(Program)).GetTypes().Where(myType =>
                    myType.IsClass &&
                    myType != typeof(UnknownJenkinsObject) &&
                    !myType.IsAbstract &&
                    myType.IsSubclassOf(typeof(JenkinsObject))))
                {
                    var jenkinsObject = (JenkinsObject)Activator.CreateInstance(type);
                    typeMap.Add(jenkinsObject.ClassName, type);
                }
                this.typeMap = typeMap;
            }
            return this.typeMap;
        }
    }
}
