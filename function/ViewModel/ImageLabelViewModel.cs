using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.ViewModel
{
    public class ImageLabelViewModel
    {
        public Guid ImageId { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Filename { get; set; }
        public string View { get; set; }
        public string ImgQuality { get; set; }
        public string Context { get; set; }
        public string Url { get; set; }
        public IEnumerable<ImageAnnotationBoundingBox> bbox { get; set; }

        public ImageLabelViewModel()
        {

        }
        public ImageLabelViewModel(ImageLabel imageLabel)
        {
            ImageId = imageLabel.Id;
            CreatorId = imageLabel.Id_creator_fk;
            CreatedOn = imageLabel.Createdon;
            Filename = imageLabel.FileName;
            View = imageLabel.View;
            ImgQuality = imageLabel.Image_Quality;
            Context = imageLabel.Context;
            Url = imageLabel.Container_url;
            bbox = imageLabel.Bbox;
        }
    }
}
