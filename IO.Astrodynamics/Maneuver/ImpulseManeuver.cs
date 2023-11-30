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

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        protected ImpulseManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public override void Execute(StateVector stateVector)
        {
            if (TargetOrbit.Frame != stateVector.Frame)
            {
                throw new InvalidOperationException("Maneuver must be executed in the same frame");
            }

            var newVelocity = stateVector.Position.Cross(TargetOrbit.SpecificAngularMomentum());
            DeltaV = newVelocity - stateVector.Velocity;
        }
    }
}