using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;

[assembly: FunctionsStartup(typeof(Surfrider.PlasticOrigins.Backend.Mobile.Startup))]
namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
#if DEBUG
            builder.Services.AddSingleton<IUserStore, PostgresqlUserStore>();
#else
            //builder.Services.AddSingleton<IUserStore, SqlUserStore>();
            builder.Services.AddSingleton<IUserStore, PostgresqlUserStore>();
#endif

            builder.Services.AddSingleton<IConfigurationService, EnvironmentConfigurationService>();
            builder.Services.AddSingleton<IUserService, UserService>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthenticationCore(options =>
                {
                    options.DefaultAuthenticateScheme = "Bearer";
                });
            //services.AddJwtBearer()

            //services.AddAuthentication(options =>
            //    {
            //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(options =>
            //    {
            //        options.SaveToken = true;
            //        options.RequireHttpsMetadata = false;
            //        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            //        {
            //            ValidateIssuer = true,
            //            ValidateAudience = true,
            //            ValidAudience = "http://dotnetdetail.net",
            //            ValidIssuer = "http://dotnetdetail.net",
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SecureKey"))
            //        };
            //    });
        }
    }
}
