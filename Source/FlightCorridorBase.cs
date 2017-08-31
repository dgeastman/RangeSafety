using System;
using UnityEngine;

namespace RangeSafety
{
    [Flags]
    internal enum FlightStatus : uint
    {
        Nominal = 0x000,
        Prelaunch = 0x001,
        SafeMass = 0x002,
        SafeSpeed = 0x004,
        SafeRange = 0x008,
        NominalPadExclusion = 0x010,
        NominalInCorridor = 0x020,
        AnyNominal = 0x03F,            // includes all the above nominal conditions

        CorridorViolation = 0x040,
        AnyViolation = 0x040,           // includes all the above violation conditions
        Disarmed = 0x100
    }

    internal interface IFlightCorridor
    {
        string Name { get; set; }
        Coordinates PadCoordinates { get; set; }
        EditableInt SafeRange { get; set; }
        EditableInt SafeSpeed { get; set; }
        EditableInt SafeMass { get; set; }
        EditableInt PadSafetyRadius { get; set; }
        FlightStatus Status { get; }
        Settings SystemSettings { get; set; }
        FlightStatus CheckStatus(FlightStateData flightState);
        void DrawEditor();
    }

    internal class FlightCorridorBase : IFlightCorridor
    {
        public string Name { get; set; }
        public Coordinates PadCoordinates { get; set; }
        public EditableInt SafeRange { get; set; }
        public EditableInt SafeSpeed { get; set; }
        public EditableInt SafeMass { get; set; }
        public EditableInt PadSafetyRadius { get; set; }
        public FlightStatus Status { get; protected set; }
        public Settings SystemSettings { get; set; }

        internal static IFlightCorridor InstantiateFromConfig()
        {
            if (FlightGlobals.ActiveVessel == null) return null;

            ConfigNode rootNode = null;
            ConfigNode configNode = null;
            ConfigNode corridorsNode = null;
            ConfigNode tempNode = null;
            ConfigNode defaultNode = null;
            FlightCorridorBase instance = null;

            try
            {
                var path = string.Format("{0}GameData/RangeSafety/FlightCorridors.cfg", KSPUtil.ApplicationRootPath);
                rootNode = ConfigNode.Load(path);

                if (rootNode.TryGetNode("RANGESAFETY", ref tempNode))
                {
                    if (tempNode.TryGetNode("FlightCorridors", ref corridorsNode))
                    {
                        for (int i = 0; i < corridorsNode.nodes.Count; i++)
                        {
                            ConfigNode testNode = corridorsNode.nodes[i];
                            double lat = 0, lon = 0;
                            if (!testNode.TryGetValue("latitude", ref lat))
                            {
                                break;
                            }
                            if (!testNode.TryGetValue("longitude", ref lon))
                            {
                                break;
                            }
                            var vesselCoords = new Coordinates(FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude);
                            var padCoords = new Coordinates(lat, lon);
                            if (vesselCoords.DistanceTo(padCoords) <= 1500)
                            {
                                configNode = testNode;
                                break;
                            }
                            else if (testNode.HasValue("default"))
                            {
                                defaultNode = testNode;
                            }
                        }
                    }
                }

                if (configNode == null)
                {
                    configNode = defaultNode;
                }
                if (configNode == null)
                {
                    return null;
                }
                if (configNode.HasNode("Inclination"))
                {
                    instance = new FlightCorridorInclinations();
                }
                else
                {
                    instance = new FlightCorridorBase();
                }
                instance.ParseFromConfig(configNode);
            }
            catch (Exception e)
            {
                Debug.LogError("FlightCorridorBase.InstantiateFromConfig() caught an exception trying to load FlightCorridors.cfg: " + e);
            }
            return instance;
        }

        public virtual FlightStatus CheckStatus(FlightStateData flightState)
        {
            FlightStatus result = FlightStatus.Nominal;
            if (!SystemSettings.enableRangeSafety)
            {
                result = FlightStatus.Disarmed;
                Status = result;
                return result;
            }

            if (flightState == null)
            {
                result = FlightStatus.Prelaunch;
                Status = result;
                return result;
            }

            result |= CheckVehicleMass(flightState);
            result |= CheckVehicleVelocity(flightState);
            result |= CheckVehicleDistanceFromPad(flightState);
            if ((result & FlightStatus.NominalPadExclusion) != FlightStatus.NominalPadExclusion)
            {
                // only check bearing once we leave the exlcusion radius to avoid spurious short distance variatons
                result |= CheckCorridor(flightState);
            }

            Status = result;
            return result;
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
                return FlightStatus.SafeMass;
            }
            return FlightStatus.Nominal;
        }

        protected FlightStatus CheckVehicleVelocity(FlightStateData flightState)
        {
            if (flightState.VesselSurfaceSpeed >= SafeSpeed)
            {
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

        public static string GetFlightStatusText(FlightStatus status)
        {
            string description = string.Empty;

            if ((status & FlightStatus.Disarmed) == FlightStatus.Disarmed)
            {
                description = "";
            }
            else if ((status & FlightStatus.Prelaunch) == FlightStatus.Prelaunch)
            {
                description = "Awaiting launch";
            }
            else if ((status & FlightStatus.SafeMass) == FlightStatus.SafeMass)
            {
                description = "Vessel reached safe mass";
            }
            else if ((status & FlightStatus.SafeRange) == FlightStatus.SafeRange)
            {
                description = "Vessel reached safe range";
            }
            else if ((status & FlightStatus.SafeSpeed) == FlightStatus.SafeSpeed)
            {
                description = "Vessel reached safe speed";
            }
            else if ((status & FlightStatus.NominalPadExclusion) == FlightStatus.NominalPadExclusion)
            {
                description = "Over launch facility";
            }
            else if ((status & FlightStatus.NominalInCorridor) == FlightStatus.NominalInCorridor)
            {
                description = "On course";
            }
            else if ((status & FlightStatus.CorridorViolation) == FlightStatus.CorridorViolation)
            {
                description = "Departed flight corridor!";
            }
            return description;
        }
    }
}
