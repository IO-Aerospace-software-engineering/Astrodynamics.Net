using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Maneuver
{
    public abstract class Maneuver
    {
        public Window ThrustWindow { get; internal set; }
        public Window AttitudeWindow { get; internal set; }
        public Window ManeuverWindow { get; internal set; }
        public DateTime MinimumEpoch { get; }
        public TimeSpan ManeuverHoldDuration { get; }
        public IReadOnlyCollection<SpacecraftEngine> Engines { get; private set; }
        public Maneuver NextManeuver { get; protected set; }

        //todo Maybe could I remove it
        public Spacecraft Spacecraft { get; private set; }

        public OrbitalParameters.OrbitalParameters TargetOrbit { get; }

        public double FuelBurned { get; internal set; }

        protected Maneuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params SpacecraftEngine[] engines)
        {
            if (spacecraft == null)
            {
                throw new ArgumentNullException(nameof(spacecraft));
            }

            if (targetOrbit == null)
            {
                throw new ArgumentException("Target orbit must be define");
            }

            Spacecraft = spacecraft;
            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engines = engines;
            TargetOrbit = targetOrbit;
        }

        protected Maneuver(Spacecraft spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines)
        {
            if (spacecraft == null)
            {
                throw new ArgumentNullException(nameof(spacecraft));
            }

            Spacecraft = spacecraft;
            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engines = engines;
        }

        public Maneuver SetNextManeuver(Maneuver maneuver)
        {
            NextManeuver = maneuver;
            return maneuver;
        }

        public static double ComputeDeltaV(double isp, double initialMass, double finalMass)
        {
            return isp * Constants.g0 * System.Math.Log(initialMass / finalMass) * 1E-03;
        }

        public static TimeSpan ComputeDeltaT(double isp, double initialMass, double fuelFlow, double deltaV)
        {
            return TimeSpan.FromSeconds(initialMass / fuelFlow * (1 - System.Math.Exp(-deltaV * 1E03 / (isp * Constants.g0))));
        }

        public static double ComputeDeltaM(double isp, double initialMass, double deltaV)
        {
            return initialMass * (1 - System.Math.Exp(-deltaV * 1E03 / (isp * Constants.g0)));
        }
    }
}