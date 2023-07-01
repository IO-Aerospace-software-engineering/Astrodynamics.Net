using System;
using IO.Astrodynamics.Body.Spacecraft;


namespace IO.Astrodynamics.Maneuver
{
    public class ApsidalAlignmentManeuver : ImpulseManeuver
    {
        public ApsidalAlignmentManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit,
            engines)
        {
        }
    }
}