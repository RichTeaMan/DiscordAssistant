using Discord;
using Discord.WebSocket;
using DiscordAssistant.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordAssistant.Jobs
{
    [DisallowConcurrentExecution]
    public class WorkflowRunUpdateJob : IJob, IDisposable
    {
        private readonly ILogger logger;

        private readonly StateStore stateStore;

        private readonly JenkinsRestClient jenkinsRestClient;

        private readonly DiscordSocketClient client;

        // disallow concurrent excecution doesn't seem to work...
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

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

            bool lockTaken = await semaphoreSlim.WaitAsync(0);
            if (!lockTaken)
            {
                logger.LogInformation("Update job already in progress, aborting update.");
                return;
            }
            try
            {
                var state = await stateStore.FetchState();

                var jobs = await jenkinsRestClient.FetchCachedWorkflowJobs();
                var updatedJobTasks = jobs.Select(j => jenkinsRestClient.FetchFullObject(j, false)).ToArray();

                await Task.WhenAll(updatedJobTasks);
                var updatedJobs = updatedJobTasks.Select(t => t.Result).Cast<WorkflowJob>().ToArray();
                logger.LogInformation($"{updatedJobs.Length} jobs found:\n{string.Join(",\n", updatedJobs.Select(r => r.Url).ToArray())}");
                var buildLinks = updatedJobs.SelectMany(j => j.builds).ToArray();

                var runTasks = buildLinks.Select(bl => jenkinsRestClient.FetchFullObject(bl)).ToArray();
                await Task.WhenAll(runTasks);
                var runs = runTasks.Select(t => t.Result).Select(r => r as WorkflowRun).Where(r => r != null).ToArray();

                var newRuns = runs.Where(r => r.Timestamp + r.Duration > state.LastUpdateDateTime && r.Result != null)
                    .ToArray();

                logger.LogInformation($"{newRuns.Length} new runs found:\n{string.Join(",\n", newRuns.Select(r => r.url).ToArray())}");

                var jenkinsChannel = client.Guilds.SelectMany(g => g.Channels).FirstOrDefault(c => c.Name == "jenkins") as SocketTextChannel;

                // getting timezones is not platform agnostic, so try both ways
                TimeZoneInfo timezoneInfo = TimeZoneInfo.Utc;
                var zones = TimeZoneInfo.GetSystemTimeZones();
                timezoneInfo = zones.FirstOrDefault(tz => tz.Id == "Europe/London") ??
                    zones.FirstOrDefault(tz => tz.Id == "GMT Standard Time") ??
                    TimeZoneInfo.Utc;

                logger.LogInformation($"Using timezone {timezoneInfo.Id}");
                foreach (var newRun in newRuns)
                {
                    string passEmoji = $"[{newRun.Result}]";
                    if (newRun.Result == "SUCCESS")
                    {
                        passEmoji = ":white_check_mark:";
                    }
                    else if (newRun.Result == "FAILURE")
                    {
                        passEmoji = ":octagonal_sign:";
                    }
                    else if (newRun.Result == "ABORTED")
                    {
                        passEmoji = ":no_pedestrians:";
                    }
                    else if (newRun.Result == "UNSTABLE")
                    {
                        passEmoji = ":warning:";
                    }

                    string durationStr = $"Job ran for {(newRun.Duration.Hours > 0 ? $"{newRun.Duration.Hours} hours, " : "")}" +
                        $"{(newRun.Duration.Minutes > 0 ? $"{newRun.Duration.Minutes} minutes, " : "")}" +
                        $"{(newRun.Duration.Seconds > 0 ? $"{newRun.Duration.Seconds} seconds." : "")}";

                    var localisedDateTime = TimeZoneInfo.ConvertTime(newRun.Timestamp.UtcDateTime, TimeZoneInfo.Utc, timezoneInfo);
                    string timestampStr = localisedDateTime.ToLongTimeString();
                    var embed = new EmbedBuilder
                    {
                        // Embed property can be set within object initializer
                        Title = newRun.fullDisplayName,
                        Url = newRun.url
                    };
                    await jenkinsChannel.SendMessageAsync($"{passEmoji} [{timestampStr}] {newRun.fullDisplayName}. {durationStr}", false, embed.Build());
                }
                state.LastUpdateDateTime = DateTimeOffset.Now;
                await stateStore.SaveState(state);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting Jenkins job status.");
            }
            finally
            {
                stopwatch.Stop();
                if (lockTaken)
                {
                    semaphoreSlim.Release();
                }
                logger.LogInformation($"Updating Jenkins jobs complete ({stopwatch.Elapsed.TotalSeconds} seconds).");
            }
        }

        public void Dispose()
        {
            semaphoreSlim?.Dispose();
        }
    }
}
