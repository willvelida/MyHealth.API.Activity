using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHealth.API.Activity;
using MyHealth.API.Activity.Services;
using MyHealth.API.Activity.Validators;
using MyHealth.Common;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyHealth.API.Activity
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddLogging();

            builder.Services.AddSingleton(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new CosmosClient(configuration["CosmosDBConnectionString"]);
            });

            builder.Services.AddSingleton<IServiceBusHelpers>(sp =>
            {
                IConfiguration configuration = sp.GetService<IConfiguration>();
                return new ServiceBusHelpers(configuration["ServiceBusConnectionString"]);
            });

            builder.Services.AddScoped<IActivityDbService, ActivityDbService>();
            builder.Services.AddScoped<IDateValidator, DateValidator>();
        }
    }
}
