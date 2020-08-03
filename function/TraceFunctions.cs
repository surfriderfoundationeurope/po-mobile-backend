using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class TraceFunctions
    {
        private TraceService _traceService;

        public TraceFunctions(TraceService traceService)
        {
            _traceService = traceService;
        }

        [FunctionName("UploadTraceAttachment")]
        public  async Task<IActionResult> RunUploadTraceAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/{traceId}/attachments/{fileName}")] HttpRequest req,
            [Blob("trace-attachments", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            string traceId,
            string fileName,
            ILogger log
        )
        {
            log.LogInformation("Trace Attachment file");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();

            string name = $"{accessTokenResult.User.Id}/{traceId}/{fileName}";

            var traceAttachmentBlob = blobContainer.GetBlockBlobReference(name);

            traceAttachmentBlob.Properties.ContentType = req.ContentType;
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            await traceAttachmentBlob.UploadFromStreamAsync(req.Body);

            return new StatusCodeResult(200);
        }

        [FunctionName("UploadTrace")]
        public async Task<IActionResult> RunUploadTrace(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace")] HttpRequest req,
            [Blob("trace", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {

            log.LogInformation("Trace file");
            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();

            var body = await new StreamReader(req.Body).ReadToEndAsync();


            TraceViewModel traceVm = JsonSerializer.Deserialize<TraceViewModel>(body);

            await _traceService.AddTrace(accessTokenResult.User.Id, traceVm);
            
            string name = $"{DateTime.UtcNow.Year}/{DateTime.Now.Month}/{DateTime.UtcNow.Day}/{traceVm.id}.json";

            var traceAttachmentBlob = blobContainer.GetBlockBlobReference(name);
            traceAttachmentBlob.Properties.ContentType = "application/json";
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            await traceAttachmentBlob.UploadTextAsync(body);

            return new OkObjectResult(new
            {
                traceId = traceVm.id
            });
        }
    }





}
