// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver;

public class RetrogradeAttitude : Maneuver
{
    public RetrogradeAttitude(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
    {
    }

    protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
    {
        throw new NotImplementedException();
    }

    protected override void Execute(StateVector vector)
    {
        throw new NotImplementedException();
    }
}