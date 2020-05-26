using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class Assistant
    {
        private readonly DiscordSocketClient client;

        private readonly JenkinsRestClient jenkinsRestClient;

        private readonly Config config;

        public Assistant(DiscordSocketClient client, JenkinsRestClient jenkinsRestClient, Config config)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.jenkinsRestClient = jenkinsRestClient ?? throw new ArgumentNullException(nameof(jenkinsRestClient));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task MainAsync()
        {
            client.Log += _client_Log;

            client.MessageReceived += _client_MessageReceived;

            await client.LoginAsync(TokenType.Bot, config.DiscordToken);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
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
                    var workflowJobs = await jenkinsRestClient.FetchWorkflowJobs();
                    var lastRun = await jenkinsRestClient.FetchWorkflowRun(workflowJobs.builds.FirstOrDefault());
                    await arg.Channel.SendMessageAsync($"{workflowJobs.displayName}, {lastRun.result}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                await arg.Channel.SendMessageAsync("Test. Message received!");
            }
        }
    }
}
