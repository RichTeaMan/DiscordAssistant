using DiscordAssistant.Models;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;

        private readonly HttpClient httpClient = new HttpClient();

        private readonly Config config;

        private readonly JenkinsDeserialiser jenkinsDeserialiser;

        private readonly DataStore dataStore;

        private readonly LambdaRetry lambdaRetry;

        public JenkinsRestClient(ILogger<JenkinsRestClient> logger,
            Config config,
            JenkinsDeserialiser jenkinsDeserialiser,
            DataStore dataStore,
            LambdaRetry lambdaRetry)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.jenkinsDeserialiser = jenkinsDeserialiser ?? throw new ArgumentNullException(nameof(jenkinsDeserialiser));
            this.dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            this.lambdaRetry = lambdaRetry ?? throw new ArgumentNullException(nameof(lambdaRetry));
        }

        public async Task<Hudson> FetchWorkflows()
        {
            var response = await lambdaRetry.Retry(async () =>
            {
                using var request = CreateJenkinsRequest("https://jenkins.richteaman.com/api/json");
                return await httpClient.SendAsync(request);
            });

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var workflowJob = JsonConvert.DeserializeObject<Hudson>(json);
            return workflowJob;
        }

        /// <summary>
        /// Fetches the full object from an ApiLink object.
        ///
        /// Many representations in the Jenkins API will be a reference to an object. This
        /// method will resolve that link and serialise into an appropriate form.
        ///
        /// Objects can be optionally retrieved from a cache. A newly acquired response
        /// will always update the cache.
        /// </summary>
        /// <param name="apiLink"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public async Task<JenkinsObject> FetchFullObject(ApiLink apiLink, bool useCache = true)
        {
            string json = null;
            var uri = new Uri($"{apiLink.Url}api/json");
            if (useCache)
            {
                try
                {
                    var cacheResponse = await lambdaRetry.Retry(async () => await dataStore.Load(uri.ToString()));
                    if (cacheResponse.IsSuccess)
                    {
                        json = cacheResponse.Content;
                    }
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "Could not contact CouchDB. Is it switched on with correct credentials?");
                }
            }

            if (json == null)
            {
                using var response = await lambdaRetry.Retry(async () =>
                {
                    using var request = CreateJenkinsRequest(uri.ToString());
                    return await httpClient.SendAsync(request);
                });
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
                var jenkinsObject = jenkinsDeserialiser.Deserialise(json);
                bool shouldCache = true;
                if (jenkinsObject is WorkflowRun wr)
                {
                    shouldCache = wr.Result != null;
                }
                if (shouldCache)
                {
                    await dataStore.Save(uri.ToString(), jenkinsObject);
                }
            }

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
                    logger.LogWarning($"Unknown Jenkins object type: {jenkinsObject.GetType().FullName}:\n{JsonConvert.SerializeObject(jenkinsObject, Formatting.Indented)}");
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
                case ApiLink link:
                    {
                        var jenkinsObject = await FetchFullObject(link);
                        var r = await FetchAllWorkflowRuns(jenkinsObject);
                        runs.AddRange(r);
                    }
                    break;
                default:
                    logger.LogWarning($"Unknown API link type: {apiLink.GetType().FullName}.");
                    break;
            }
            return runs;
        }

        public async Task<IReadOnlyCollection<WorkflowJob>> FetchCachedWorkflowJobs()
        {
            var jobJsons = await dataStore.Find("jenkinsDataType", "org.jenkinsci.plugins.workflow.job.WorkflowJob");
            var jobs = jobJsons.Select(j => JsonConvert.DeserializeObject<WorkflowJob>(j)).ToArray();
            return jobs;
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
