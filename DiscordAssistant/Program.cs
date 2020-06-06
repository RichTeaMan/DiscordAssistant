using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Discord Assistant");
            Console.WriteLine($"Version {VersionNumber}");

            var serviceProvider = ServiceProviderFactory.CreateServiceProvider();

            var assistant = serviceProvider.GetRequiredService<Assistant>();
            await assistant.MainAsync();
        }

        public static string VersionNumber => typeof(Program).Assembly
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
          .InformationalVersion;
    }
}
