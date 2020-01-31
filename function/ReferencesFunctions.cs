using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public static class ReferencesFunctions
    {
        [FunctionName("ReferenceGetRiverDB")]
        public static async Task<HttpResponseMessage> RunGetRiversDB(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reference/rivers")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Rivers DB");
            
            var riversPath = Path.Combine(context.FunctionDirectory, "../Assets/europe-rivers.json");
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = File.OpenRead(riversPath);
            
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
            
        }
    }
}
