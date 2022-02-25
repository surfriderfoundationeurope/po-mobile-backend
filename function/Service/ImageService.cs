using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;

[assembly: InternalsVisibleTo("Surfrider.PlasticOrigins.Backend.Mobile.Tests")]
namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IImageService
    {
        public Task<ImageLabel> GetOneImageRandom(BlobContainerClient container);
        public Task<IEnumerable<TrashType>> GetTrashTypes();
        public Task<IEnumerable<ImageAnnotationBoundingBox>> GetImageBBox(Guid id);
        public Task<bool> AnnotateImage(ImageAnnotationBoundingBoxResult aBbox);
        public Task<bool> UpdateImageData(ImageLabelViewModel img);
        public Task<bool> InsertImageData(ImageLabel img);
    }

    internal class ImageService : IImageService
    {

        // TODO This is a hack. We should have something like a "FilesystemService"
        private IImageStore _imageStore;
        private string _storageConnection;



        public ImageService(IImageStore imageStore, IConfigurationService configurationService)
        {
            _imageStore = imageStore;
            _storageConnection = configurationService.GetValue("TraceStorage");
        }
        public async Task<IEnumerable<TrashType>> GetTrashTypes()
        {
            
            return await _imageStore.GetTrashTypes();
        }
        public async Task<ImageLabel> GetOneImageRandom(BlobContainerClient container)
        {
            ImageLabel image = await _imageStore.GetARandomImage();

            image.Container_url = ImageFunctions.GetImage(container, image.FileName);

            image.Bbox = await _imageStore.GetBBoxForOneImage(image.Id);
            return image;
        }
        public async Task<IEnumerable<ImageAnnotationBoundingBox>> GetImageBBox(Guid id)
        {
            IEnumerable<ImageAnnotationBoundingBox> bBoxList = await _imageStore.GetBBoxForOneImage(id);
            
            return bBoxList;
        }
        public async Task<bool> AnnotateImage(ImageAnnotationBoundingBoxResult aBbox)
        {
            return await _imageStore.AnnotateImage(aBbox);
        }
        public async Task<bool> UpdateImageData(ImageLabelViewModel img)
        {
            return await _imageStore.UpdateImageData(img);
        }
        public async Task<bool> InsertImageData(ImageLabel img)
        {
            return await _imageStore.InsertImageData(img);
        }

    }
}