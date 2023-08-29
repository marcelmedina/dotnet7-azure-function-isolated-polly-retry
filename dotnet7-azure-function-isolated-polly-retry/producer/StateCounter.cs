using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using producer.Interfaces;

namespace producer
{
    public class StateCounter
    {
        private readonly ITableStorageHelper _tableStorageHelper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public StateCounter(ILoggerFactory loggerFactory, ITableStorageHelper tableStorageHelper,
            IConfiguration configuration)
        {
            _tableStorageHelper = tableStorageHelper;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<StateCounter>();
        }

        [Function(nameof(Increment))]
        public HttpResponseData Increment([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Request to increment counter.");

            var isFailureEnabled = bool.Parse(_configuration[Constants.FailureEnabled]);
            var currentCounter = _tableStorageHelper.GetCounter(Constants.Counter, Constants.PartitionKey, Constants.Row);

            if (isFailureEnabled && currentCounter % 3 == 0)
            {
                const string errorMessage = "Counter is divisible by 3, throwing exception.";

                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var counter =
                _tableStorageHelper.IncrementCounter(Constants.Counter, Constants.PartitionKey, Constants.Row);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var message = $"Current counter: {counter}";
            _logger.LogInformation(message);
            response.WriteString(message);

            return response;
        }
    }
}
