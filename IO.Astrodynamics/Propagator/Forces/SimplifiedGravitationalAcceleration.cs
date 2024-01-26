using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

/// <summary>
/// GravitationalAcceleration force from given celestial body
/// </summary>
public class SimplifiedGravitationalAcceleration : ForceBase
{
    public CelestialBody CelestialBody { get; }

    public SimplifiedGravitationalAcceleration(CelestialBody celestialBody)
    {
        CelestialBody = celestialBody;
    }

    /// <summary>
    /// Evaluate gravitational acceleration at given stateVector
    /// </summary>
    /// <param name="stateVector"></param>
    /// <returns></returns>
    public override Vector3 Apply(StateVector stateVector)
    {
        return CelestialBody.EvaluateGravitationalAcceleration(stateVector);
    }
}