using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class PlaneAlignmentManeuver : ImpulseManeuver
    {
        public PlaneAlignmentManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }
    }
}