using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PerigeeHeightManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; private set; } = double.NaN;

        public PerigeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public PerigeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
        }
    }
}