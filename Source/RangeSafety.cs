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
        private double? armedMET;
        private double? destructMET;
        private double? abortMET;
        private double? destroyMET;
        private bool coastingToAp;

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
            flightCorridor.SetRangeStateDescription();
        }

        private void CheckIfRunwayOrLaunchPad()
        {
            var vessel = FlightGlobals.ActiveVessel;

            var biome = FlightGlobals.currentMainBody.BiomeMap.GetAtt(Utils.DegreeToRadian(vessel.latitude), Utils.DegreeToRadian(vessel.longitude)).name;
            if (biome == "Runway")
            {
                flightCorridor.State = RangeState.Exempt;
            }
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
            if (!configWindow.settings.enableRangeSafety || flightCorridor.State == RangeState.Exempt)
            {
                return;
            }

            var flightState = GetFlightState();

            if (flightState == null)
            {
                flightCorridor.SetRangeStateDescription();
                return;
            }

            flightCorridor.CheckStatus(flightState);
            flightCorridor.SetRangeStateDescription();

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
                currentRangeState = flightCorridor.State;
            }
            else if (currentRangeState == RangeState.Armed)
            {
                if (coastingToAp)
                {
                    if (FlightGlobals.ActiveVessel.orbit.timeToAp > FlightGlobals.ActiveVessel.orbit.timeToPe)
                    {
                        coastingToAp = false;
                        PerformPostCoastArmActions();
                    }
                }
                else
                {
                    if (abortMET.HasValue && abortMET.Value <= FlightGlobals.ActiveVessel.missionTime)
                    {
                        abortMET = null;
                        ExecuteAbortAction();
                    }

                    if (destroyMET.HasValue && destroyMET.Value <= FlightGlobals.ActiveVessel.missionTime)
                    {
                        destroyMET = null;
                        flightCorridor.State = RangeState.Destruct;
                        currentRangeState = RangeState.Destruct;
                        PerformDestructActions();
                    }
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
            if (!armedMET.HasValue)
            {
                armedMET = FlightGlobals.ActiveVessel.missionTime;
                FlightLogger.eventLog.Add(string.Format("[{0}]: Range safety entered ARM state: {1}", KSPUtil.PrintTimeCompact((int)Math.Floor(armedMET.Value), false), flightCorridor.StatusDescription));

                if (configWindow.settings.terminatThrustOnArm)
                {
                    ExecuteDisableThrustAction();
                }
                if (configWindow.settings.coastToApogeeBeforeAbort)
                {
                    if (FlightGlobals.ActiveVessel.orbit.timeToAp < FlightGlobals.ActiveVessel.orbit.timeToPe)
                    {
                        coastingToAp = true;
                    }
                }
                PerformPostCoastArmActions();
            }
        }

        private void PerformPostCoastArmActions()
        {
            if (!coastingToAp && configWindow.settings.abortOnArm)
            {
                abortMET = FlightGlobals.ActiveVessel.missionTime + 0.5;
            }
            if (!coastingToAp && configWindow.settings.delay3secAfterAbort)
            {
                destroyMET = FlightGlobals.ActiveVessel.missionTime + 3.5;
            }
        }

        private void PerformDestructActions()
        {
            if (!destructMET.HasValue)
            {
                destructMET = FlightGlobals.ActiveVessel.missionTime;
                if (configWindow.settings.destroyOnDestruct)
                {
                    ExecuteDestroyAction();
                }
            }
        }

        private void ExecuteDisableThrustAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightGlobals.ActiveVessel.ctrlState.mainThrottle = 0;
                FlightInputHandler.state.mainThrottle = 0; //so that the on-screen throttle gauge reflects the autopilot throttle
            }
        }

        private void ExecuteCoastToApAction()
        {
            coastingToAp = true;
        }

        private void ExecuteAbortAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightLogger.eventLog.Add(string.Format("[{0}]: ABORT triggered by range safety.", KSPUtil.PrintTimeCompact((int)Math.Floor(FlightGlobals.ActiveVessel.missionTime), false)));
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
            }
        }

        private void ExecuteDestroyAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightLogger.eventLog.Add(string.Format("[{0}]: Craft destroyed by range safety.", KSPUtil.PrintTimeCompact((int)Math.Floor(FlightGlobals.ActiveVessel.missionTime), false)));
                FlightGlobals.ActiveVessel.Die();
            }
        }
    }
}
