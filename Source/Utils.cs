using System;

namespace RangeSafety
{
    internal static class Utils
    {
        //keeps angles in the range 0 to 360
        internal static double ClampDegrees360(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0) return angle + 360.0;
            else return angle;
        }

        //keeps angles in the range -180 to 180
        internal static double ClampDegrees180(double angle)
        {
            angle = ClampDegrees360(angle);
            if (angle > 180) angle -= 360;
            return angle;
        }

        internal static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        internal static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}
