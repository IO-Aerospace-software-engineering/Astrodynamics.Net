using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ApogeeHeightManeuver : ImpulseManeuver
    {
        public double TargetApogee { get; }


        public ApogeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft,
            minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetApogee = targetOrbit.ApogeeVector().Magnitude();
        }

        public ApogeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            double apogeeRadius, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch,
            maneuverHoldDuration, engines)
        {
            TargetApogee = apogeeRadius;
        }
    }
}