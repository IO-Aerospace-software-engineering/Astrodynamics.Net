// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

public class GravitationalField : ForceBase
{
    public GravitationalField()
    {
    }

    public virtual Vector3 ComputeGravitationalAcceleration(StateVector stateVector)
    {
        CelestialBody centerOfMotion = stateVector.Observer as CelestialBody;

        var position = stateVector.Position;

        return position.Normalize() * (-centerOfMotion.GM / System.Math.Pow(position.Magnitude(), 2.0));
    }

    public override Vector3 Apply(StateVector stateVector)
    {
        return ComputeGravitationalAcceleration(stateVector);
    }
}