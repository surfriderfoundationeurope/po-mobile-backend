using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;

[assembly: WebJobsStartup(typeof(Surfrider.PlasticOrigins.Backend.Mobile.WebJobsStartup))]
namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class WebJobsStartup : IWebJobsStartup
    {
        void IWebJobsStartup.Configure(IWebJobsBuilder builder)
        {
            Debug.WriteLine("WebJobsStartup.Configure");
            builder.AddExtension<AccessTokenExtensionProvider>();
        }
    }
}