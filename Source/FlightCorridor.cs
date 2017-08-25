using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RangeSafety
{
    [Flags]
    public enum FlightStatus : uint
    {
        Nominal             = 0x000,
        SafeMass            = 0x001,
        SafeSpeed           = 0x002,
        SafeRange           = 0x004,
        NominalPadExclusion = 0x008,
        NominalInCorridor   = 0x010,
        CorridorViolation   = 0x020
    }

    public interface IFlightCorridor
    {
        string Name { get; set; }
        Coordinates PadCoordinates { get; set; }
        EditableInt SafeRange { get; set; }
        EditableInt SafeSpeed { get; set; }
        EditableInt SafeMass { get; set; }
        EditableInt PadSafetyRadius { get; set; }

        FlightStatus Status(FlightStateData flightState);

        void DrawEditor();
    }

    public virtual class FlightCorridorBase : IFlightCorridor
    {
        public string Name { get; set; }
        public Coordinates PadCoordinates { get; set; }
        public EditableInt SafeRange { get; set; }
        public EditableInt SafeSpeed { get; set; }
        public EditableInt SafeMass { get; set; }
        public EditableInt PadSafetyRadius { get; set; }

        public static IFlightCorridor InstantiateFromConfig(ConfigNode configNode)
        {
            if (configNode.HasNode("Inclination"))
            {
                return FlightCorridorInclinations.InstantiateFromConfig(configNode);
            }
            else 
            {
                var instance = new FlightCorridorBase();
                instance.ParseFromConfig(configNode);
                return instance;
            }
        }

        public virtual FlightStatus Status(FlightStateData flightState)
        {
            FlightStatus result = FlightStatus.Nominal;

            result |= CheckVehicleMass(flightState);
            result |= CheckVehicleVelocity(flightState);
            return result;
        }

        public virtual void DrawEditor()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Safe Distance (km)", HighLogic.Skin.label);
            GUILayout.TextField("1000");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Safe Altitude (km)", HighLogic.Skin.label);
            GUILayout.TextField("50000");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Safe Velocity (m/sec)", HighLogic.Skin.label);
            GUILayout.TextField("4000");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Safe Mass (tons)", HighLogic.Skin.label);
            GUILayout.TextField("10");

            GUILayout.EndHorizontal();

        }

        protected virtual bool IsInCorridor(FlightStateData flightState)
        {
            return true;
        }

        protected FlightStatus CheckVehicleMass(FlightStateData flightState)
        {
            return flightState.VesselTotalMass <= SafeMass ? FlightStatus.SafeMass : FlightStatus.Nominal;
        }

        protected FlightStatus CheckVehicleVelocity(FlightStateData flightState)
        {
            return flightState.VesselSurfaceSpeed >= SafeSpeed ? FlightStatus.SafeSpeed : FlightStatus.Nominal;
        }

        protected FlightStatus CheckVehicleDistanceFromPad(FlightStateData flightState)
        {
            FlightStatus result = FlightStatus.Nominal;

            var distFromPad = Utils.DistanceBetween(PadCoordinates.latitude, PadCoordinates.longitude, flightState.Lattitude, flightState.Longitude);           
            if (distFromPad <= PadSafetyRadius)
            {
                result |= FlightStatus.NominalPadExclusion;
            } 
            else if (distFromPad >= SafeRange)
            {
                result |= FlightStatus.SafeRange;
            }
            return result;
        }

        protected virtual void ParseFromConfig(ConfigNode configNode)
        {
            string valueString = string.Empty;
            double valueDouble = 0;
            int valueInt = 0;

            if (configNode.TryGetValue("Name", ref valueString))
            {
                this.Name = valueString;
            }
            if (configNode.TryGetValue("latitude", ref valueDouble))
            {
                double valueDouble2 = 0;
                if (configNode.TryGetValue("longitude", ref valueDouble2))
                {
                    this.PadCoordinates = new Coordinates(valueDouble, valueDouble2);
                }
            }
            if (configNode.TryGetValue("SafeRange", ref valueInt))
            {
                this.SafeRange = valueInt;
            }
            if (configNode.TryGetValue("SafeSpeed", ref valueInt))
            {
                this.SafeSpeed = valueInt;
            }
            if (configNode.TryGetValue("SafeMass", ref valueInt))
            {
                this.SafeMass = valueInt;
            }
            if (configNode.TryGetValue("PadSafetyRadius", ref valueInt))
            {
                this.PadSafetyRadius = valueInt;
            }
        }
    }

    public class FlightCorridorInclinations : FlightCorridorBase
    {
        public EditableDouble MinimumInclination { get; set; }
        public EditableDouble MaximumInclination { get; set; }
        public EditableDouble MinimumVerticalInclination { get; set; }

        public static IFlightCorridor InstantiateFromConfig(ConfigNode configNode)
        {
            var instance = new FlightCorridorInclinations();
            instance.ParseFromConfig(configNode);
            return instance;
        }

        public override void DrawEditor()
        {
            base.DrawEditor();

            GUIUtils.SimpleTextBox("Minimum Inclination", MinimumInclination, "°");
            GUIUtils.SimpleTextBox("Maximum Inclination", MaximumInclination, "°");
            GUIUtils.SimpleTextBox("Minimum Climb", MinimumVerticalInclination, "°");
        }

        protected override bool IsInCorridor(FlightStateData flightState)
        {
            bool result = base.IsInCorridor(flightState);
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
