using MyCouch;
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
            DbConnectionInfo dbConnectionInfo = new DbConnectionInfo("http://localhost:5985", "workflow-runs")
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

        public void Dispose()
        {
            couchClient?.Dispose();
        }

        class Container
        {
            public string _id { get; set; }
            public object Data { get; set; }
        }
    }
}
