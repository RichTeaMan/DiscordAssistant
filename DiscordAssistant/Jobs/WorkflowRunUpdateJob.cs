using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
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

        public WorkflowRunUpdateJob(ILogger<WorkflowRunUpdateJob> logger, StateStore stateStore, JenkinsRestClient jenkinsRestClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            this.jenkinsRestClient = jenkinsRestClient ?? throw new ArgumentNullException(nameof(jenkinsRestClient));
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("WORKFLOW JOB!!!");
            return Task.CompletedTask;
        }
    }
}
