using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using producer.Interfaces;

namespace producer.Functions
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
        public HttpResponseData Increment([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Request to increment counter.");

            try
            {
                var isFailureEnabled = _configuration.GetValue<bool>(Constants.FailureEnabled); // variable to control failure injection
                var isRandomFailureEnabled = _configuration.GetValue<bool>(Constants.RandomFailureEnabled); // variable to control random failure injection
                var currentCounter = _tableStorageHelper.GetCounter(Constants.Counter, Constants.PartitionKey, Constants.Row);

                if (IsFailureEnabledWithMod(isFailureEnabled, currentCounter, 3))
                {
                    const string errorMessage = "Counter is divisible by 3, throwing exception.";
                    throw new Exception(errorMessage);
                }

                if (IsFailureEnabledWithRandom(isRandomFailureEnabled))
                {
                    const string errorMessage = "Random exception raised.";
                    throw new Exception(errorMessage);
                }

                var counter = _tableStorageHelper.IncrementCounter(Constants.Counter, Constants.PartitionKey, Constants.Row);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                var message = $"Current counter: {counter}";
                _logger.LogInformation(message);
                response.WriteString(message);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                response.WriteString(ex.Message);
                return response;
            }
        }

        private static bool IsFailureEnabledWithMod(bool isFailureEnabled, int currentCounter, int modNumber)
        {
            return (isFailureEnabled && currentCounter % modNumber == 0);
        }

        private static bool IsFailureEnabledWithRandom(bool isRandomFailureEnabled)
        {
            // Generate a random number (0 - false or 1 - true)
            var randomNumber = new Random().Next(2);

            return (isRandomFailureEnabled && randomNumber == 1);
        }
    }
}