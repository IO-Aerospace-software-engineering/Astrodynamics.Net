using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class PerigeeHeightManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; } = double.NaN;

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
        }
    }
}