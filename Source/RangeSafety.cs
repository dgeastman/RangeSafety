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

        internal FlightSceneWindow flightSceneWindow = null;
        internal FlightRange flightRange = null;
        internal IFlightCorridor flightCorridor = null;
        internal Settings settings = null;

        private RangeState currentRangeState;
        private RangeActions currentAction;

        protected void Awake()
        {
            try
            {
                flightSceneWindow = new FlightSceneWindow(this);
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
            try
            {
                settings = Settings.InstantiateFromConfig();
                flightCorridor = FlightCorridorBase.InstantiateFromConfig();
                flightRange = new FlightRange();
                flightRange.Initialize(this);

                currentAction = RangeActions.WaitForLaunch;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
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
                settings.SaveToFile();
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
                flightSceneWindow.OnGUI();
        }

        protected void FixedUpdate()
        {
            if (settings == null || flightCorridor == null || flightRange == null)
            {
                return;
            }

            try
            {
                flightRange.CheckState(flightCorridor);
                flightRange.ProcessActions();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

    }
}
