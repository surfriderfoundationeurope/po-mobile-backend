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

        public static DateTime ToDateTime(long unixDateTicks, DateTimeKind kind)
        {
            if (unixDateTicks <= 0)
                return DateTime.MinValue;

            DateTime unixStartDate = new DateTime(1970, 01, 01, 0, 0, 0, kind);
            return unixStartDate.AddSeconds(unixDateTicks);
        }

        /// <summary>
        /// Converts a .NET DateTime into a unix "Epoch Time" based number of seconds.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static long ToEpochTime(DateTime date)
        {
            DateTime unixStartDate = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            if (date <= unixStartDate)
                return 0;

            TimeSpan diff = date - unixStartDate;
            // Rounded to the nearest 64-bit signed integer. 
            // If value is halfway between two whole numbers, the even number is returned; that is, 4.5 is converted to 4, and 5.5 is converted to 6.
            return Convert.ToInt64(diff.TotalSeconds);
        }
    }
}
