using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherMap
{
    internal class Conversions
    {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, bool toLocal)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (toLocal)
            {
                dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            }
            else
            {
                dateTime = dateTime.AddSeconds(unixTimeStamp);
            }
            return dateTime;
        }

        public static double MetersToMiles(ushort meters)
        {
            return (meters / 1000) * 0.6213711922;
        }

        public static double mmToInch(float mm)
        {
            return (mm * 0.393701) / (10);
        }

        public static string WindDegToDir(float windDeg)
        {
            int deg = (int)Math.Round(windDeg, 0);

            if ((deg >= 349 && deg <= 360) || (deg >= 0 && deg <= 11))
            {
                return "N";
            }
            else if (deg >= 12 && deg <= 34)
            {
                return "NNE";
            }
            else if (deg >= 35 && deg <= 56)
            {
                return "NE";
            }
            else if (deg >= 57 && deg <= 79)
            {
                return "ENE";
            }
            else if (deg >= 80 && deg <= 101)
            {
                return "E";
            }
            else if (deg >= 102 && deg <= 124)
            {
                return "ESE";
            }
            else if (deg >= 125 && deg <= 146)
            {
                return "SE";
            }
            else if (deg >= 147 && deg <= 169)
            {
                return "SSE";
            }
            else if (deg >= 170 && deg <= 191)
            {
                return "S";
            }
            else if (deg >= 192 && deg <= 214)
            {
                return "SSW";
            }
            else if (deg >= 215 && deg <= 236)
            {
                return "SW";
            }
            else if (deg >= 237 && deg <= 259)
            {
                return "WSW";
            }
            else if (deg >= 260 && deg <= 281)
            {
                return "W";
            }
            else if (deg >= 282 && deg <= 304)
            {
                return "WNW";
            }
            else if (deg >= 305 && deg <= 326)
            {
                return "NW";
            }
            // else if (deg >= 327 && deg <= 348) -- any remaining values will be in this range
            else
            {
                return "NNW";
            }
        }
    }
}
