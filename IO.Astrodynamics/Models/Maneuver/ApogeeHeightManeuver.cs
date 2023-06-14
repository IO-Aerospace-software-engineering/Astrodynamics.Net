using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ApogeeHeightManeuver : ImpulseManeuver
    {
        public double TargetApogee { get; }


        public ApogeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft,
            minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetApogee = targetOrbit.ApogeeVector().Magnitude();
        }

        public ApogeeHeightManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            double apogeeRadius, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch,
            maneuverHoldDuration, engines)
        {
            TargetApogee = apogeeRadius;
        }
    }
}