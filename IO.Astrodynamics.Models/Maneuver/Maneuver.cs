using System;
using System.Linq;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Maneuver
{
    public abstract class Maneuver : Entity
    {
        public Window ThrustWindow { get; protected set; }
        public Window AttitudeWindow { get; protected set; }
        public DateTime MinimumEpoch { get; protected set; }
        public TimeSpan ManeuverHoldDuration { get; protected set; }
        public IReadOnlyCollection<SpacecraftEngine> Engines { get; private set; }
        public Maneuver NextManeuver { get; protected set; }

        public SpacecraftScenario Spacecraft { get; private set; }

        private readonly BodyScenario _targetBody;
        private OrbitalParameters.OrbitalParameters _targetOrbit;

        protected Maneuver() { }

        protected Maneuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, BodyScenario targetBody, params SpacecraftEngine[] engines)
        {
            if (spacecraft == null)
            {
                throw new ArgumentNullException(nameof(spacecraft));
            }

            if (targetBody == null)
            {
                throw new ArgumentException("Target body must be define");
            }

            Spacecraft = spacecraft;
            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engines = engines;
            _targetBody = targetBody;
        }

        protected Maneuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit, params SpacecraftEngine[] engines)
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
            _targetOrbit = targetOrbit;
        }

        protected Maneuver(SpacecraftScenario spacecraft, DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params SpacecraftEngine[] engines)
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

        public Window GetGlobalWindow()
        {
            Window temp = new(ThrustWindow.StartDate, ManeuverHoldDuration);
            return ThrustWindow.Merge(AttitudeWindow).Merge(temp);
        }

        /// <summary>
        /// Get target orbit at epoch
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public OrbitalParameters.OrbitalParameters GetTargetOrbit(DateTime epoch)
        {
            if (_targetBody != null)
            {
                _targetOrbit = _targetBody.GetEphemeris(epoch);
            }
            else if (_targetOrbit != null)
            {
                _targetOrbit = _targetOrbit.AtEpoch(epoch);
            }
            return _targetOrbit;
        }

        /// <summary>
        /// Check if maneuver can execute at epoch
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public virtual bool CanExecute(OrbitalParameters.OrbitalParameters maneuverPoint)
        {
            if (maneuverPoint.Epoch < MinimumEpoch)
            {
                return false;
            }

            return ComputeCanExecute(maneuverPoint);
        }

        /// <summary>
        /// Evaluate if maneuver can execute
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public abstract bool ComputeCanExecute(OrbitalParameters.OrbitalParameters maneuverPoint);

        /// <summary>
        /// Execute maneuver
        /// </summary>
        /// <param name="maneuverPoint"></param>
        public abstract void Execute(OrbitalParameters.OrbitalParameters maneuverPoint);

        /// <summary>
        /// This maneuver become the standby maneuver
        /// </summary>
        /// <param name="minimumEpoch"></param>
        public void Handle(DateTime minimumEpoch)
        {
            this.MinimumEpoch = minimumEpoch;
            this.Spacecraft.SetStandbyManeuver(this);
        }

        /// <summary>
        /// Compute DeltaV and orientation
        /// </summary>
        /// <param name="epoch"></param>
        public abstract Vector3 ComputeDeltaV(OrbitalParameters.OrbitalParameters maneuverPoint);

        /// <summary>
        /// Compute spacecraft orientation
        /// </summary>
        /// <param name="epoch"></param>
        public abstract Quaternion ComputeOrientation(OrbitalParameters.OrbitalParameters maneuverPoint);

        public Maneuver SetNextManeuver(Maneuver maneuver)
        {
            this.NextManeuver = maneuver;
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