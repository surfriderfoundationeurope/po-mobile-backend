using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class ImageLabel : BaseEntity
    {

        public Guid Id { get; set; }
        public Guid Id_creator_fk { get; set; }
        public DateTime Createdon { get; set; }
        public string FileName { get; set; }
        public string View { get; set; }
        public string Image_Quality { get; set; }
        public string Context { get; set; }
        public string Container_url { get; set; }
        public IEnumerable<ImageAnnotationBoundingBox> Bbox { get; set; }

        public ImageLabel()
        { }

        public ImageLabel(Guid id, Guid creatorId, DateTime createdOn, string fileName, string view, string imgQuality, string context, string url)
        {
            Id = id;
            Id_creator_fk = creatorId;
            Createdon = createdOn;
            FileName = fileName;
            View = view;
            Image_Quality = imgQuality;
            Context = context;
            Container_url = url;
        }

        protected override string GetEntityKey()
        {
            return Id.ToString();
        }
    }

}
