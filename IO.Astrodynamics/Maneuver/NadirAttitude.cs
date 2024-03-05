// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver;

public class NadirAttitude : Attitude
{
    public NadirAttitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
    {
    }

    protected override Quaternion ComputeOrientation(StateVector stateVector)
    {
        return Spacecraft.Front.To(stateVector.Position.Inverse());
    }
}