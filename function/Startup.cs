using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Surfrider.PlasticOrigins.Backend.Mobile.Startup))]
namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
#else
            // TODO: Implement SQL User Store
            //builder.Services.AddSingleton<IUserStore, SqlUserStore>();
            builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
#endif

            builder.Services.AddSingleton<IConfigurationService, EnvironmentConfigurationService>();
            builder.Services.AddSingleton<IUserService, UserService>();
        }
    }
}
