using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSP.IO;
using File = KSP.IO.File;
using UnityEngine;

namespace RangeSafety
{
    public class ConfigLoader
    {
        ConfigNode global = new ConfigNode("RangeSafetyConfig");
        public void Load()
        {
            if (File.Exists<RangeSafety>("flightcorridors.cfg"))
            {
                try
                {
                    global = ConfigNode.Load(IOUtils.GetFilePathFor(this.GetType(), "flightcorridors.cfg"));                   
                }
                catch (Exception e)
                {
                    Debug.LogError("ConfigLoader.Load caught an exception trying to load flightcorridors.cfg: " + e);
                }
            }
        }

        public ConfigNode GetRangeConfig()
        {
            if (FlightGlobals.ActiveVessel == null) return null;
            
            ConfigNode result = null;
            ConfigNode corridorsNode = null;
            if (global.TryGetNode("FlightCorridors", ref corridorsNode))
            {
                for (int i = 0; i < corridorsNode.nodes.Count; i++)
                {
                    ConfigNode testNode = corridorsNode.nodes[i];
                    double lat =0, lon=0;
                    if (!testNode.TryGetValue("latitude", ref lat))
                    {
                        break;
                    }
                    if (!testNode.TryGetValue("longitude", ref lon))
                    {
                        break;
                    }
                    if (Utils.DistanceBetween(FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude, lat, lon) <= 1500)
                    {
                        result = testNode;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
