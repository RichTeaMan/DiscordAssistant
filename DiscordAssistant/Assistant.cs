using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordAssistant.Jobs;
using DiscordAssistant.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class Assistant
    {
        private readonly ILogger logger;

        private readonly IServiceProvider serviceProvider;

        private readonly DiscordSocketClient client;

        private readonly JenkinsRestClient jenkinsRestClient;

        private readonly Config config;
        private IScheduler TaskScheduler;

        public Assistant(
            ILogger<Assistant> logger,
            IServiceProvider serviceProvider,
            DiscordSocketClient client,
            JenkinsRestClient jenkinsRestClient,
            Config config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.jenkinsRestClient = jenkinsRestClient ?? throw new ArgumentNullException(nameof(jenkinsRestClient));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task MainAsync()
        {
            client.Log += _client_Log;
            client.Ready += Client_Ready;

            client.MessageReceived += _client_MessageReceived;

            await client.LoginAsync(TokenType.Bot, config.DiscordToken);
            await client.StartAsync();

            var schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
            TaskScheduler = await schedulerFactory.GetScheduler();
            TaskScheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task Client_Ready()
        {
            var jenkinsChannel = client.Guilds.SelectMany(g => g.Channels).FirstOrDefault(c => c.Name == "jenkins") as SocketTextChannel;
            if (jenkinsChannel == null)
            {
                logger.LogWarning("Jenkins channel not found");
            }
            else
            {
                await jenkinsChannel.SendMessageAsync("Booted.");
            }

            logger.LogInformation("Getting runs...");
            var jenkins = await jenkinsRestClient.FetchWorkflows();
            var runs = await jenkinsRestClient.FetchAllWorkflowRuns(jenkins);
            logger.LogInformation("Save complete...");


            // define the jobs
            // var workflowRunUpdateJob = serviceProvider.GetRequiredService<WorkflowRunUpdateJob>();
            IJobDetail job = JobBuilder.Create<WorkflowRunUpdateJob>()
                .WithIdentity("myJob", "group1")
                .Build();

            // Trigger the job to run now, and then every 40 seconds
            ITrigger workflowRunUpdateTrigger = TriggerBuilder.Create()
                .WithIdentity("myTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(3)
                    .RepeatForever())
            .Build();

            var res = await TaskScheduler.ScheduleJob(job, workflowRunUpdateTrigger);
            await TaskScheduler.Start();
            Console.WriteLine(res);
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            // ensures we don't process system/other bot messages
            if (!(arg is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }


            if (arg.Content == "status")
            {
                try
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Showing latest job statuses:");
                    var jenkins = await jenkinsRestClient.FetchWorkflows();
                    var runs = await jenkinsRestClient.FetchAllWorkflowRuns(jenkins);
                    foreach (var run in runs)
                    {
                        stringBuilder.AppendLine($"{run.fullDisplayName} - {run.result}");
                    }
                    await arg.Channel.SendMessageAsync(stringBuilder.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Exception during status message.");
                }
            }
            else
            {
                await arg.Channel.SendMessageAsync("Test. Message received!");
            }
        }
    }
}
