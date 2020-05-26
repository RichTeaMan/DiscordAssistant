using DiscordAssistant.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class JenkinsRestClient : IDisposable
    {
        private readonly HttpClient httpClient = new HttpClient();

        private readonly Config config;

        public JenkinsRestClient(Config config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<WorkflowJob> FetchWorkflowJobs()
        {
            using var request = CreateJenkinsRequest("https://jenkins.richteaman.com/job/deathclock/job/news-crawler/api/json");
            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var workflowJob = JsonConvert.DeserializeObject<WorkflowJob>(json);
            return workflowJob;
        }

        private HttpRequestMessage CreateJenkinsRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes(config.JenkinsUsername + ":" + config.JenkinsKey));
            request.Headers.Add("Authorization", "Basic " + basicAuthToken);
            return request;
        }

        public async Task<WorkflowRun> FetchWorkflowRun(Build build)
        {
            string url = $"{build.url}api/json";
            using var request = CreateJenkinsRequest(url);
            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var workflowRun = JsonConvert.DeserializeObject<WorkflowRun>(json);
            return workflowRun;

        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
