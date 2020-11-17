using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordAssistant
{
    public class LambdaRetry
    {
        private readonly ILogger logger;

        public LambdaRetry(ILogger<LambdaRetry> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> Retry<T>(Func<Task<T>> func)
        {
            int maxAttempts = 10;
            Exception prevException = null;
            foreach (var i in Enumerable.Range(0, maxAttempts))
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    prevException = ex;
                    logger.LogWarning($"Error contacting CouchDB.\n{ex}");

                    // wait before next attempt.
                    await Task.Delay(30 * 1000);
                }
            }
            throw prevException;
        }
    }
}
