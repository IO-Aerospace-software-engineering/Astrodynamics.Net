using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public class PerigeeHeightManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; } = double.NaN;

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            TargetPerigeeHeight = targetOrbit.PerigeeVector().Magnitude();
        }

        public PerigeeHeightManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
        }

        public override Vector3 ManeuverPointComputation(StateVector stateVector)
        {
            throw new NotImplementedException();
        }
    }
}