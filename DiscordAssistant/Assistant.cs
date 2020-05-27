using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordAssistant.Models;
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
            client.Ready += Client_Ready;

            client.MessageReceived += _client_MessageReceived;

            await client.LoginAsync(TokenType.Bot, config.DiscordToken);
            await client.StartAsync();


            
            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private async Task Client_Ready()
        {
            var jenkinsChannel = client.Guilds.SelectMany(g => g.Channels).FirstOrDefault(c => c.Name == "jenkins") as SocketTextChannel;
            if (jenkinsChannel == null)
            {
                Console.WriteLine("Jenkins channel not found");
            }
            else
            {
                await jenkinsChannel.SendMessageAsync("Booted.");
            }
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
