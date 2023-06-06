using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class CombinedManeuver : ImpulseManeuver
    {
        public double TargetPerigeeHeight { get; private set; } = double.NaN;
        public double TargetInclination { get; private set; } = double.NaN;

        CombinedManeuver() : base() { }
        public CombinedManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {

        }

        public CombinedManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public CombinedManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double perigeeRadius, double inclination, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetPerigeeHeight = perigeeRadius;
            TargetInclination = inclination;
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            //check nodes and apsidal alignment
            double angleSeparation = maneuverPoint.ApogeeVector().Angle(maneuverPoint.AscendingNodeVector());
            if (angleSeparation <= Constants.AngularTolerance || angleSeparation >= Constants.PI - Constants.AngularTolerance)
            {
                if (Constants.PI <= maneuverPoint.TrueAnomaly() && maneuverPoint.TrueAnomaly() < Constants.PI + Constants.AngularTolerance)
                {
                    return true;
                }
            }

            return false;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (double.IsNaN(TargetPerigeeHeight))
            {
                var orbit = GetTargetOrbit(maneuverPoint.Epoch);
                TargetPerigeeHeight = orbit.ApogeeVector().Magnitude();
                TargetInclination = orbit.Inclination();
            }

            double newa = maneuverPoint.SemiMajorAxis() * 2.0 - maneuverPoint.PerigeeVector().Magnitude() + TargetPerigeeHeight;
            double pe = TargetPerigeeHeight;
            double ap = newa - TargetPerigeeHeight;
            if (ap < TargetPerigeeHeight)
            {
                double tmp = pe;
                pe = ap;
                ap = tmp;
            }

            double e = (ap - pe) / (ap + pe);

            KeplerianElements ke = new KeplerianElements(newa * 0.5, e, TargetInclination, maneuverPoint.AscendingNode(), maneuverPoint.ArgumentOfPeriapsis(), maneuverPoint.MeanAnomaly(), maneuverPoint.CenterOfMotion, maneuverPoint.Epoch, maneuverPoint.Frame);

            return ke.ToStateVector().Velocity - maneuverPoint.ToStateVector().Velocity;
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return SpacecraftScenario.Front.To(DeltaV);
        }
    }
}