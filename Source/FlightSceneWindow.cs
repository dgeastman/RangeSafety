using UnityEngine;

namespace RangeSafety
{
    internal class FlightSceneWindow
    {
        private enum tabs { FlightCorridor, ArmActions, Settings };

        private Rect windowPos;
        private tabs currentTab;
        private GUIStyle pressedButton;
        private RangeSafety rangeSafetyInstance;

        public FlightSceneWindow(RangeSafety instance)
        {
            rangeSafetyInstance = instance;
            windowPos = new Rect(0, 0, 0, 0);
            pressedButton = new GUIStyle(HighLogic.Skin.button);
            pressedButton.normal = pressedButton.active;
            currentTab = tabs.FlightCorridor;
        }

        public void OnGUI()
        {
            if (rangeSafetyInstance.settings != null)
            {
                windowPos.x = rangeSafetyInstance.settings.windowX;
                windowPos.y = rangeSafetyInstance.settings.windowY;
            }
            windowPos = GUILayout.Window("RangeSafety.FlightSceneWindow".GetHashCode(), windowPos, DrawWindow, "Range Safety");
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
                    if (GUILayout.Button("Settings", currentTab == tabs.Settings ? pressedButton : HighLogic.Skin.button)) currentTab = tabs.Settings;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUIUtils.SimpleLabel("State", FlightRange.GetRangeStateText(rangeSafetyInstance.flightRange.State, rangeSafetyInstance.flightCorridor.Status));
                    GUILayout.EndHorizontal();

                    switch (currentTab)
                    {
                        case tabs.FlightCorridor:
                            FlightCorridorTab();
                            break;
                        case tabs.ArmActions:
                            ArmActionsTab();
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
                rangeSafetyInstance.settings.windowX = windowPos.x;
                rangeSafetyInstance.settings.windowY = windowPos.y;
            }
        }


        private void FlightCorridorTab()
        {
            if (rangeSafetyInstance.flightCorridor != null)
            {
                rangeSafetyInstance.flightCorridor.DrawEditor();
            }
        }

        private void ArmActionsTab()
        {
            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.terminateThrustOnArm = GUILayout.Toggle(rangeSafetyInstance.settings.terminateThrustOnArm, "Terminate Thrust");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.destroySolids = GUILayout.Toggle(rangeSafetyInstance.settings.destroySolids, "Destroy Solids");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.abortOnArm = GUILayout.Toggle(rangeSafetyInstance.settings.coastToApogeeBeforeAbort, "Coast to Apogee");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.abortOnArm = GUILayout.Toggle(rangeSafetyInstance.settings.abortOnArm, "Abort");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.delay3secAfterAbort = GUILayout.Toggle(rangeSafetyInstance.settings.delay3secAfterAbort, "Delay 3 seconds");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.destroyLaunchVehicle = GUILayout.Toggle(rangeSafetyInstance.settings.destroyLaunchVehicle, "Destroy Launch Vehicle");
            GUILayout.EndHorizontal();
        }

        private void SettingsTab()
        {
            GUILayout.BeginHorizontal();
            rangeSafetyInstance.settings.enableRangeSafety = GUILayout.Toggle(rangeSafetyInstance.settings.enableRangeSafety, "Enable Range Safety");
            GUILayout.EndHorizontal();
        }

    }
}
