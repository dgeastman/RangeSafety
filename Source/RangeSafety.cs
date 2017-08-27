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
        private ConfigWindow configWindow = null;
        private ConfigLoader configLoader = null;
        private ConfigNode rangeConfig = null;
        private IFlightCorridor flightCorridor = null;
        private RangeState currentRangeState = RangeState.Disarmed;

        protected void Awake()
        {
            try
            {
                configWindow = new ConfigWindow();
                configLoader = new ConfigLoader();
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

            configWindow.LoadSettings();
            flightCorridor.SystemSettings = configWindow.settings;
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
                configWindow.SaveSettings();
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
            var flightState = GetFlightState();

            if (flightState == null)
            {
                return;
            }

            flightCorridor.CheckStatus(flightState);
            if (currentRangeState != flightCorridor.State)
            {
                if (flightCorridor.State == RangeState.Armed)
                {
                    PerformArmActions();
                }
                else if (flightCorridor.State == RangeState.Destruct)
                {
                    PerformDestructActions();
                }
                else
                {
                    currentRangeState = flightCorridor.State;
                }
            }
        }

        private FlightStateData GetFlightState()
        {
            var vessel = FlightGlobals.ActiveVessel;

            if (vessel == null || vessel.situation == Vessel.Situations.PRELAUNCH)
            {
                flightCorridor.CheckStatus(null);
                return null;
            }

            return new FlightStateData
            {
                Lattitude = vessel.latitude,
                Longitude = vessel.longitude,
                VesselTotalMass = vessel.totalMass,
                VesselSurfaceSpeed = vessel.srfSpeed,
                VesselHeightAboveSurface = vessel.heightFromSurface
            };
        }

        private void PerformArmActions()
        {
        }

        private void PerformDestructActions()
        {
        }
    }
}
