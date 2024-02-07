using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public class PhasingManeuver : ImpulseManeuver
    {
        public double TargetTrueLongitude { get; } = double.NaN;
        public uint RevolutionNumber { get; }

        public PhasingManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, uint revolutionNumber, Engine engine) : base( minimumEpoch, maneuverHoldDuration, targetOrbit, engine)
        {
            RevolutionNumber = revolutionNumber;
        }

        public PhasingManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double trueLongitude, uint revolutionNumber, Engine engine) : base(minimumEpoch, maneuverHoldDuration, engine)
        {
            TargetTrueLongitude = trueLongitude;
            RevolutionNumber = revolutionNumber;
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