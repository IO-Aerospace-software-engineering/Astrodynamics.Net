using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;

namespace IO.Astrodynamics.Maneuver
{
    public abstract class ImpulseManeuver : Maneuver
    {
        public Vector3 DeltaV { get; internal set; }

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }
    }
}