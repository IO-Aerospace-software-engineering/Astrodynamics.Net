using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Maneuver
{
    public abstract class LowThrustManeuver : Maneuver
    {
        protected LowThrustManeuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,params Engine[] engines) : base(minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        protected override Vector3 ComputeManeuverPoint(StateVector stateVector)
        {
            throw new NotImplementedException();
        }

        protected override void Execute(StateVector vector)
        {
            throw new NotImplementedException();
        }
    }
}