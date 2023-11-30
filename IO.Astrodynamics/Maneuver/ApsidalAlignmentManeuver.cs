using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;


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