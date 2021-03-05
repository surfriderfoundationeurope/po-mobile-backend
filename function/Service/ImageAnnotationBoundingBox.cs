using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class ImageAnnotationBoundingBox : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid id_creator_fk { get; set; }
        public DateTime CreatedOn { get; set; }
        public int id_ref_trash_type_fk { get; set; }
        public Guid id_ref_images_for_labelling { get; set; }
        public int Location_x { get; set; }
        public int Location_y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ImageAnnotationBoundingBox()
        { }

        public ImageAnnotationBoundingBox(Guid id, Guid creatorId, DateTime createdOn, int trashId, Guid imageId, int location_x, int location_y, int width, int height)
        {
            SetValue(nameof(Id), id);
            SetValue(nameof(id_creator_fk), creatorId);
            SetValue(nameof(CreatedOn), createdOn);
            SetValue(nameof(id_ref_trash_type_fk), trashId);
            SetValue(nameof(id_ref_images_for_labelling), imageId);
            SetValue(nameof(Location_x), location_x);
            SetValue(nameof(Location_y), location_y);
            SetValue(nameof(Width), width);
            SetValue(nameof(Height), height);
        }

        protected override string GetEntityKey()
        {
            return Id.ToString();
        }
    }
}
