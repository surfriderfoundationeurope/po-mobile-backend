using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class TraceFunctions
    {
        private IImageService _imageService;
        private IMediaStore _mediaStore;
        private TraceService _traceService;

        public TraceFunctions(TraceService traceService, IMediaStore mediaStore, IImageService imageService)
        {
            _imageService = imageService;
            _traceService = traceService;
            _mediaStore = mediaStore;
        }

        [FunctionName("UploadTraceAttachment")]
        public async Task<IActionResult> RunUploadTraceAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/{traceId}/attachments/{fileName}")] HttpRequest req,
            [Blob("images2label", FileAccess.Write, Connection = "TraceStorage")] CloudBlobContainer blobContainerImageToLabel,
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
            string name = string.Empty;
            Guid mediaId = Guid.NewGuid();
            CloudBlockBlob traceAttachmentBlob = null;
            if (traceVm.trackingMode.ToLower() == "manual")
            {
                name = $"{await GetNextGileName(blobContainerImageToLabel, traceId)}{Path.GetExtension(fileName)}";
                traceAttachmentBlob = blobContainerImageToLabel.GetBlockBlobReference(name);
            }
            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                name = $"{await GetNextGileName(blobContainerMobile, traceId)}{Path.GetExtension(fileName)}";
                traceAttachmentBlob = blobContainerMobile.GetBlockBlobReference(name);
            }
            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                name = $"{await GetNextGileName(blobContainerGoPro, traceId)}{Path.GetExtension(fileName)}";
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
                ImageLabel imageToInsert = new ImageLabel(Guid.NewGuid(), Guid.Parse(accessTokenResult.User.Id), DateTime.Now, name, string.Empty, string.Empty, string.Empty, traceAttachmentBlob.Uri.AbsoluteUri);
                if (traceVm.trackingMode.ToLower() == "manual")
                {
                    await _imageService.InsertImageData(imageToInsert);
                }
                
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
                await _traceService.AddTrajectoryPoints(traceVm);
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to store media to DB");
            }

            return new OkObjectResult(new
            {
                traceId = traceVm.id,
                uploadUri = $"{traceAttachmentBlob.Uri.AbsoluteUri}{sas}"
            });
        }
        private async Task<string> GetNextGileName(CloudBlobContainer blobContainer, string name)
        {
            string tempName = $"{name}";
            List<string> list = new List<string>();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var resultSegment = await blobContainer.ListBlobsSegmentedAsync(
                    prefix: null,
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.None,
                    maxResults: null,
                    currentToken: blobContinuationToken,
                    options: null,
                    operationContext: null
                );

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = resultSegment.ContinuationToken;
                foreach (IListBlobItem item in resultSegment.Results)
                {

                    list.Add(Path.GetFileNameWithoutExtension(item.Uri.LocalPath));

                }
            } while (blobContinuationToken != null);
            int i = 1;

            while (list.Any(fileName => tempName == fileName))
            {
                tempName = $"{name}({i})";
                i++;
            }


            return tempName;
        }

    }
}
