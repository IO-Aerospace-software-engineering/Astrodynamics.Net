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

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, Engine engine) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engine)
        {
            TargetPerigeeHeight = targetOrbit.PerigeeVector().Magnitude();
            TargetInclination = targetOrbit.Inclination();
        }

        public CombinedManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, double inclination, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
        {
            TargetPerigeeHeight = perigeeRadius;
            TargetInclination = inclination;
        }

        protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
        {
            throw new NotImplementedException();
        }

        protected override Vector3 Execute(StateVector vector)
        {
            throw new NotImplementedException();
        }
    }
}