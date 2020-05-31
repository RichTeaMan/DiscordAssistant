using DiscordAssistant.Models;
using MyCouch;
using MyCouch.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class DataStore : IDisposable
    {
        private readonly Config config;

        public DataStore(Config config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private MyCouchClient couchClient = null;

        private async Task setupCouchClient()
        {
            if (couchClient != null)
            {
                return;
            }
            DbConnectionInfo dbConnectionInfo = new DbConnectionInfo(config.CouchDbUrl, "discord-assistant-store")
            {
                BasicAuth = new MyCouch.Net.BasicAuthString(config.CouchDbUsername, config.CouchDbPassword)
            };
            couchClient = new MyCouchClient(dbConnectionInfo);
            await couchClient.Database.PutAsync();
        }
        public async Task Save(string id, object data)
        {
            await setupCouchClient();

            // check if an update is required
            string revision = null;
            var r = await couchClient.Documents.GetAsync(id);
            if (r.IsSuccess)
            {
                revision = r.Rev;
            }

            string jenkinsDataType = "not-jenkins-object";
            var jenkinsObject = data as JenkinsObject;
            if (jenkinsObject != null)
            {
                jenkinsDataType = jenkinsObject.ClassName;
            }

            Container container = new Container
            {
                Id = id,
                Rev = revision,
                Data = data,
                JenkinsDataType = jenkinsDataType
            };

            await couchClient.Entities.PutAsync(container);

        }

        public async Task<LoadResponse> Load(string id)
        {
            await setupCouchClient();

            var r = await couchClient.Documents.GetAsync(id);
            string content = null;
            if (r.IsSuccess)
            {
                JObject o = JObject.Parse(r.Content);
                var jsonData = o.SelectToken("data");
                var data = JsonConvert.SerializeObject(jsonData);
                content = data;
            }
            return new LoadResponse(r.IsSuccess, content);
        }

        public async Task<IReadOnlyCollection<string>> Find(string key, string value)
        {

            string selector = $"{{ \"{key}\": \"{value}\" }}";

            var findRequest = new FindRequest
            {
                SelectorExpression = selector
            };
            var findResponse = await couchClient.Queries.FindAsync(findRequest);
            var jObjects = findResponse.Docs.Select(d =>
            {
                JObject o = JObject.Parse(d);
                var jsonData = o.SelectToken("data");
                var data = JsonConvert.SerializeObject(jsonData);
                return data;
            }).ToList().AsReadOnly();
            return jObjects;
        }

        public void Dispose()
        {
            couchClient?.Dispose();
        }

        public class LoadResponse
        {
            public bool IsSuccess { get; }

            public string Content { get; }

            public LoadResponse(bool isSuccess, string content)
            {
                IsSuccess = isSuccess;
                Content = content;
            }
        }

        class Container
        {
            [JsonProperty("_id")]
            public string Id { get; set; }

            [JsonProperty("_rev")]
            public string Rev { get; set; }

            public object Data { get; set; }

            public DateTimeOffset StorageDateTime { get; set; } = DateTimeOffset.UtcNow;

            public string JenkinsDataType { get; set; }
        }
    }
}
