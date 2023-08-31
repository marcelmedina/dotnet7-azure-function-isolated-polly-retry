using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace consumer
{
    public class TimerTriggered
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TimerTriggered(IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [Function("TimerTriggered")]
        public async Task Run([TimerTrigger("*/10 * * * * *")] FunctionContext context)
        {
            var logger = context.GetLogger<TimerTriggered>();

            var httpClient = _httpClientFactory.CreateClient("PollyRetry");

            var endpoint = _configuration.GetValue<string>("Values:ProducerEndpoint");

            var increment = await httpClient.GetStringAsync(endpoint);

            logger.LogInformation($"### Increment = {increment}");
        }
    }
}
