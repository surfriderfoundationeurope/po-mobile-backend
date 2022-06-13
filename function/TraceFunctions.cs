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
using System.Collections.Generic;
using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class TraceFunctions
    {
        private const string OpenApiTraceCategory = "Trace";

        private IImageService _imageService;
        private IMediaStore _mediaStore;
        private TraceService _traceService;

        public TraceFunctions(TraceService traceService, IMediaStore mediaStore, IImageService imageService)
        {
            _imageService = imageService;
            _traceService = traceService;
            _mediaStore = mediaStore;
        }

        [FunctionName(nameof(GetTraceAttachmentUploadUrl))]
        [OpenApiOperation(operationId: nameof(GetTraceAttachmentUploadUrl), tags: new[] { OpenApiTraceCategory }, Description = "Get an upload URL for an attachment. Used for large files.")]
        [OpenApiParameter("traceId", In = ParameterLocation.Path, Description = "The Trace Id associated with this attachment")]
        [OpenApiParameter("fileName", In = ParameterLocation.Path, Description = "The name of the file uploaded. Used to determine attachment type (jpeg, png, mp4, ...) ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TraceAttachmentUploadDetailsViewModel), Description = "An object containing the upload URL details.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The trace ID is incorrect.")]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        public async Task<IActionResult> GetTraceAttachmentUploadUrl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/{traceId}/attachments/uploadurl/{fileName}")] HttpRequest req,
            [Blob("images2label", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerImageToLabel,
            [Blob("mobile", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerMobile,
            [Blob("gopro", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerGoPro,
            [AccessToken] AccessTokenResult accessTokenResult,
            string traceId,
            string fileName,
            ILogger log,
            [Blob("manual/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceManualIdJsonFile = null,
            [Blob("mobile/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceMobileIdJsonFile = null,
            [Blob("gopro/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceGoProIdJsonFile = null
            )
        {
            if (accessTokenResult.Status != AccessTokenStatus.Valid)
            {
                log.LogWarning($"Access token invalid: {accessTokenResult.Status}");
                return new UnauthorizedResult();
            }
              

            var traceAttachmentDetails = new TraceAttachmentUploadDetailsViewModel() { TraceId = traceId };

            // Load the trace
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
                        return new NotFoundResult();
                }
            }
            var trace = await new StreamReader(jsonStream).ReadToEndAsync();
            TraceViewModel traceVm = JsonSerializer.Deserialize<TraceViewModel>(trace);

            // Generate media
            string name = string.Empty;
            Guid mediaId = Guid.NewGuid();
            BlobClient traceAttachmentBlob = null;
            string extension = Path.GetExtension(fileName);
            if (traceVm.trackingMode.ToLower() == "manual")
            {
                name = await GetNextFileName(blobContainerImageToLabel, traceId, extension);
                traceAttachmentBlob = blobContainerImageToLabel.GetBlobClient(name);
            }
            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                name = await GetNextFileName(blobContainerMobile, traceId, extension);
                traceAttachmentBlob = blobContainerMobile.GetBlobClient(name);
            }
            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                name = await GetNextFileName(blobContainerGoPro, traceId, extension);
                traceAttachmentBlob = blobContainerGoPro.GetBlobClient(name);
            }

            SharedAccessBlobPolicy sharedAccessPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create |
                              SharedAccessBlobPermissions.Write,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(20)
            };
            string sas = traceAttachmentBlob.GenerateSasUri(
                BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Write | BlobSasPermissions.Tag,
                DateTimeOffset.Now.AddMinutes(20)).AbsoluteUri;

            traceAttachmentDetails.UploadUrl = sas;
            traceAttachmentDetails.HttpHeaders.Add("x-ms-blob-type", "BlockBlob");
            traceAttachmentDetails.HttpHeaders.Add("x-ms-version", "2021-04-10");
            traceAttachmentDetails.HttpHeaders.Add("x-ms-tags", $"uid={accessTokenResult.User.Id}&mediaId={mediaId.ToString()}");

            // TODO: Add them to Media table
            return new OkObjectResult(traceAttachmentDetails);
        }



        [FunctionName(nameof(UploadTraceAttachment))]
        [OpenApiOperation(operationId: nameof(UploadTraceAttachment), tags: new[] { OpenApiTraceCategory })]
        [OpenApiRequestBody(contentType: "image/jpeg", typeof(object))]
        [OpenApiParameter("traceId",In = ParameterLocation.Path, Description = "The Trace Id associated with this attachment")]
        [OpenApiParameter("fileName", In = ParameterLocation.Path, Description = "The name of the file uploaded. Used to determine attachment type (jpeg, png, mp4, ...) ")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The file has been successfully uploaded.")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The trace ID is incorrect.")]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        public async Task<IActionResult> UploadTraceAttachment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace/{traceId}/attachments/{fileName}")] HttpRequest req,
            [Blob("images2label", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerImageToLabel,
            [Blob("mobile", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerMobile,
            [Blob("gopro", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerGoPro,
            [AccessToken] AccessTokenResult accessTokenResult,
            string traceId,
            string fileName,
            ILogger log,
            [Blob("manual/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceManualIdJsonFile = null,
            [Blob("mobile/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceMobileIdJsonFile = null,
            [Blob("gopro/{traceId}.json", FileAccess.Read, Connection = "TraceStorage")] Stream traceGoProIdJsonFile = null
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
                        return new NotFoundResult();
                }
            }
            var trace = await new StreamReader(jsonStream).ReadToEndAsync();
            TraceViewModel traceVm = JsonSerializer.Deserialize<TraceViewModel>(trace);

            //ecrire dans le bon blob
            string name = string.Empty;
            Guid mediaId = Guid.NewGuid();
            BlobClient traceAttachmentBlob = null;
            string extension = Path.GetExtension(fileName);
            if (traceVm.trackingMode.ToLower() == "manual")
            {
                name = await GetNextFileName(blobContainerImageToLabel, traceId, extension);
                traceAttachmentBlob = blobContainerImageToLabel.GetBlobClient(name);
            }
            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                name = await GetNextFileName(blobContainerMobile, traceId, extension);
                traceAttachmentBlob = blobContainerMobile.GetBlobClient(name);
            }
            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                name = await GetNextFileName(blobContainerGoPro, traceId, extension);
                traceAttachmentBlob = blobContainerGoPro.GetBlobClient(name);
            }

            Dictionary<string, string> blobMetadata = new Dictionary<string, string>();
            blobMetadata.Add("uid", accessTokenResult.User.Id);
            blobMetadata.Add("mediaId", mediaId.ToString());
            
            await traceAttachmentBlob.UploadAsync(req.Body);
            await traceAttachmentBlob.SetHttpHeadersAsync(new BlobHttpHeaders() {ContentType = req.ContentType});
            await traceAttachmentBlob.SetMetadataAsync(blobMetadata);


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

        [FunctionName(nameof(UploadTrace))]
        [OpenApiOperation(operationId: nameof(UploadTrace), tags: new[] { OpenApiTraceCategory })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TraceViewModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TraceUploadResultViewModel), Description = "An object representing the uploaded trace.")]
        [OpenApiSecurity("bearer_auth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
        public async Task<IActionResult> UploadTrace(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trace")] HttpRequest req,
            [Blob("manual", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerManual,
            [Blob("mobile", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerMobile,
            [Blob("gopro", FileAccess.Write, Connection = "TraceStorage")] BlobContainerClient blobContainerGoPro,
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

            BlobClient traceAttachmentBlob = blobContainerManual.GetBlobClient(name);
            SharedAccessBlobPolicy sharedAccessPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create |
                              SharedAccessBlobPermissions.Write,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(20)
            };
            string sas = traceAttachmentBlob.GenerateSasUri(
                BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Write,
                DateTimeOffset.Now.AddMinutes(20)).AbsoluteUri;

            if (traceVm.trackingMode.ToLower() == "automatic")
            {
                traceAttachmentBlob = blobContainerMobile.GetBlobClient(name);
                sas = traceAttachmentBlob.GenerateSasUri(
                    BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Write,
                    DateTimeOffset.Now.AddMinutes(20)).AbsoluteUri;
            }

            if (traceVm.trackingMode.ToLower() == "gopro")
            {
                traceAttachmentBlob = blobContainerGoPro.GetBlobClient(name);
                sas = traceAttachmentBlob.GenerateSasUri(
                    BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Write,
                    DateTimeOffset.Now.AddMinutes(20)).AbsoluteUri;
            }

            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));

            await traceAttachmentBlob.UploadAsync(bodyStream);
            await traceAttachmentBlob.SetHttpHeadersAsync(new BlobHttpHeaders() { ContentType = "application/json" });
            await traceAttachmentBlob.SetMetadataAsync(new Dictionary<string, string> { { "uid", accessTokenResult.User.Id } });

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

            return new OkObjectResult(new TraceUploadResultViewModel()
            {
                traceId = traceVm.id,
                uploadUri = $"{traceAttachmentBlob.Uri.AbsoluteUri}{sas}"
            });
        }

        private async Task<string> GetNextFileName(BlobContainerClient blobContainer, string name, string extension)
        {
            string tempName = $"{name}";
            if (extension != ".mp4")
            {
                List<string> list = new List<string>();



                IAsyncEnumerable<BlobItem> results = blobContainer.GetBlobsAsync(prefix: name);
                await foreach (BlobItem item in results)
                {
                    list.Add(Path.GetFileNameWithoutExtension(item.Name));
                }

                int i = 1;

                while (list.Exists(fileName => tempName == fileName))
                {
                    tempName = $"{name}({i})";
                    i++;
                }
            }

            return tempName + extension;
        }
    }
}
