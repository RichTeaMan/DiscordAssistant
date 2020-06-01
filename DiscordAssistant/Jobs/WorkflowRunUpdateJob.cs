﻿using Discord;
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

            try
            {
                var state = await stateStore.FetchState();

                var jobs = await jenkinsRestClient.FetchCachedWorkflowJobs();
                var updatedJobTasks = jobs.Select(j => jenkinsRestClient.FetchFullObject(j, false)).ToArray();
                await Task.WhenAll(updatedJobTasks);
                var updatedJobs = updatedJobTasks.Select(t => t.Result).Cast<WorkflowJob>().ToArray();
                var buildLinks = updatedJobs.SelectMany(j => j.builds).ToArray();

                var runTasks = buildLinks.Select(bl => jenkinsRestClient.FetchFullObject(bl)).ToArray();
                await Task.WhenAll(runTasks);
                var runs = runTasks.Select(t => t.Result).Cast<WorkflowRun>().ToArray();

                var newRuns = runs.Where(r => r.Timestamp + r.Duration > state.LastUpdateDateTime && r.Result != null).GroupBy(r => r.url).First().ToArray();

                var jenkinsChannel = client.Guilds.SelectMany(g => g.Channels).FirstOrDefault(c => c.Name == "jenkins") as SocketTextChannel;


                // getting timezones is not platform agnostic, so try both ways
                TimeZoneInfo timezoneInfo = TimeZoneInfo.Utc;
                var zones = TimeZoneInfo.GetSystemTimeZones();
                timezoneInfo = zones.FirstOrDefault(tz => tz.Id == "Europe/London") ??
                    zones.FirstOrDefault(tz => tz.Id == "GMT Standard Time") ??
                    TimeZoneInfo.Utc;

                // GMT Standard Time
                logger.LogInformation("zones:");
                foreach(var tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    logger.LogInformation($"{tz.Id} - {tz.DisplayName}");
                }
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

                    var localisedDateTime = TimeZoneInfo.ConvertTime(newRun.Timestamp.UtcDateTime, TimeZoneInfo.Utc, timezoneInfo);
                    string timestampStr = localisedDateTime.ToLongTimeString();
                    var embed = new EmbedBuilder
                    {
                        // Embed property can be set within object initializer
                        Title = newRun.fullDisplayName,
                        Url = newRun.url
                    };
                    await jenkinsChannel.SendMessageAsync($"{passEmoji} [{timestampStr}] {newRun.fullDisplayName}", false, embed.Build());
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
                logger.LogInformation($"Updating Jenkins jobs complete ({stopwatch.Elapsed.TotalSeconds} seconds).");
            }
        }
    }
}
