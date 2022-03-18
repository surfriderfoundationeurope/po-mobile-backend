using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class OpenAPIConfig : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = "1.0.0",
            Title = "Plastic Origins Labelling API",
            Description = "HTTP APIs for Surfrider Plastic Origins Labelling platform.",
            TermsOfService = new Uri("https://github.com/surfriderfoundationeurope/po-mobile-backend"),
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("http://opensource.org/licenses/MIT"),
            }
        };

        public override List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>()
        {
            new OpenApiServer() { Url = "https://api.dev.trashroulette.com/" },
            new OpenApiServer() { Url = "https://api.trashroulette.com/" }
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;

    }
}
