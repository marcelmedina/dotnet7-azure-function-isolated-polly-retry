using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((services) =>
    {
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
            .RetryAsync(3, onRetry: (message, retryCount) =>
            {
                Console.Out.WriteLine($"### RequestMessage: {message.Result.RequestMessage}");
                Console.Out.WriteLine($"### Content: {message.Result.Content.ReadAsStringAsync().Result}");
                Console.Out.WriteLine($"### ReasonPhrase: {message.Result.ReasonPhrase}");
                Console.Out.WriteLine($"### Retry: {retryCount}");
            });

        services.AddHttpClient("PollyRetry")
            .AddPolicyHandler(retryPolicy);
    })
    .Build();

host.Run();