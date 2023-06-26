using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class CombinedManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; } = double.NaN;
        public double TargetInclination { get; } = double.NaN;

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, double inclination, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
            TargetInclination = inclination;
        }
    }
}