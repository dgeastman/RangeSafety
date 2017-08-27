using System;
using UnityEngine;

namespace RangeSafety
{
    public class ConfigLoader
    {
        ConfigNode global = new ConfigNode("RangeSafetyConfig");
        public void Load()
        {
            try
            {
                var path = string.Format("{0}GameData/RangeSafety/FlightCorridors.cfg", KSPUtil.ApplicationRootPath);
                global = ConfigNode.Load(path);                   
            }
            catch (Exception e)
            {
                Debug.LogError("ConfigLoader.Load caught an exception trying to load FlightCorridors.cfg: " + e);
            }
        }

        public ConfigNode GetRangeConfig()
        {
            if (FlightGlobals.ActiveVessel == null) return null;
            
            ConfigNode result = null;
            ConfigNode corridorsNode = null;
            ConfigNode tempNode = null;

            if (global.TryGetNode("RANGESAFETY", ref tempNode))
            {
                if (tempNode.TryGetNode("FlightCorridors", ref corridorsNode))
                {
                    for (int i = 0; i < corridorsNode.nodes.Count; i++)
                    {
                        ConfigNode testNode = corridorsNode.nodes[i];
                        double lat = 0, lon = 0;
                        if (!testNode.TryGetValue("latitude", ref lat))
                        {
                            break;
                        }
                        if (!testNode.TryGetValue("longitude", ref lon))
                        {
                            break;
                        }
                        var vesselCoords = new Coordinates(FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude);
                        var padCoords = new Coordinates(lat, lon);
                        if (vesselCoords.DistanceTo(padCoords) <= 1500)
                        {
                            result = testNode;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
