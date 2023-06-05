using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ApsidalAlignmentManeuver : ImpulseManeuver
    {
        public double Theta { get; private set; }
        public bool IntersectionAtP { get; private set; }
        public bool IntersectionAtQ { get; private set; }

        ApsidalAlignmentManeuver():base(){}
        protected ApsidalAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
        }

        public ApsidalAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {
        }

        public ApsidalAlignmentManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
        }

        public override bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            Theta = ComputeTheta(maneuverPoint);
            double pv = TrueAnomalyAtP(maneuverPoint);
            double qv = TrueAnomalyAtQ(maneuverPoint);
            double v = maneuverPoint.TrueAnomaly();
            double vRelativeToP = v - pv;
            double vRelativeToQ = v - qv;

            if (vRelativeToP < 0.0) vRelativeToP += Constants._2PI;
            if (vRelativeToQ < 0.0) vRelativeToQ += Constants._2PI;

            vRelativeToP %= Constants._2PI;
            vRelativeToQ %= Constants._2PI;

            //TODO:manage case where pv or pq == 359° and Tolerance + 2° (that does mean uppervalue ==361°)
            if (vRelativeToP > Constants._2PI - Constants.AngularTolerance || vRelativeToP < Constants.AngularTolerance)
            {
                IntersectionAtP = true;
            }
            else if (vRelativeToQ > Constants._2PI - Constants.AngularTolerance || vRelativeToQ < Constants.AngularTolerance)
            {
                IntersectionAtQ = true;
            }
            return IntersectionAtP || IntersectionAtQ;
        }

        public override Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            var currentTargetOrbit = GetTargetOrbit(maneuverPoint.Epoch);
            double targetMeanAnomaly = 0.0;

            if (IntersectionAtP)
            {
                targetMeanAnomaly = currentTargetOrbit.MeanAnomaly(TargetTrueAnomalyAtP(maneuverPoint));
            }
            else if (IntersectionAtQ)
            {
                targetMeanAnomaly = currentTargetOrbit.MeanAnomaly(TargetTrueAnomalyAtQ(maneuverPoint));
            }
            else
            {
                throw new InvalidOperationException("Orbits must instersect");
            }

            OrbitalParameters.KeplerianElements targetOrbit = new OrbitalParameters.KeplerianElements(currentTargetOrbit.SemiMajorAxis(), currentTargetOrbit.Eccentricity(), currentTargetOrbit.Inclination(), currentTargetOrbit.AscendingNode(), currentTargetOrbit.ArgumentOfPeriapsis(), targetMeanAnomaly, currentTargetOrbit.CenterOfMotion, currentTargetOrbit.Epoch, currentTargetOrbit.Frame);
            return targetOrbit.ToStateVector().Velocity - maneuverPoint.ToStateVector().Velocity;
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return SpacecraftScenario.Front.To(DeltaV);
        }

        double TrueAnomalyAtP(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            var (A, B, C, alpha) = GetCoefficients(maneuverPoint);
            double res = alpha + System.Math.Acos((C / A) * System.Math.Cos(alpha));
            if (double.IsNaN(res))
            {
                throw new InvalidOperationException("Apsidal alignment requieres orbits intersection");
            }

            if (res < 0.0)
            {
                res += Constants._2PI;
            }
            return res;
        }

        double TrueAnomalyAtQ(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            var (A, B, C, alpha) = GetCoefficients(maneuverPoint);
            double res = alpha - System.Math.Acos(C / A * System.Math.Cos(alpha));
            if (double.IsNaN(res))
            {
                throw new InvalidOperationException("Apsidal alignment requieres orbits intersection");
            }

            if (res < 0.0)
            {
                res += Constants._2PI;
            }
            return res;
        }

        (double A, double B, double C, double alpha) GetCoefficients(OrbitalParameters.OrbitalParameters maneuverPoints)
        {
            double h1 = System.Math.Pow(maneuverPoints.SpecificAngularMomentum().Magnitude(), 2.0);
            var target = GetTargetOrbit(maneuverPoints.Epoch);
            double h2 = System.Math.Pow(target.SpecificAngularMomentum().Magnitude(), 2.0);
            
            double a = h2 * maneuverPoints.Eccentricity() - h1 * target.Eccentricity() * System.Math.Cos(Theta);
            double b = -h1 * target.Eccentricity() * System.Math.Sin(Theta);
            double c = h1 - h2;
            double alpha = System.Math.Atan(b / a);

            return (a, b, c, alpha);
        }

        double ComputeTheta(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return maneuverPoint.PerigeeVector().Angle(GetTargetOrbit(maneuverPoint.Epoch).PerigeeVector());
        }

        double TargetTrueAnomalyAtP(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            var res = TrueAnomalyAtP(orbitalParameters) - Theta;
            if (res < 0.0)
            {
                res += Constants._2PI;
            }
            return res;
        }

        double TargetTrueAnomalyAtQ(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            var res = TrueAnomalyAtQ(orbitalParameters) - Theta;
            if (res < 0.0)
            {
                res += Constants._2PI;
            }
            return res;
        }
    }
}