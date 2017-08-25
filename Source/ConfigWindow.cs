using UnityEngine;

namespace RangeSafety
{
    public enum tabs { FlightCorridor, ArmActions, DestructOptions };

    class ConfigWindow
    {
        static Rect windowPos = new Rect(500, 240, 0, 0);
        private tabs currentTab;
        private GUIStyle pressedButton;

        public IFlightCorridor FlightCorridor { get; set; }

        public ConfigWindow()
        {
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
                    GUILayout.Button("Flight Corridor", currentTab == tabs.FlightCorridor ? pressedButton : HighLogic.Skin.button);
                    GUILayout.Button("ARM Actions", currentTab == tabs.ArmActions ? pressedButton : HighLogic.Skin.button);
                    GUILayout.Button("DESTRUCT Actions", currentTab == tabs.DestructOptions ? pressedButton : HighLogic.Skin.button);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("State", HighLogic.Skin.label);
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
            GUILayout.Toggle(true, "Terminate Thrust");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "Abort");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "Delay 3000ms");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "DESTRUCT");
            GUILayout.EndHorizontal();
        }

        private void DestructActionsTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Toggle(true, "Destroy Launch Vehicle");
            GUILayout.EndHorizontal();
        }
    }
}
