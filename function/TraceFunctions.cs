using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public static class TraceFunctions
    {
        [FunctionName("UploadTracefile")]
        public static async Task<IActionResult> RunUploadTraceAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/file")] HttpRequest req,
            [Blob("trace-attachments", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation("Trace Attachment file");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();
            
            string name = $"{Guid.NewGuid():N}/test.txt";

            var traceAttachmentBlob = blobContainer.GetBlockBlobReference(name);

            traceAttachmentBlob.Properties.ContentType = req.ContentType ?? "application/json";
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            await traceAttachmentBlob.UploadFromStreamAsync(req.Body);
            
            return (ActionResult)new OkObjectResult($"File uploaded!");
        }

        [FunctionName("UploadTrace")]
        public static async Task<IActionResult> RunUploadTrace(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace")] HttpRequest req,
            [Blob("trace", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation("Trace file");
            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();

            string name = $"{DateTime.UtcNow.Year}/{DateTime.Now.Month}/{DateTime.UtcNow.Day}/{Guid.NewGuid():N}";

            var traceAttachmentBlob = blobContainer.GetBlockBlobReference(name);

            traceAttachmentBlob.Properties.ContentType = "application/json";
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            await traceAttachmentBlob.UploadFromStreamAsync(req.Body);

            return (ActionResult)new OkObjectResult($"File uploaded!");
        }
    }
}
