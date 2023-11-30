using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public class CombinedManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; } = double.NaN;
        public double TargetInclination { get; } = double.NaN;

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetPerigeeHeight = targetOrbit.PerigeeVector().Magnitude();
            TargetInclination = targetOrbit.Inclination();
        }

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, double inclination, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
            TargetInclination = inclination;
        }

        public override Vector3 ManeuverPointComputation(StateVector stateVector)
        {
            throw new NotImplementedException();
        }
    }
}