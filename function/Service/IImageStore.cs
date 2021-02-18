using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

//using StackExchange.Redis;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IImageStore
    {
        Task<ImageLabel> GetARandomImage();
        Task<IEnumerable<ImageAnnotationBoundingBox>> GetBBoxForOneImage(Guid id);
        Task<IEnumerable<TrashType>> GetTrashTypes();
        Task<bool> AnnotateImage(ImageAnnotationBoundingBoxResult aBbox);
        Task<bool> UpdateImageData(ImageLabelViewModel img);
    }
}
