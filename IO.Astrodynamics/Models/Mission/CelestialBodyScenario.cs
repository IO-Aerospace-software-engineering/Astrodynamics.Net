using System;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Mission
{
    public class CelestialBodyScenario : BodyScenario
    {
        public new CelestialBody PhysicalBody
        {
            get => base.PhysicalBody as CelestialBody;
            private init => base.PhysicalBody = value;
        }

        public double SphereOfInfluence { get; private set; }

        public CelestialBodyScenario(CelestialBody celestialBody,
            OrbitalParameters.OrbitalParameters initialOrbitalParameters, Scenario scenario) : base(
            celestialBody, initialOrbitalParameters, new Frames.Frame(celestialBody.FrameName), scenario)
        {
            PhysicalBody = celestialBody ?? throw new ArgumentNullException(nameof(celestialBody));

            SphereOfInfluence = initialOrbitalParameters != null
                ? SphereOfInluence(initialOrbitalParameters.SemiMajorAxis(), celestialBody.Mass,
                    initialOrbitalParameters.CenterOfMotion.PhysicalBody.Mass)
                : double.PositiveInfinity;
        }

        public CelestialBodyScenario(CelestialBody celestialBody, Scenario scenario) : this(
            celestialBody, null, scenario)
        {
        }

        private double SphereOfInluence(double a, double minorMass, double majorMass)
        {
            return a * System.Math.Pow(minorMass / majorMass, 2.0 / 5.0);
        }

        public override void SetInitialOrbitalParameters(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            if (orbitalParameters == null) throw new ArgumentNullException(nameof(orbitalParameters));
            base.SetInitialOrbitalParameters(orbitalParameters);
            SphereOfInfluence = SphereOfInluence(orbitalParameters.SemiMajorAxis(), PhysicalBody.Mass,
                    orbitalParameters.CenterOfMotion.PhysicalBody.Mass);
        }
    }
}