﻿using System;
using System.Collections.Generic;

namespace RangeSafety
{
    internal enum RangeState
    {
        Disarmed = 0,
        Nominal = 1,
        Armed = 2,
        Destroyed = 3,
        Exempt = 4,
        Safe = 5
    }

    internal enum RangeActions
    {
        TerminateThrust,
        DestroySolids,
        CoastToApogee,
        ExecuteAbort,
        WaitForAbortToClear,
        TerminateFlight
    }

    internal class FlightRange
    {
        internal RangeState State { get; set; }

        private RangeSafety rangeSafetyInstance;
        private double? abortMET;
        private Queue<RangeActions> actionQueue = new Queue<RangeActions>();

        internal void Initialize(RangeSafety instance)
        {
            rangeSafetyInstance = instance;

            CheckIfRunwayOrLaunchPad();
            if (State != RangeState.Exempt)
            {
                State = rangeSafetyInstance.settings.enableRangeSafety ? RangeState.Nominal : RangeState.Disarmed;
            }
        }

        internal void CheckState(IFlightCorridor flightCorridor)
        {
            var flightState = new FlightStateData(FlightGlobals.ActiveVessel);
            if (State == RangeState.Nominal || State == RangeState.Armed)
            {
                var previousStatus = flightCorridor.Status;
                var currentStatus = flightCorridor.CheckStatus(flightState);
                if (previousStatus != currentStatus)
                {
                    if ((currentStatus & FlightStatus.AnySafe) != 0)
                    {
                        EnterSafeState(currentStatus);
                    }
                    else if (State == RangeState.Nominal)
                    {
                        if ((currentStatus & FlightStatus.AnyViolation) != 0)
                            // there has been a status change and we are no longer nominal
                            EnterArmedState(currentStatus);
                    }
                    else if (State == RangeState.Armed)
                    {
                        if ((currentStatus & FlightStatus.AnyViolation) == 0)
                        {
                            // there has been a status change and we can potentially revert from armed to nominal
                        }
                    }
                }
            }
        }

        internal void ProcessActions()
        {
            if (State == RangeState.Armed && actionQueue.Count > 0)
            {
                var currentAction = actionQueue.Peek();
                bool actionComplete = false;
                switch (currentAction)
                {
                    case RangeActions.TerminateThrust:
                        actionComplete = ExecuteDisableThrustAction();
                        break;
                    case RangeActions.DestroySolids:
                        actionComplete = ExecuteDestroySolidsAction();
                        break;
                    case RangeActions.CoastToApogee:
                        actionComplete = ExecuteCoastToApogeeAction();
                        break;
                    case RangeActions.ExecuteAbort:
                        actionComplete = ExecuteAbortAction();
                        break;
                    case RangeActions.WaitForAbortToClear:
                        actionComplete = ExecuteWaitForAbortToClearAction();
                        break;
                    case RangeActions.TerminateFlight:
                        actionComplete = ExecuteDestroyAction();
                        break;
                }
                if (actionComplete)
                {
                    actionQueue.Dequeue();
                }
            }
        }

        internal static string GetRangeStateText(RangeState rangeState, FlightStatus corridorState)
        {
            string state = "DISARM";
            string description = string.Empty;

            switch (rangeState)
            {
                case RangeState.Nominal:
                    state = "NOMINAL";
                    break;
                case RangeState.Armed:
                    state = "ARM";
                    break;
                case RangeState.Destroyed:
                    state = "DESTROYED";
                    break;
                case RangeState.Safe:
                    state = "SAFE";
                    break;
            }
            description = FlightCorridorBase.GetFlightStatusText(corridorState);
            return string.Format("{0}: {1}", state, description);
        }

        private void CheckIfRunwayOrLaunchPad()
        {
            var vessel = FlightGlobals.ActiveVessel;

            var biome = Vessel.GetLandedAtString(vessel.landedAt);
            //var biome = FlightGlobals.currentMainBody.BiomeMap.GetAtt(Utils.DegreeToRadian(vessel.latitude), Utils.DegreeToRadian(vessel.longitude)).name;
            if (biome == "Runway")
            {
                State = RangeState.Exempt;
            }
        }

        private void EnterArmedState(FlightStatus triggerStatus)
        {
            FlightLogger.eventLog.Add(string.Format("[{0}]: Range safety entered ARM state: {1}", KSPUtil.PrintTimeCompact((int)Math.Floor(FlightGlobals.ActiveVessel.missionTime), false), FlightCorridorBase.GetFlightStatusText(triggerStatus)));
            State = RangeState.Armed;

            if (rangeSafetyInstance.settings.terminateThrustOnArm)
            {
                actionQueue.Enqueue(RangeActions.TerminateThrust);
            }
            if (rangeSafetyInstance.settings.destroySolids)
            {
                actionQueue.Enqueue(RangeActions.DestroySolids);
            }
            if (rangeSafetyInstance.settings.coastToApogeeBeforeAbort)
            {
                actionQueue.Enqueue(RangeActions.CoastToApogee);
            }
            if (rangeSafetyInstance.settings.abortOnArm)
            {
                actionQueue.Enqueue(RangeActions.ExecuteAbort);
            }
            if (rangeSafetyInstance.settings.delay3secAfterAbort)
            {
                actionQueue.Enqueue(RangeActions.WaitForAbortToClear);
            }
            if (rangeSafetyInstance.settings.destroyLaunchVehicle)
            {
                actionQueue.Enqueue(RangeActions.TerminateFlight);
            }
        }

        private void EnterSafeState(FlightStatus triggerStatus)
        {
            FlightLogger.eventLog.Add(string.Format("[{0}]: Range safety entered SAFE state: {1}", KSPUtil.PrintTimeCompact((int)Math.Floor(FlightGlobals.ActiveVessel.missionTime), false), FlightCorridorBase.GetFlightStatusText(triggerStatus)));
            State = RangeState.Safe;
        }

        private bool ExecuteDisableThrustAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightGlobals.ActiveVessel.ctrlState.mainThrottle = 0;
                FlightInputHandler.state.mainThrottle = 0; //so that the on-screen throttle gauge reflects the autopilot throttle
            }
            return true;
        }

        private bool ExecuteDestroySolidsAction()
        {
            var partlist = new List<Part>();
            foreach (var part in FlightGlobals.ActiveVessel.Parts)
            {
                if (part.Resources.Contains("SolidFuel"))
                {
                    var moduleEngine = part.FindModuleImplementing<ModuleEngines>();
                    if (moduleEngine != null && moduleEngine.EngineIgnited)
                    {
                        partlist.Add(part);
                    }
                }
            }

            foreach (var part in partlist)
            {
                part.Die();
            }
            return true;
        }

        private bool ExecuteAbortAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                abortMET = FlightGlobals.ActiveVessel.missionTime;
                FlightLogger.eventLog.Add(string.Format("[{0}]: ABORT triggered by range safety.", KSPUtil.PrintTimeCompact((int)Math.Floor(abortMET.Value), false)));
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(KSPActionGroup.Abort);
            }
            return true;
        }

        private bool ExecuteCoastToApogeeAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                return (FlightGlobals.ActiveVessel.orbit.timeToAp >= FlightGlobals.ActiveVessel.orbit.timeToPe);
            }
            return true;
        }

        private bool ExecuteDestroyAction()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                FlightLogger.eventLog.Add(string.Format("[{0}]: Craft destroyed by range safety.", KSPUtil.PrintTimeCompact((int)Math.Floor(FlightGlobals.ActiveVessel.missionTime), false)));
                FlightGlobals.ActiveVessel.Die();
            }
            return true;
        }

        private bool ExecuteWaitForAbortToClearAction()
        {
            if (FlightGlobals.ActiveVessel != null && abortMET.HasValue)
            {
                return FlightGlobals.ActiveVessel.missionTime >= abortMET + 3;
            }
            return true;
        }

    }
}
