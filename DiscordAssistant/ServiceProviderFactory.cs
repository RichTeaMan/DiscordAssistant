﻿using Discord.WebSocket;
using DiscordAssistant.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordAssistant
{
    public class ServiceProviderFactory
    {
        public static IServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddUserSecrets("857fc796-f1e6-4f51-a86b-a0f1172872e3");
            var config = builder.Build();

            string jenkinsUsername = config.GetValue<string>("jenkins:username");
            string jenkinsKey = config.GetValue<string>("jenkins:key");
            string discordToken = config.GetValue<string>("discord:token");

            serviceCollection.AddTransient(sp =>
            {
                return new Config(jenkinsUsername, jenkinsKey, discordToken);
            });

            serviceCollection.AddLogging(configure => { configure.AddConfiguration(config.GetSection("Logging")); configure.AddConsole(); });
            serviceCollection.AddSingleton<Assistant>();
            serviceCollection.AddSingleton<DiscordSocketClient>();
            serviceCollection.AddSingleton<JenkinsRestClient>();
            serviceCollection.AddSingleton<JenkinsDeserialiser>();
            serviceCollection.AddSingleton<DataStore>();
            serviceCollection.AddSingleton<StateStore>();

            // Add Quartz services
            serviceCollection.AddSingleton<IJobFactory, SingletonJobFactory>();
            serviceCollection.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            serviceCollection.AddSingleton<WorkflowRunUpdateJob>();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
