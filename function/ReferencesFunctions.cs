using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public static class ReferencesFunctions
    {
        [FunctionName(nameof(GetRiversDB))]
        [OpenApiOperation(operationId: nameof(GetRiversDB), tags: new[] { "Reference data" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "An object representing all the rivers.")]

        public static async Task<HttpResponseMessage> GetRiversDB(
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
