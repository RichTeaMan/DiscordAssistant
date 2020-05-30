using DiscordAssistant.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class JenkinsRestClient : IDisposable
    {
        private readonly HttpClient httpClient = new HttpClient();

        private readonly Config config;

        private readonly JenkinsDeserialiser jenkinsDeserialiser;

        public JenkinsRestClient(Config config, JenkinsDeserialiser jenkinsDeserialiser)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.jenkinsDeserialiser = jenkinsDeserialiser ?? throw new ArgumentNullException(nameof(jenkinsDeserialiser));
        }

        public async Task<Hudson> FetchWorkflows()
        {
            using var request = CreateJenkinsRequest("https://jenkins.richteaman.com/api/json");
            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var workflowJob = JsonConvert.DeserializeObject<Hudson>(json);
            return workflowJob;
        }

        public async Task<JenkinsObject> FetchWorkflowJobs(Job job)
        {
            using var request = CreateJenkinsRequest($"{job.Url}api/json");
            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return jenkinsDeserialiser.Deserialise(json);
        }

        public async Task<JenkinsObject> FetchWorkflowRun(Build build)
        {
            string url = $"{build.Url}api/json";
            using var request = CreateJenkinsRequest(url);
            using var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return jenkinsDeserialiser.Deserialise(json);
        }

        /// <summary>
        /// Recursively searches for all workflow runs.
        /// </summary>
        /// <param name="jenkinsObject"></param>
        /// <returns></returns>
        public async Task<List<WorkflowRun>> FetchAllWorkflowRuns(JenkinsObject jenkinsObject)
        {
            List<WorkflowRun> runs = new List<WorkflowRun>();
            List<Task<List<WorkflowRun>>> tasks = new List<Task<List<WorkflowRun>>>();
            switch (jenkinsObject)
            {
                case Hudson h:
                    tasks.AddRange(h.Jobs.Select(j => FetchAllWorkflowRuns(j)));
                    break;
                case Folder f:
                    tasks.AddRange(f.Jobs.Select(j => FetchAllWorkflowRuns(j)));
                    break;
                case WorkflowJob wj:
                    tasks.AddRange(wj.builds.Take(1).Select(j => FetchAllWorkflowRuns(j)));
                    break;
                case WorkflowRun wr:
                    runs.Add(wr);
                    break;
                default:
                    Console.WriteLine($"Unknown Jenkins object type: {jenkinsObject.GetType().FullName}.");
                    break;
            }

            await Task.WhenAll(tasks);
            runs.AddRange(tasks.SelectMany(t => t.Result));

            return runs;
        }

        public async Task<List<WorkflowRun>> FetchAllWorkflowRuns(ApiLink apiLink)
        {
            List<WorkflowRun> runs = new List<WorkflowRun>();
            switch (apiLink)
            {
                case Job j:
                    {
                        var job = await FetchWorkflowJobs(j);
                        var r = await FetchAllWorkflowRuns(job);
                        runs.AddRange(r);
                    }
                    break;
                case Build b:
                    {
                        var run = await FetchWorkflowRun(b);
                        var r = await FetchAllWorkflowRuns(run);
                        runs.AddRange(r);
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown API link type: {apiLink.GetType().FullName}.");
                    break;
            }
            return runs;
        }

        private HttpRequestMessage CreateJenkinsRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            string basicAuthToken = Convert.ToBase64String(Encoding.Default.GetBytes(config.JenkinsUsername + ":" + config.JenkinsKey));
            request.Headers.Add("Authorization", "Basic " + basicAuthToken);
            return request;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
