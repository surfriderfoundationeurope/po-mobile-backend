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
using System.Linq;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class ImageFunctions
    {
        private IImageService _imageService;

        public ImageFunctions(IImageService imageService)
        {
            _imageService = imageService;
        }

        [FunctionName("GetOneImage")]
        public string RunGetOneImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/imgName/{fileName}")] HttpRequest req,
            [Blob("images2label", FileAccess.Read, Connection = "TraceStorage")] BlobContainerClient blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            string fileName,
            ILogger log
        )
        {
            log.LogInformation($"Get Image {fileName}");

            return GetImage(blobContainer, fileName);
        }

        [FunctionName("GetImageBBox")]
        public async Task<IActionResult> RunGetImageBBox(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/bbox/{imageId}")] HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
             string imageId,
            ILogger log
        )
        {
            log.LogInformation($"Get Image Image Annotation Bounding Box List");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return null;
            IEnumerable<ImageAnnotationBoundingBox> result = await _imageService.GetImageBBox(Guid.Parse(imageId));
            return (ActionResult)new OkObjectResult(result.Select(i => new ImageAnnotationBoundingBoxResult(i)));
        }

        [FunctionName("GetImageTrashTypes")]
        public async Task<IActionResult> RunGetImageTrashTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/trashtypes")] HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation($"Get Trash Types");

            IEnumerable<TrashType> result = await _imageService.GetTrashTypes();
            return (ActionResult)new OkObjectResult(result);
        }

        [FunctionName("GetRandomImage")]
        public async Task<IActionResult> RunGetRandomImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images/random")] HttpRequest req,
            [Blob("images2label", FileAccess.Read, Connection = "TraceStorage")] BlobContainerClient blobContainer,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation($"Get Random Image");

            ImageLabel result = await _imageService.GetOneImageRandom(blobContainer);
            return (ActionResult)new OkObjectResult(new ImageLabelViewModel(result));
        }

        [FunctionName("AnnotateImage")]
        public async Task<IActionResult> RunAnnotateImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/annotate")] HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation("Annotate Image");

            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            ImageAnnotationBoundingBoxResult imgBBox = JsonSerializer.Deserialize<ImageAnnotationBoundingBoxResult>(body, options);
            var result = await _imageService.AnnotateImage(imgBBox);
            if (result)
                return new StatusCodeResult(200);
            else
                return new StatusCodeResult(500);
        }
        [FunctionName("UpdateImageData")]
        public async Task<IActionResult> RunUpdateImageData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "images/update")] HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log
        )
        {
            log.LogInformation("Update Image Data");

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true
            };
            ImageLabelViewModel img= JsonSerializer.Deserialize<ImageLabelViewModel>(body, options);
            var result = await _imageService.UpdateImageData(img);
            if (result)
                return new StatusCodeResult(200);
            else
                return new StatusCodeResult(500);
        }

        public static string GetImage(BlobContainerClient blobContainer, string fileName)
        {
            var traceAttachmentBlob = blobContainer.GetBlobClient(fileName);
            var sas = traceAttachmentBlob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddMinutes(45));

            return sas.AbsoluteUri;
        }
    }
}
