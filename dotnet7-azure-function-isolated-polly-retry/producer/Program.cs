using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using producer;
using producer.Helpers;
using producer.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((ctx, cfg) =>
    {
        var connectionString = ctx.Configuration.GetConnectionString(Constants.TableStorage);
        cfg.AddSingleton<ITableStorageHelper>(new TableStorageHelper(connectionString));
    })
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(ctx.HostingEnvironment.ContentRootPath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        cfg.AddConfiguration(config);
    })
    .Build();

host.Run();
