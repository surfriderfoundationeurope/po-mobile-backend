using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    class Utilities
    {

        public static int DateToEpochDate(DateTime dateTime)
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static DateTime DateFromEpoch(int epochDate)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddSeconds(epochDate);
        }
    }
}
