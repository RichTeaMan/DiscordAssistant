using Discord;
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
    public class WorkflowUpdateJob : IJob
    {
        private readonly ILogger logger;

        private readonly JenkinsRestClient jenkinsRestClient;

        public WorkflowUpdateJob(ILogger<WorkflowUpdateJob> logger,
            JenkinsRestClient jenkinsRestClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.jenkinsRestClient = jenkinsRestClient ?? throw new ArgumentNullException(nameof(jenkinsRestClient));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Updating Jenkins workflows.");

            try
            {
                var jenkins = await jenkinsRestClient.FetchWorkflows();
                var runs = await jenkinsRestClient.FetchAllWorkflowRuns(jenkins);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting Jenkins workflows data.");
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation($"Updating Jenkins workflows complete ({stopwatch.Elapsed.TotalSeconds} seconds).");
            }
        }
    }
}
