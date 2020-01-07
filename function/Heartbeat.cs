using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public static class Heartbeat
    {
        [FunctionName("Heartbeat")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "heartbeat")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Heartbeat request");

            return (ActionResult)new OkObjectResult(
                new
                {
                    Status = "OK",
                    Dependencies = new
                    {
                        Storage = "Unknown",
                        Table = "Unknown",
                        Logger = "Unknown"
                    }
                });
        }
    }
}
