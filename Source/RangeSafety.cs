using KSP.UI.Screens;
using System;
using UnityEngine;

namespace RangeSafety
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RangeSafety : MonoBehaviour
    {
        private bool guiEnabled = false;
        private ApplicationLauncherButton button;
        private ConfigWindow configWindow = new ConfigWindow();
        private ConfigLoader configLoader = new ConfigLoader();
        private ConfigNode rangeConfig = null;
        private IFlightCorridor flightCorridor = null;

        protected void Awake()
        {
            try
            {
                GameEvents.onGUIApplicationLauncherReady.Add(this.OnGuiAppLauncherReady);
            }
            catch (Exception ex)
            {
                Debug.LogError("RangeSafety failed to register RangeSafety.OnGuiAppLauncherReady");
                Debug.LogException(ex);
            }
        }

        protected void Start()
        {
            configLoader.Load();
            rangeConfig = configLoader.GetRangeConfig();
            flightCorridor = FlightCorridorBase.InstantiateFromConfig(rangeConfig);
            configWindow.FlightCorridor = flightCorridor;
        }

        private void ShowWindow()
        {
            guiEnabled = true;
        }

        private void HideWindow()
        {
            guiEnabled = false;
        }

        private void OnGuiAppLauncherReady()
        {
            try
            {
                button = ApplicationLauncher.Instance.AddModApplication(
                    ShowWindow,
                    HideWindow,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    GameDatabase.Instance.GetTexture("RangeSafety/abortbutton", false));
            }
            catch (Exception ex)
            {
                Debug.LogError("RangeSafety failed to register RangeSafety");
                Debug.LogException(ex);
            }
        }

        public void OnDestroy()
        {
            try
            {
                GameEvents.onGUIApplicationLauncherReady.Remove(this.OnGuiAppLauncherReady);
                if (button != null)
                    ApplicationLauncher.Instance.RemoveModApplication(button);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void OnGUI()
        {
            if (guiEnabled)
                configWindow.OnGUI();
        }

        protected void FixedUpdate()
        {
        }

    }
}
