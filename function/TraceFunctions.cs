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
        private IMediaStore _mediaStore;

        public TraceFunctions(TraceService traceService, IMediaStore mediaStore)
        {
            _traceService = traceService;
            _mediaStore = mediaStore;
        }

        [FunctionName("UploadTraceAttachment")]
        public async Task<IActionResult> RunUploadTraceAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/{traceId}/attachments/{fileName}")] HttpRequest req,
            [Blob("manual", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerManual,
            [Blob("mobile", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerMobile,
            [Blob("gopro", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerGoPro,
            [AccessToken] AccessTokenResult accessTokenResult,
            string traceId,
            string fileName,
            ILogger log,
            [Blob("manual/{traceId}.json", FileAccess.Read)] Stream traceManualIdJsonFile = null,
            [Blob("mobile/{traceId}.json", FileAccess.Read)] Stream traceMobileIdJsonFile = null,
            [Blob("gopro/{traceId}.json", FileAccess.Read)] Stream traceGoProIdJsonFile = null
        )
        {
            log.LogInformation("Trace Attachment file");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();
            Stream jsonStream;
            if (traceManualIdJsonFile != null)
            {
                jsonStream = traceManualIdJsonFile;
            }
            else
            {
                if (traceMobileIdJsonFile != null)
                    jsonStream = traceMobileIdJsonFile;
                else
                {
                    if (traceGoProIdJsonFile != null)
                        jsonStream = traceGoProIdJsonFile;
                    else
                        return new UnauthorizedResult();
                }

            }
            var trace = await new StreamReader(jsonStream).ReadToEndAsync();
            TraceViewModel traceVm = JsonSerializer.Deserialize<TraceViewModel>(trace);


            //ecrire dans le bon blob

            string name = $"{traceId}{Path.GetExtension(fileName)}";
            Guid mediaId = Guid.NewGuid();

            CloudBlockBlob traceAttachmentBlob = blobContainerManual.GetBlockBlobReference(name);
            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                traceAttachmentBlob = blobContainerMobile.GetBlockBlobReference(name);
            }
            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                traceAttachmentBlob = blobContainerGoPro.GetBlockBlobReference(name);
            }
            traceAttachmentBlob.Properties.ContentType = req.ContentType;
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            traceAttachmentBlob.Metadata.Add("mediaId", mediaId.ToString());
            await traceAttachmentBlob.UploadFromStreamAsync(req.Body);

            // Add to Media
            try
            {
                await _mediaStore.AddMedia(mediaId, name, accessTokenResult.User.Id, traceId, DateTime.UtcNow, traceAttachmentBlob.Uri);
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to store media to DB");
            }

            return new StatusCodeResult(200);
        }

        [FunctionName("UploadTrace")]
        public async Task<IActionResult> RunUploadTrace(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace")] HttpRequest req,
            [Blob("manual", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerManual,
            [Blob("mobile", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerMobile,
            [Blob("gopro", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerGoPro,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {

            log.LogInformation("Trace file");
            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            TraceViewModel traceVm = JsonSerializer.Deserialize<TraceViewModel>(body);

            /// Backup trace to storage account
            string name = $"{traceVm.id}.json";

            CloudBlockBlob traceAttachmentBlob = blobContainerManual.GetBlockBlobReference(name);
            SharedAccessBlobPolicy sharedAccessPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create |
                              SharedAccessBlobPermissions.Write,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(20)
            };
            string sas = blobContainerManual.GetSharedAccessSignature(sharedAccessPolicy);

            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                traceAttachmentBlob = blobContainerMobile.GetBlockBlobReference(name);
                sas = blobContainerMobile.GetSharedAccessSignature(sharedAccessPolicy);
            }
            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                traceAttachmentBlob = blobContainerGoPro.GetBlockBlobReference(name);
                sas = blobContainerMobile.GetSharedAccessSignature(sharedAccessPolicy);
            }

            traceAttachmentBlob.Properties.ContentType = "application/json";
            traceAttachmentBlob.Metadata.Add("uid", accessTokenResult.User.Id);
            await traceAttachmentBlob.UploadTextAsync(body);

            // Insert it into PGSQL
            try
            {
                await _traceService.AddTrace(accessTokenResult.User.Id, traceVm);
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to store media to DB");
            }


            string attachmentPath = $"{accessTokenResult.User.Id}/{traceVm.id}/{{fileName}}";


            return new OkObjectResult(new
            {
                traceId = traceVm.id,
                uploadUri = $"{traceAttachmentBlob.Uri.AbsoluteUri}{sas}"
            });
        }
    }





}
