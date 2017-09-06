using System;

namespace RangeSafety
{
    internal class FlightCorridorInclinations : FlightCorridorBase
    {
        public EditableDouble MinimumAzimuth { get; set; }
        public EditableDouble MaximumAzimuth { get; set; }
        public EditableDouble MinimumVerticalInclination { get; set; }

        public override void DrawEditor()
        {
            base.DrawEditor();

            GUIUtils.SimpleTextBox("Minimum Azimuth", MinimumAzimuth, "°");
            GUIUtils.SimpleTextBox("Maximum Azimuth", MaximumAzimuth, "°");
            GUIUtils.SimpleTextBox("Minimum Climb", MinimumVerticalInclination, "°");
        }

        protected override FlightStatus CheckCorridor(FlightStateData flightState)
        {
            FlightStatus result = base.CheckCorridor(flightState);

            var vesselCoords = new Coordinates(flightState.Lattitude, flightState.Longitude);

            var distance = PadCoordinates.DistanceTo(vesselCoords);
            var bearing = PadCoordinates.BearingTo(vesselCoords);

            if ((MaximumAzimuth.val > MinimumAzimuth.val && bearing > MaximumAzimuth.val || bearing < MinimumAzimuth.val)
             || (MaximumAzimuth.val < MinimumAzimuth.val && bearing < MaximumAzimuth.val && bearing > MinimumAzimuth.val))
            {
                result = FlightStatus.CorridorViolation;
            }
            return result;
        }

        protected override void ParseFromConfig(ConfigNode configNode)
        {
            base.ParseFromConfig(configNode);

            double valueDouble = 0;
            var inclinationNode = configNode.GetNode("Inclination");

            if (inclinationNode.TryGetValue("MinimumAzimuth", ref valueDouble))
            {
                this.MinimumAzimuth = valueDouble;
            }
            if (inclinationNode.TryGetValue("MaximumAzimuth", ref valueDouble))
            {
                this.MaximumAzimuth = valueDouble;
            }
            if (inclinationNode.TryGetValue("MinimumVerticalInclination", ref valueDouble))
            {
                this.MinimumVerticalInclination = valueDouble;
            }
        }
    }
}
