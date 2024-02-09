// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Maneuver;

public abstract class Attitude : Maneuver
{
    public Attitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
    {
    }

    protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
    {
        return stateVector.Position;
    }

    protected override Vector3 Execute(StateVector vector)
    {
        return Vector3.Zero;
    }

    protected abstract Quaternion ComputeOrientation(StateVector stateVector);

    public override (StateVector sv, StateOrientation so) TryExecute(StateVector stateVector)
    {
        //Compute maneuver window
        ManeuverWindow = new Window(stateVector.Epoch, ManeuverHoldDuration);
        
        //If state vector is outside maneuver windows the next maneuver can be set
        if (stateVector.Epoch > ManeuverWindow.EndDate)
        {
            Engine.FuelTank.Spacecraft.SetStandbyManeuver(this.NextManeuver, ManeuverWindow.EndDate);
        }
        
        //Return state vector and computed state orientation
        return (stateVector, new StateOrientation(ComputeOrientation(stateVector), Vector3.Zero, stateVector.Epoch, stateVector.Frame));
    }
}