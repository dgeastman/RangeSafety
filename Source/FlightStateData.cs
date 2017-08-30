using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeSafety
{
    internal class FlightStateData
    {
        public double Lattitude { get; set; }
        public double Longitude { get; set; }
        public double VesselTotalMass { get; set; }
        public double VesselHeightAboveSurface { get; set; }
        public double VesselSurfaceSpeed { get; set; }

        public FlightStateData(Vessel vessel)
        {
            Lattitude = vessel.latitude;
            Longitude = vessel.longitude;
            VesselTotalMass = vessel.totalMass;
            VesselSurfaceSpeed = vessel.srfSpeed;
            VesselHeightAboveSurface = vessel.heightFromSurface;
        }
    }
}
