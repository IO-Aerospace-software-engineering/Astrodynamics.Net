using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PerigeeHeightManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; private set; } = double.NaN;
        PerigeeHeightManeuver() : base() { }
        public PerigeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {

        }

        public PerigeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public PerigeeHeightManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (maneuverPoint.IsCircular() || (Constants.PI <= maneuverPoint.TrueAnomaly() && maneuverPoint.TrueAnomaly() < Constants.PI + Constants.AngularTolerance))
            {
                return true;
            }

            return false;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (double.IsNaN(TargetPerigeeHeight))
            {
                var orbit = GetTargetOrbit(maneuverPoint.Epoch);
                TargetPerigeeHeight = orbit.ApogeeVector().Magnitude();
            }

            double vInit = maneuverPoint.ToStateVector().Velocity.Magnitude();
            double vFinal = System.Math.Sqrt(maneuverPoint.CenterOfMotion.PhysicalBody.GM * ((2.0 / maneuverPoint.ApogeeVector().Magnitude()) - (1.0 / ((maneuverPoint.ApogeeVector().Magnitude() + TargetPerigeeHeight) / 2.0))));
            return maneuverPoint.ToStateVector().Velocity.Normalize() * (vFinal - vInit);
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return SpacecraftScenario.Front.To(DeltaV);
        }
    }
}