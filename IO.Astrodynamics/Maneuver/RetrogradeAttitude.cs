// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver;

public class RetrogradeAttitude : Maneuver
{
    public RetrogradeAttitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
    {
    }

    protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
    {
        throw new NotImplementedException();
    }

    protected override Vector3 Execute(StateVector vector)
    {
        throw new NotImplementedException();
    }
}