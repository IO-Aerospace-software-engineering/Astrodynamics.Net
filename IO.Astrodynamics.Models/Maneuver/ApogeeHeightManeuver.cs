using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ApogeeHeightManeuver : ImpulseManeuver
    {
        public double TargetApogee { get; private set; } = double.NaN;

        ApogeeHeightManeuver() : base() { }
        public ApogeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {

        }

        public ApogeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public ApogeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double apogeeRadius, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetApogee = apogeeRadius;
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (maneuverPoint.IsCircular() || maneuverPoint.TrueAnomaly() < Constants.AngularTolerance)
            {
                return true;
            }

            return false;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (double.IsNaN(TargetApogee))
            {
                var orbit = GetTargetOrbit(maneuverPoint.Epoch);
                TargetApogee = orbit.ApogeeVector().Magnitude();
            }

            double vInit = maneuverPoint.ToStateVector().Velocity.Magnitude();
            double vFinal = System.Math.Sqrt(maneuverPoint.CenterOfMotion.PhysicalBody.GM * ((2.0 / maneuverPoint.PerigeeVector().Magnitude()) - (1.0 / ((maneuverPoint.PerigeeVector().Magnitude() + TargetApogee) / 2.0))));
            return maneuverPoint.ToStateVector().Velocity.Normalize() * (vFinal - vInit);
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return SpacecraftScenario.Front.To(DeltaV);
        }
    }
}