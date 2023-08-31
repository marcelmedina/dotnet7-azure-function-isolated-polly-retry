using System.Net.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace consumer
{
    public class TimerTriggered
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public TimerTriggered(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = loggerFactory.CreateLogger<TimerTriggered>();
        }

        [Function("TimerTriggered")]
        public async Task Run([TimerTrigger("*/10 * * * * *")] object param)
        {
            var httpClient = _httpClientFactory.CreateClient("PollyRetry");

            var increment = await httpClient.GetStringAsync(
                Environment.GetEnvironmentVariable("ProducerEndpoint"));

            _logger.LogInformation($"### Increment = {increment}");
        }
    }
}
