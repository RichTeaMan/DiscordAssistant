using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Discord Assistant");

            var cultureInfo = new CultureInfo("en-GB");
            cultureInfo.NumberFormat.CurrencySymbol = "£";

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var serviceProvider = ServiceProviderFactory.CreateServiceProvider();

            var assistant = serviceProvider.GetRequiredService<Assistant>();
            await assistant.MainAsync();
        }
    }
}
