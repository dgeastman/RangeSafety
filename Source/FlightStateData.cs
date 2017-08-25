using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeSafety
{
    public class FlightStateData
    {
        public double Lattitude { get; set; }
        public double Longitude { get; set; }
        public float VesselTotalMass { get; set; }
        public float VesselHeightAboveSurface { get; set; }
        public float VesselSurfaceSpeed { get; set; }
    }
}
