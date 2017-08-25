using System.Device.Location;

namespace RangeSafety
{
    public static class Utils
    {
        //keeps angles in the range 0 to 360
        public static double ClampDegrees360(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0) return angle + 360.0;
            else return angle;
        }

        //keeps angles in the range -180 to 180
        public static double ClampDegrees180(double angle)
        {
            angle = ClampDegrees360(angle);
            if (angle > 180) angle -= 360;
            return angle;
        }

        public static double DistanceBetween(double lat1, double lon1, double lat2, double lon2)
        {
            var coord1 = new GeoCoordinate(lat1, lon1);
            var coord2 = new GeoCoordinate(lat2, lon2);
            return coord1.GetDistanceTo(coord2);
        }
    }
}
