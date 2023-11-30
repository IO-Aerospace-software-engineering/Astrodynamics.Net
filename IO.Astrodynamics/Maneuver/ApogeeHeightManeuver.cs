using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public class ApogeeHeightManeuver : ImpulseManeuver
    {
        public double TargetApogee { get; }

        public ApogeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(
            minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetApogee = targetOrbit.ApogeeVector().Magnitude();
        }

        public ApogeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration,
            double apogeeRadius, params Engine[] engines) : base(minimumEpoch,
            maneuverHoldDuration, engines)
        {
            TargetApogee = apogeeRadius;
        }

        public override Vector3 ManeuverPointComputation(StateVector stateVector)
        {
            throw new NotImplementedException();
        }
    }
}