using System;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public abstract class ImpulseManeuver : Maneuver
    {
        public Vector3 DeltaV { get; internal set; }

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
        {
        }

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, Engine engine) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engine)
        {
        }
    }
}