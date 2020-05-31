using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Discord Assistant");

            var serviceProvider = ServiceProviderFactory.CreateServiceProvider();

            var assistant = serviceProvider.GetRequiredService<Assistant>();
            await assistant.MainAsync();
        }
    }
}
