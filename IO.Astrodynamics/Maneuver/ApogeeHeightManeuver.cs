using System;
using IO.Astrodynamics.Body.Spacecraft;

namespace IO.Astrodynamics.Maneuver
{
    public class ApogeeHeightManeuver : ImpulseManeuver
    {
        public double TargetApogee { get; }

        public ApogeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(spacecraft,
            minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetApogee = targetOrbit.ApogeeVector().Magnitude();
        }

        public ApogeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            double apogeeRadius, params Engine[] engines) : base(spacecraft, minimumEpoch,
            maneuverHoldDuration, engines)
        {
            TargetApogee = apogeeRadius;
        }
    }
}