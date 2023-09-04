using System.Net;
using consumer.TypedHttpClient;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace consumer.Functions
{
    public class HttpTriggered
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly StateCounterHttpClient _httpClient;
        private readonly ILogger _logger;

        public HttpTriggered(IHttpClientFactory httpClientFactory,
            IConfiguration configuration, StateCounterHttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger<HttpTriggered>();
        }

        [Function("HttpTriggered")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            try
            {
                var endpoint = _configuration.GetValue<string>(Constants.ProducerEndpoint);

                var increment = await _httpClient.GetStateCounter(endpoint);

                _logger.LogInformation($"### Increment = {increment}");

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
