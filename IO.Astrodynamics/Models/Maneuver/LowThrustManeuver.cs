using System;
using IO.Astrodynamics.Models.Body.Spacecraft;

namespace IO.Astrodynamics.Models.Maneuver
{
    public abstract class LowThrustManeuver : Maneuver
    {
        protected LowThrustManeuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }
    }
}