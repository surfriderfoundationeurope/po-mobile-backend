using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.ViewModel
{
    public class ImageAnnotationBoundingBoxResult
    {
        public Guid Id { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TrashId { get; set; }
        public Guid ImageId { get; set; }
        public int Location_x { get; set; }
        public int Location_y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ImageAnnotationBoundingBoxResult()
        {

        }
        public ImageAnnotationBoundingBoxResult(ImageAnnotationBoundingBox imageAnnotationBoundingBox)
        {
            Id = imageAnnotationBoundingBox.Id;
            CreatorId = imageAnnotationBoundingBox.id_creator_fk;
            CreatedOn = imageAnnotationBoundingBox.CreatedOn;
            TrashId = imageAnnotationBoundingBox.id_ref_trash_type_fk;
            ImageId = imageAnnotationBoundingBox.id_ref_images_for_labelling;
            Location_x = imageAnnotationBoundingBox.Location_x;
            Location_y = imageAnnotationBoundingBox.Location_y;
            Width = imageAnnotationBoundingBox.Width;
            Height = imageAnnotationBoundingBox.Height;
        }
    }
}
