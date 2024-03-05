// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Body;

public class GravitationalField
{
    public virtual Vector3 ComputeGravitationalAcceleration(StateVector stateVector)
    {
        CelestialBody centerOfMotion = stateVector.Observer as CelestialBody;
        var position = stateVector.Position;

        return position.Normalize() * (-centerOfMotion.GM / System.Math.Pow(position.Magnitude(), 2.0));
    }
}