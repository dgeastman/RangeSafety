using System;

namespace RangeSafety
{
    internal class FlightCorridorInclinations : FlightCorridorBase
    {
        public EditableDouble MinimumInclination { get; set; }
        public EditableDouble MaximumInclination { get; set; }
        public EditableDouble MinimumVerticalInclination { get; set; }

        public override void DrawEditor()
        {
            base.DrawEditor();

            GUIUtils.SimpleTextBox("Minimum Inclination", MinimumInclination, "°");
            GUIUtils.SimpleTextBox("Maximum Inclination", MaximumInclination, "°");
            GUIUtils.SimpleTextBox("Minimum Climb", MinimumVerticalInclination, "°");
        }

        protected override FlightStatus CheckCorridor(FlightStateData flightState)
        {
            FlightStatus result = base.CheckCorridor(flightState);

            var vesselCoords = new Coordinates(flightState.Lattitude, flightState.Longitude);

            var bearing = PadCoordinates.BearingTo(vesselCoords);
            if (bearing > MaximumInclination.val || bearing < MinimumInclination.val)
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

            if (inclinationNode.TryGetValue("MinimumInclination", ref valueDouble))
            {
                this.MinimumInclination = valueDouble;
            }
            if (inclinationNode.TryGetValue("MaximumInclination", ref valueDouble))
            {
                this.MaximumInclination = valueDouble;
            }
            if (inclinationNode.TryGetValue("MinimumVerticalInclination", ref valueDouble))
            {
                this.MinimumVerticalInclination = valueDouble;
            }
        }
    }
}
