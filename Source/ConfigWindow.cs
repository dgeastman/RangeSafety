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
                settings.abortOnArm = bool.Parse(settingsNode.GetValue("abortOnArm"));
                settings.delay3secOnArm = bool.Parse(settingsNode.GetValue("delay3secOnArm"));
                settings.destructOnArm = bool.Parse(settingsNode.GetValue("destructOnArm"));
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
                settings.delay3secOnArm = true;
                settings.destructOnArm = true;
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
                settingsNode.AddValue("abortOnArm", settings.abortOnArm);
                settingsNode.AddValue("delay3secOnArm", settings.delay3secOnArm);
                settingsNode.AddValue("destructOnArm", settings.destructOnArm);
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
            settings.abortOnArm = GUILayout.Toggle(settings.abortOnArm, "Abort");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.delay3secOnArm = GUILayout.Toggle(settings.delay3secOnArm, "Delay 3000ms");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.destructOnArm = GUILayout.Toggle(settings.destructOnArm, "DESTRUCT");
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
                if ((FlightCorridor.Status & FlightStatus.Disarmed) == FlightStatus.Disarmed)
                {
                    description = "";
                }
                else if ((FlightCorridor.Status & FlightStatus.Prelaunch) == FlightStatus.Prelaunch)
                {
                    description = "Awaiting launch";
                }
                else if ((FlightCorridor.Status & FlightStatus.SafeMass) == FlightStatus.SafeMass)
                {
                    description = "Vessel reached safe mass";
                }
                else if ((FlightCorridor.Status & FlightStatus.SafeRange) == FlightStatus.SafeRange)
                {
                    description = "Vessel reached safe range";
                }
                else if ((FlightCorridor.Status & FlightStatus.SafeSpeed) == FlightStatus.SafeSpeed)
                {
                    description = "Vessel reached safe speed";
                }
                else if ((FlightCorridor.Status & FlightStatus.NominalPadExclusion) == FlightStatus.NominalPadExclusion)
                {
                    description = "Over launch facility";
                }
                else if ((FlightCorridor.Status & FlightStatus.NominalInCorridor) == FlightStatus.NominalInCorridor)
                {
                    description = "On course";
                }
                else if ((FlightCorridor.Status & FlightStatus.CorridorViolation) == FlightStatus.CorridorViolation)
                {
                    description = "Departed flight corridor!";
                }
            }
            return string.Format("{0}: {1}", state, description);
        }
    }
}
