using System;

namespace RangeSafety
{
    public class Coordinates
    {
        public double latitude;
        public double longitude;

        public Coordinates(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public static string ToStringDecimal(double latitude, double longitude, bool newline = false, int precision = 3)
        {
            double clampedLongitude = Utils.ClampDegrees180(longitude);
            double latitudeAbs = Math.Abs(latitude);
            double longitudeAbs = Math.Abs(clampedLongitude);
            return latitudeAbs.ToString("F" + precision) + "° " + (latitude > 0 ? "N" : "S") + (newline ? "\n" : ", ")
                + longitudeAbs.ToString("F" + precision) + "° " + (clampedLongitude > 0 ? "E" : "W");
        }

        public string ToStringDecimal(bool newline = false, int precision = 3)
        {
            return ToStringDecimal(latitude, longitude, newline, precision);
        }

        public static string ToStringDMS(double latitude, double longitude, bool newline = false)
        {
            double clampedLongitude = Utils.ClampDegrees180(longitude);
            return AngleToDMS(latitude) + (latitude > 0 ? " N" : " S") + (newline ? "\n" : ", ")
                 + AngleToDMS(clampedLongitude) + (clampedLongitude > 0 ? " E" : " W");
        }

        public string ToStringDMS(bool newline = false)
        {
            return ToStringDMS(latitude, longitude, newline);
        }

        public static string AngleToDMS(double angle)
        {
            int degrees = (int)Math.Floor(Math.Abs(angle));
            int minutes = (int)Math.Floor(60 * (Math.Abs(angle) - degrees));
            int seconds = (int)Math.Floor(3600 * (Math.Abs(angle) - degrees - minutes / 60.0));

            return String.Format("{0:0}° {1:00}' {2:00}\"", degrees, minutes, seconds);
        }
    }
}
