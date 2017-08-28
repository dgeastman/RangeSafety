using System;
using UnityEngine;

namespace RangeSafety
{
    public enum tabs { FlightCorridor, ArmActions, DestructOptions, Settings };

    class ConfigWindow
    {
        private Rect windowPos;
        private tabs currentTab;
        private GUIStyle pressedButton;

        public IFlightCorridor FlightCorridor;

        public Settings settings = new Settings();

        public ConfigWindow()
        {
            windowPos = new Rect(settings.windowX, settings.windowY, 0, 0);
            pressedButton = new GUIStyle(HighLogic.Skin.button);
            pressedButton.normal = pressedButton.active;
            currentTab = tabs.FlightCorridor;
        }

        public void OnGUI()
        {
            windowPos = GUILayout.Window("RangeSafetyConfig".GetHashCode(), windowPos, DrawWindow, "Range Safety");
        }

        public void DrawWindow(int windowID)
        {
            try
            {
                GUILayout.BeginVertical();
                try {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Flight Corridor", currentTab == tabs.FlightCorridor ? pressedButton : HighLogic.Skin.button)) currentTab = tabs.FlightCorridor;
                    if (GUILayout.Button("ARM Actions", currentTab == tabs.ArmActions ? pressedButton : HighLogic.Skin.button)) currentTab = tabs.ArmActions;
                    if (GUILayout.Button("DESTRUCT Actions", currentTab == tabs.DestructOptions ? pressedButton : HighLogic.Skin.button)) currentTab = tabs.DestructOptions;
                    if (GUILayout.Button("Settings", currentTab == tabs.Settings ? pressedButton : HighLogic.Skin.button)) currentTab = tabs.Settings;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUIUtils.SimpleLabel("State", GetRangeState());
                    GUILayout.EndHorizontal();

                    switch (currentTab)
                    {
                        case tabs.FlightCorridor:
                            FlightCorridorTab();
                            break;
                        case tabs.ArmActions:
                            ArmActionsTab();
                            break;
                        case tabs.DestructOptions:
                            DestructActionsTab();
                            break;
                        case tabs.Settings:
                            SettingsTab();
                            break;
                    }
                }
                finally
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                }
            }
            finally
            {
                GUI.DragWindow();
                settings.windowX = windowPos.x;
                settings.windowY = windowPos.y;
            }
        }

        public void LoadSettings()
        {
            try
            {
                var path = string.Format("{0}GameData/RangeSafety/RangeSafety.settings", KSPUtil.ApplicationRootPath);
                var rootNode = ConfigNode.Load(path);
                var settingsNode = rootNode.GetNode("Settings");
                settings.windowX = float.Parse(settingsNode.GetValue("windowX"));
                settings.windowY = float.Parse(settingsNode.GetValue("windowY"));
                settings.enableRangeSafety = bool.Parse(settingsNode.GetValue("enableRangeSafety"));
                settings.terminatThrustOnArm = bool.Parse(settingsNode.GetValue("terminatThrustOnArm"));
                settings.coastToApogeeBeforeAbort = bool.Parse(settingsNode.GetValue("coastToApogeeBeforeAbort"));
                settings.abortOnArm = bool.Parse(settingsNode.GetValue("abortOnArm"));
                settings.delay3secAfterAbort = bool.Parse(settingsNode.GetValue("delay3secAfterAbort"));
                settings.destructAfterAbort = bool.Parse(settingsNode.GetValue("destructAfterAbort"));
                settings.destroyOnDestruct = bool.Parse(settingsNode.GetValue("destroyOnDestruct"));
            }
            catch (Exception e)
            {
                Debug.LogError("ConfigWindow.LoadSettings caught an exception trying to load RangeSafety.settings: " + e);
                settings.windowX = 500;
                settings.windowY = 240;
                settings.enableRangeSafety = true;
                settings.terminatThrustOnArm = true;
                settings.abortOnArm = true;
                settings.delay3secAfterAbort = true;
                settings.destructAfterAbort = true;
                settings.destroyOnDestruct = true;
            }
            windowPos.x = settings.windowX;
            windowPos.y = settings.windowY;
        }

        public void SaveSettings()
        {
            try
            {
                var path = string.Format("{0}GameData/RangeSafety/RangeSafety.settings", KSPUtil.ApplicationRootPath);
                var root = new ConfigNode("RangeSafety");
                var settingsNode = root.AddNode(new ConfigNode("Settings"));
                settingsNode.AddValue("windowX", settings.windowX);
                settingsNode.AddValue("windowY", settings.windowY);
                settingsNode.AddValue("enableRangeSafety", settings.enableRangeSafety);
                settingsNode.AddValue("terminatThrustOnArm", settings.terminatThrustOnArm);
                settingsNode.AddValue("coastToApogeeBeforeAbort", settings.coastToApogeeBeforeAbort);
                settingsNode.AddValue("abortOnArm", settings.abortOnArm);
                settingsNode.AddValue("delay3secAfterAbort", settings.delay3secAfterAbort);
                settingsNode.AddValue("destructAfterAbort", settings.destructAfterAbort);
                settingsNode.AddValue("destroyOnDestruct", settings.destroyOnDestruct);

                root.Save(path);
            }
            catch (Exception e)
            {
                Debug.LogError("ConfigWindow.SaveSettings caught an exception trying to save RangeSafety.settings: " + e);
            }
        }

        private void FlightCorridorTab()
        {
            if (FlightCorridor != null)
            {
                FlightCorridor.DrawEditor();
            }
        }

        private void ArmActionsTab()
        {
            GUILayout.BeginHorizontal();
            settings.terminatThrustOnArm = GUILayout.Toggle(settings.terminatThrustOnArm, "Terminate Thrust");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.abortOnArm = GUILayout.Toggle(settings.coastToApogeeBeforeAbort, "Coast to Apogee");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.abortOnArm = GUILayout.Toggle(settings.abortOnArm, "Abort");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.delay3secAfterAbort = GUILayout.Toggle(settings.delay3secAfterAbort, "Delay 3 seconds");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.destructAfterAbort = GUILayout.Toggle(settings.destructAfterAbort, "DESTRUCT");
            GUILayout.EndHorizontal();
        }

        private void DestructActionsTab()
        {
            GUILayout.BeginHorizontal();
            settings.destroyOnDestruct = GUILayout.Toggle(settings.destroyOnDestruct, "Destroy Launch Vehicle");
            GUILayout.EndHorizontal();
        }
        private void SettingsTab()
        {
            GUILayout.BeginHorizontal();
            settings.enableRangeSafety = GUILayout.Toggle(settings.enableRangeSafety, "Enable Range Safety");
            GUILayout.EndHorizontal();
        }

        private string GetRangeState()
        {
            string state = "DISARM";
            string description = string.Empty;

            if (FlightCorridor != null)
            {
                switch (FlightCorridor.State)
                {
                    case RangeState.Nominal:
                        state = "NOMINAL";
                        break;
                    case RangeState.Armed:
                        state = "ARM";
                        break;
                    case RangeState.Destruct:
                        state = "DESTRUCT";
                        break;
                }
                description = FlightCorridor.StatusDescription;
            }
            return string.Format("{0}: {1}", state, description);
        }
    }
}
