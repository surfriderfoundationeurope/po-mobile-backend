using System;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class Position
    {
        public Guid Id { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public DateTime Time { get; set; }
        public DateTime Createdon { get; set; }
        public Guid RefCampaign { get; set; }
        public Position()
        {

        }
        public Position(ViewModel.Position position)
        {
            Lat = position.lat;
            Lon = position.lng;
            Time = position.date;

        }
    }
}
