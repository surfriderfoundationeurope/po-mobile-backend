using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class Heartbeat
    {
        private IUserStore _userStore;

        public Heartbeat(IUserStore userStore)
        {
            _userStore = userStore;
        }

        [FunctionName("Heartbeat")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "heartbeat")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Heartbeat request");

            var userStorageAvailability = _userStore.CheckAvailability();

            return (ActionResult)new OkObjectResult(
                new
                {
                    Status = "OK",
                    Dependencies = new
                    {
                        UserStorage = userStorageAvailability,
                        Table = "Unknown",
                        Logger = "Unknown"
                    }
                });
        }
    }
}
