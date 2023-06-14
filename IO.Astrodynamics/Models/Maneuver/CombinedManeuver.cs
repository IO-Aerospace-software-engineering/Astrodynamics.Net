using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class CombinedManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; private set; } = double.NaN;
        public double TargetInclination { get; private set; } = double.NaN;

        public CombinedManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public CombinedManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, double inclination, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
            TargetInclination = inclination;
        }
    }
}