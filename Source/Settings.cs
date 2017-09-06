using System;
using UnityEngine;

namespace RangeSafety
{
    internal class Settings
    {
        internal bool terminateThrustOnArm;
        internal bool destroySolids;
        internal bool coastToApogeeBeforeAbort;
        internal bool abortOnArm;
        internal bool delay3secAfterAbort;
        internal bool destroyLaunchVehicle;
        internal bool enableRangeSafety;
        internal float windowX;
        internal float windowY;

        internal static Settings InstantiateFromConfig()
        {
            Settings result = null;
            try
            {
                var path = string.Format("{0}GameData/RangeSafety/RangeSafety.settings", KSPUtil.ApplicationRootPath);
                var rootNode = ConfigNode.Load(path);
                if (rootNode != null)
                {
                    var settingsNode = rootNode.GetNode("Settings");
                    if (settingsNode != null)
                    {
                        result = new Settings
                        {
                            windowX = float.Parse(settingsNode.GetValue("windowX")),
                            windowY = float.Parse(settingsNode.GetValue("windowY")),
                            enableRangeSafety = bool.Parse(settingsNode.GetValue("enableRangeSafety")),
                            terminateThrustOnArm = bool.Parse(settingsNode.GetValue("terminateThrustOnArm")),
                            destroySolids = bool.Parse(settingsNode.GetValue("destroySolids")),
                            coastToApogeeBeforeAbort = bool.Parse(settingsNode.GetValue("coastToApogeeBeforeAbort")),
                            abortOnArm = bool.Parse(settingsNode.GetValue("abortOnArm")),
                            delay3secAfterAbort = bool.Parse(settingsNode.GetValue("delay3secAfterAbort")),
                            destroyLaunchVehicle = bool.Parse(settingsNode.GetValue("destroyLaunchVehicle"))
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("ConfigWindow.LoadSettings caught an exception trying to load RangeSafety.settings: " + e);
            }
            finally
            {
                if (result == null)
                {
                    result = new Settings
                    {
                        windowX = 500,
                        windowY = 240,
                        enableRangeSafety = true,
                        terminateThrustOnArm = true,
                        destroySolids = true,
                        coastToApogeeBeforeAbort = true,
                        abortOnArm = true,
                        delay3secAfterAbort = true,
                        destroyLaunchVehicle = true
                    };
                }
            }
            return result;
        }

        internal void SaveToFile()
        {
            try
            {
                var path = string.Format("{0}GameData/RangeSafety/RangeSafety.settings", KSPUtil.ApplicationRootPath);
                var root = new ConfigNode("RangeSafety");
                var settingsNode = root.AddNode(new ConfigNode("Settings"));
                settingsNode.AddValue("windowX", windowX);
                settingsNode.AddValue("windowY", windowY);
                settingsNode.AddValue("enableRangeSafety", enableRangeSafety);
                settingsNode.AddValue("terminateThrustOnArm", terminateThrustOnArm);
                settingsNode.AddValue("destroySolids", destroySolids);
                settingsNode.AddValue("coastToApogeeBeforeAbort", coastToApogeeBeforeAbort);
                settingsNode.AddValue("abortOnArm", abortOnArm);
                settingsNode.AddValue("delay3secAfterAbort", delay3secAfterAbort);
                settingsNode.AddValue("destroyLaunchVehicle", destroyLaunchVehicle);

                root.Save(path);
            }
            catch (Exception e)
            {
                Debug.LogError("ConfigWindow.SaveSettings caught an exception trying to save RangeSafety.settings: " + e);
            }
        }

    }
}
