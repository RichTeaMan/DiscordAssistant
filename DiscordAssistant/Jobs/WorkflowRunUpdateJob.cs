using Discord.WebSocket;
using DiscordAssistant.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant.Jobs
{
    [DisallowConcurrentExecution]
    public class WorkflowRunUpdateJob : IJob
    {
        private readonly ILogger logger;

        private readonly StateStore stateStore;

        private readonly JenkinsRestClient jenkinsRestClient;

        private readonly DiscordSocketClient client;

        public WorkflowRunUpdateJob(ILogger<WorkflowRunUpdateJob> logger,
            StateStore stateStore,
            JenkinsRestClient jenkinsRestClient,
            DiscordSocketClient client)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            this.jenkinsRestClient = jenkinsRestClient ?? throw new ArgumentNullException(nameof(jenkinsRestClient));
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Updating Jenkins jobs.");

            var state = await stateStore.FetchState();

            var jobs = await jenkinsRestClient.FetchCachedWorkflowJobs();
            var updatedJobTasks = jobs.Select(j => jenkinsRestClient.FetchFullObject(j, false)).ToArray();
            await Task.WhenAll(updatedJobTasks);
            var updatedJobs = updatedJobTasks.Select(t => t.Result).Cast<WorkflowJob>().ToArray();
            var buildLinks = updatedJobs.SelectMany(j => j.builds).ToArray();

            var runTasks = buildLinks.Select(bl => jenkinsRestClient.FetchFullObject(bl)).ToArray();
            await Task.WhenAll(runTasks);
            var runs = runTasks.Select(t => t.Result).Cast<WorkflowRun>().ToArray();

            var newRuns = runs.Where(r => r.Timestamp > state.LastUpdateDateTime).ToArray();

            var jenkinsChannel = client.Guilds.SelectMany(g => g.Channels).FirstOrDefault(c => c.Name == "jenkins") as SocketTextChannel;
            foreach(var newRun in newRuns)
            {
                await jenkinsChannel.SendMessageAsync($"{newRun.fullDisplayName} - {newRun.number}: {newRun.result}");
            }
            state.LastUpdateDateTime = DateTimeOffset.Now;
            await stateStore.SaveState(state);

            stopwatch.Stop();
            logger.LogInformation($"Updating Jenkins jobs complete ({stopwatch.Elapsed.TotalSeconds}).");
        }
    }
}
