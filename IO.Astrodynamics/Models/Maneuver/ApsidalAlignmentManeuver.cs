using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Mission;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class ApsidalAlignmentManeuver : ImpulseManeuver
    {
        public double Theta { get; private set; }
        public bool IntersectionAtP { get; private set; }
        public bool IntersectionAtQ { get; private set; }

        protected ApsidalAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch,
            TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch,
            maneuverHoldDuration, engines)
        {
        }

        public ApsidalAlignmentManeuver(Spacecraft spacecraft, DateTime minimumEpoch,
            TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit,
            engines)
        {
        }

        public double TrueAnomalyAtP(OrbitalParameters.OrbitalParameters maneuverPoint)
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

        public double TrueAnomalyAtQ(OrbitalParameters.OrbitalParameters maneuverPoint)
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
            var target = TargetOrbit.AtEpoch(maneuverPoints.Epoch);
            double h2 = System.Math.Pow(target.SpecificAngularMomentum().Magnitude(), 2.0);

            double a = h2 * maneuverPoints.Eccentricity() - h1 * target.Eccentricity() * System.Math.Cos(Theta);
            double b = -h1 * target.Eccentricity() * System.Math.Sin(Theta);
            double c = h1 - h2;
            double alpha = System.Math.Atan(b / a);

            return (a, b, c, alpha);
        }

        public double ComputeTheta(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return maneuverPoint.PerigeeVector().Angle(TargetOrbit.AtEpoch(maneuverPoint.Epoch).PerigeeVector());
        }

        public double TargetTrueAnomalyAtP(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            var res = TrueAnomalyAtP(orbitalParameters) - Theta;
            if (res < 0.0)
            {
                res += Constants._2PI;
            }

            return res;
        }

        public double TargetTrueAnomalyAtQ(OrbitalParameters.OrbitalParameters orbitalParameters)
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