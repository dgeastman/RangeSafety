using System;

namespace RangeSafety
{
    public enum RangeState
    {
        Disarmed = 0,
        Nominal = 1,
        Armed = 2,
        Destruct = 3,
        Exempt =4
    }

    [Flags]
    public enum FlightStatus : uint
    {
        Nominal             = 0x000,
        Prelaunch           = 0x001,
        SafeMass            = 0x002,
        SafeSpeed           = 0x004,
        SafeRange           = 0x008,
        NominalPadExclusion = 0x010,
        NominalInCorridor   = 0x020,
        CorridorViolation   = 0x040,
        Disarmed            = 0x100
    }

    public interface IFlightCorridor
    {
        string Name { get; set; }
        Coordinates PadCoordinates { get; set; }
        EditableInt SafeRange { get; set; }
        EditableInt SafeSpeed { get; set; }
        EditableInt SafeMass { get; set; }
        EditableInt PadSafetyRadius { get; set; }
        FlightStatus Status { get; }
        String StatusDescription { get; }
        RangeState State { get; set; }
        Settings SystemSettings { get; set; }
        void CheckStatus(FlightStateData flightState);
        void SetRangeStateDescription();
        void DrawEditor();
    }

    public class FlightCorridorBase : IFlightCorridor
    {
        public string Name { get; set; }
        public Coordinates PadCoordinates { get; set; }
        public EditableInt SafeRange { get; set; }
        public EditableInt SafeSpeed { get; set; }
        public EditableInt SafeMass { get; set; }
        public EditableInt PadSafetyRadius { get; set; }
        public FlightStatus Status { get; protected set; }
        public String StatusDescription { get; protected set; }
        public RangeState State { get; set; }
        public Settings SystemSettings { get; set; }

        public static IFlightCorridor InstantiateFromConfig(ConfigNode configNode)
        {
            if (configNode == null)
                return null;

            FlightCorridorBase instance = null;
            if (configNode.HasNode("Inclination"))
            {
                instance = new FlightCorridorInclinations();
            }
            else 
            {
                instance = new FlightCorridorBase();
            }
            instance.ParseFromConfig(configNode);
            return instance;
        }

        public virtual void CheckStatus(FlightStateData flightState)
        {
            if (!SystemSettings.enableRangeSafety)
            {
                Status = FlightStatus.Disarmed;
                State = RangeState.Disarmed;
                return;
            }

            if (State == RangeState.Armed || State == RangeState.Destruct)
            {
                // once we've entered the armed or destruct state, there is no going back
                return;
            }

            if (State == RangeState.Exempt)
            {
                Status = FlightStatus.Disarmed;
                return;
            }
            State = RangeState.Nominal;

            if (flightState == null)
            {
                Status = FlightStatus.Prelaunch;
                return;
            }

            FlightStatus result = FlightStatus.Nominal;

            result |= CheckVehicleMass(flightState);
            result |= CheckVehicleVelocity(flightState);
            result |= CheckVehicleDistanceFromPad(flightState);
            if ((result & FlightStatus.NominalPadExclusion) != FlightStatus.NominalPadExclusion)
            {
                // only check bearing once we leave the exlcusion radius to avoid spurious short distance variatons
                result |= CheckCorridor(flightState);
            }

            Status = result;
        }
        public void SetRangeStateDescription()
        {
            string description = string.Empty;

            if ((Status & FlightStatus.Disarmed) == FlightStatus.Disarmed)
            {
                description = "";
            }
            else if ((Status & FlightStatus.Prelaunch) == FlightStatus.Prelaunch)
            {
                description = "Awaiting launch";
            }
            else if ((Status & FlightStatus.SafeMass) == FlightStatus.SafeMass)
            {
                description = "Vessel reached safe mass";
            }
            else if ((Status & FlightStatus.SafeRange) == FlightStatus.SafeRange)
            {
                description = "Vessel reached safe range";
            }
            else if ((Status & FlightStatus.SafeSpeed) == FlightStatus.SafeSpeed)
            {
                description = "Vessel reached safe speed";
            }
            else if ((Status & FlightStatus.NominalPadExclusion) == FlightStatus.NominalPadExclusion)
            {
                description = "Over launch facility";
            }
            else if ((Status & FlightStatus.NominalInCorridor) == FlightStatus.NominalInCorridor)
            {
                description = "On course";
            }
            else if ((Status & FlightStatus.CorridorViolation) == FlightStatus.CorridorViolation)
            {
                description = "Departed flight corridor!";
            }
            StatusDescription = description;
        }

        public virtual void DrawEditor()
        {
            GUIUtils.SimpleLabel("Range Name", Name);
            GUIUtils.SimpleTextBox("Safe Range", SafeRange, "km");
            GUIUtils.SimpleTextBox("Safe Velocity ", SafeSpeed, "m/sec");
            GUIUtils.SimpleTextBox("Safe Mass", SafeMass, "tons");
            GUIUtils.SimpleTextBox("Pad Exclusion Radius", PadSafetyRadius, "km");
        }

        protected virtual FlightStatus CheckCorridor(FlightStateData flightState)
        {
            return FlightStatus.NominalInCorridor;
        }

        protected FlightStatus CheckVehicleMass(FlightStateData flightState)
        {
            if (flightState.VesselTotalMass <= SafeMass)
            {
                State = RangeState.Disarmed;
                return FlightStatus.SafeMass;
            }
            return FlightStatus.Nominal;
        }

        protected FlightStatus CheckVehicleVelocity(FlightStateData flightState)
        {
            if (flightState.VesselSurfaceSpeed >= SafeSpeed)
            {
                State = RangeState.Disarmed;
                return FlightStatus.SafeSpeed;
            }
            return FlightStatus.Nominal;
        }

        protected FlightStatus CheckVehicleDistanceFromPad(FlightStateData flightState)
        {
            FlightStatus result = FlightStatus.Nominal;

            var vesselCoords = new Coordinates(flightState.Lattitude, flightState.Longitude);

            var distFromPad = vesselCoords.DistanceTo(PadCoordinates);          
            if (distFromPad <= PadSafetyRadius)
            {
                result |= FlightStatus.NominalPadExclusion;
            } 
            else if (distFromPad >= SafeRange)
            {
                State = RangeState.Disarmed;
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
                State = RangeState.Armed;
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
