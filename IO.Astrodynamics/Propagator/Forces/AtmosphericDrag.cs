using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

public class AtmosphericDrag : ForceBase
{
    private readonly Spacecraft _spacecraft;
    private readonly double _areaMassRatio = 0.0;

    public AtmosphericDrag(Spacecraft spacecraft)
    {
        _spacecraft = spacecraft ?? throw new ArgumentNullException(nameof(spacecraft));
        _areaMassRatio = _spacecraft.SectionalArea / _spacecraft.Mass;
    }

    public override Vector3 Apply(StateVector stateVector)
    {
        var celestialBody = stateVector.Observer as CelestialBody;
        var planetodetic = stateVector.ToPlanetocentric(Aberration.None).ToPlanetodetic(celestialBody!.Flattening, celestialBody.EquatorialRadius);
        var density = celestialBody.GetAirDensity(planetodetic.Altitude);
        return stateVector.Velocity * -0.5 * density * _areaMassRatio * _spacecraft.DragCoefficient * stateVector.Velocity.Magnitude();
    }
}