using MyCouch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class DataStore : IDisposable
    {
        private MyCouchClient couchClient = null;

        private async Task setupCouchClient()
        {
            if (couchClient != null)
            {
                return;
            }
            DbConnectionInfo dbConnectionInfo = new DbConnectionInfo("http://localhost:5985", "jenkins-cache")
            {
                BasicAuth = new MyCouch.Net.BasicAuthString("admin", "password")
            };
            couchClient = new MyCouchClient(dbConnectionInfo);
            await couchClient.Database.PutAsync();
        }
        public async Task Save(string id, object data)
        {
            await setupCouchClient();

            Container container = new Container
            {
                _id = id,
                Data = data
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
            public string _id { get; set; }
            public object Data { get; set; }
        }
    }
}
