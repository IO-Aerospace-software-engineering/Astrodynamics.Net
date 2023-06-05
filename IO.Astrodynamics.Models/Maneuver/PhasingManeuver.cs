using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class PhasingManeuver : ImpulseManeuver
    {
        public double TargetTrueLongitude { get; private set; } = double.NaN;
        public uint RevolutionNumber { get; private set; }
        PhasingManeuver() : base() { }
        public PhasingManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, uint revolutionNumber, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetBody, engines)
        {
            RevolutionNumber = revolutionNumber;
        }

        public PhasingManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, uint revolutionNumber, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, targetOrbit, engines)
        {
            RevolutionNumber = revolutionNumber;
        }

        public PhasingManeuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, double trueLongitude, uint revolutionNumber, params SpacecraftEngine[] engines) : base(spacecraft, minimumEpoch, maneuverHoldDuration, engines)
        {
            TargetTrueLongitude = trueLongitude;
            RevolutionNumber = revolutionNumber;
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
            if (double.IsNaN(TargetTrueLongitude))
            {
                var targetOrbit = GetTargetOrbit(maneuverPoint.Epoch);
                TargetTrueLongitude = targetOrbit.TrueLongitude();
            }

            double deltaTrueAnomaly = TargetTrueLongitude - maneuverPoint.TrueLongitude();
            double e = maneuverPoint.Eccentricity();

            double E = 2 * System.Math.Atan((System.Math.Sqrt((1 - e) / (1 + e))) * System.Math.Tan(deltaTrueAnomaly / 2.0));
            double T1 = maneuverPoint.Period().TotalSeconds;
            double t = T1 / Constants._2PI * (E - e * System.Math.Sin(E));

            double T2 = T1 - t / RevolutionNumber;

            double u = maneuverPoint.CenterOfMotion.PhysicalBody.GM;

            double a2 = System.Math.Pow((System.Math.Sqrt(u) * T2 / Constants._2PI), 2.0 / 3.0);

            double rp = maneuverPoint.PerigeeVector().Magnitude();
            double ra = 2 * a2 - rp;

            double h2 = System.Math.Sqrt(2 * u) * System.Math.Sqrt(ra * rp / (ra + rp));

            double dv = h2 / rp - maneuverPoint.SpecificAngularMomentum().Magnitude() / rp;

            ManeuverHoldDuration = TimeSpan.FromSeconds(T2 * RevolutionNumber * 0.9);//Hold maneuver for 90% of maneuver total time

            return maneuverPoint.ToStateVector().Velocity.Normalize() * dv;
        }

        public override Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            return SpacecraftScenario.Front.To(DeltaV);
        }
    }
}