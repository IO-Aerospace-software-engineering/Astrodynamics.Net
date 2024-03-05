using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public class PerigeeHeightManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; } = double.NaN;

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, Engine engine) : this(minimumEpoch,
            maneuverHoldDuration, targetOrbit.PerigeeVector().Magnitude(), engine)
        {
        }

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, Engine engine) : base(minimumEpoch, maneuverHoldDuration,
            engine)
        {
            TargetPerigeeHeight = perigeeRadius;
        }

        protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
        {
            return stateVector.ApogeeVector();
        }

        protected override Vector3 Execute(StateVector stateVector)
        {
            var apogee = stateVector.ApogeeVector().Magnitude();
            double vInit = stateVector.Velocity.Magnitude();
            double vFinal = System.Math.Sqrt(stateVector.Observer.GM * ((2.0 / apogee) - (1.0 / ((apogee + TargetPerigeeHeight) / 2.0))));
            return stateVector.Velocity.Normalize() * (vFinal - vInit);
        }
    }
}