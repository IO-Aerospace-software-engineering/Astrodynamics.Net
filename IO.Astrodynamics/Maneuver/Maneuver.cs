using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Maneuver
{
    public abstract class Maneuver
    {
        public static TimeSpan MANEUVER_POINT_UPDATE_DELAY = TimeSpan.FromMinutes(10.0);

        /// <summary>
        /// Gets or sets the ThrustWindow instance.
        /// </summary>
        /// <value>
        /// The ThrustWindow instance.
        /// </value>
        public Window ThrustWindow { get; internal set; }

        /// <summary>
        /// The AttitudeWindow property represents the window object used to display attitude-related information.
        /// </summary>
        /// <value>
        /// Gets or sets the AttitudeWindow property.
        /// </value>
        public Window AttitudeWindow { get; internal set; }

        /// <summary>
        /// Gets or sets the ManeuverWindow object that represents the window for controlling maneuvering tasks.
        /// </summary>
        /// <value>
        /// The ManeuverWindow object for controlling maneuvering tasks.
        /// </value>
        public Window ManeuverWindow { get; internal set; }

        /// <summary>
        /// Gets the minimum epoch value.
        /// </summary>
        /// <remarks>
        /// The minimum epoch value represents the earliest date and time when the maneuver can be executed
        /// </remarks>
        /// <value>
        /// The minimum epoch value.
        /// </value>
        public DateTime MinimumEpoch { get; }

        /// <summary>
        /// Gets the duration for which a maneuver should be held.
        /// </summary>
        /// <remarks>
        /// This property represents the duration as a TimeSpan object.
        /// </remarks>
        /// <returns>The maneuver hold duration as a TimeSpan.</returns>
        public TimeSpan ManeuverHoldDuration { get; }

        /// <summary>
        /// Represents a collection of engine objects.
        /// </summary>
        /// <returns>A read-only collection of <see cref="Engine"/> objects.</returns>
        public IReadOnlyCollection<Engine> Engines { get; }

        /// <summary>
        /// Gets or sets the next maneuver.
        /// </summary>
        public Maneuver NextManeuver { get; protected set; }

        /// <summary>
        /// Gets the target orbital parameters.
        /// </summary>
        /// <returns>The target orbital parameters.</returns>
        public OrbitalParameters.OrbitalParameters TargetOrbit { get; }

        /// <summary>
        /// Gets or sets the amount of fuel burned.
        /// </summary>
        public double FuelBurned { get; internal set; }

        protected Vector3? ManeuverPoint { get; set; }
        protected bool? IsInbound { get; private set; }

        private DateTime? _latestManeuverPointUpdate;

        protected Maneuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, OrbitalParameters.OrbitalParameters targetOrbit,
            params Engine[] engines)
        {
            if (engines == null) throw new ArgumentNullException(nameof(engines));
            if (targetOrbit == null)
            {
                throw new ArgumentException("Target orbit must be define");
            }

            if (engines.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(engines));

            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engines = engines;
            TargetOrbit = targetOrbit;
        }

        protected Maneuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, params Engine[] engines)
        {
            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engines = engines;
        }

        public Maneuver SetNextManeuver(Maneuver maneuver)
        {
            NextManeuver = maneuver;
            return maneuver;
        }

        /// <summary>
        /// Determines whether the given state vector satisfies the conditions for executing a maneuver.
        /// </summary>
        /// <param name="stateVector">The current state vector.</param>
        /// <returns>True if the maneuver can be executed, false otherwise.</returns>
        public virtual bool CanExecute(StateVector stateVector)
        {
            if (stateVector.Epoch < this.MinimumEpoch)
            {
                return false;
            }

            if (!_latestManeuverPointUpdate.HasValue || stateVector.Epoch - _latestManeuverPointUpdate.Value > MANEUVER_POINT_UPDATE_DELAY)
            {
                UpdateManeuverPoint(stateVector);
            }

            bool isInbound = ManeuverPoint != null && stateVector.Position.Angle(ManeuverPoint.Value) > 0.0;
            if (isInbound || IsInbound != true)
            {
                IsInbound = isInbound;
                return false;
            }

            return true;
        }

        //Compute maneuver parameters from maneuver point and parameters set by user in specified class
        public abstract void Execute(StateVector stateVector);

        public abstract Vector3 ManeuverPointComputation(StateVector stateVector);

        /// <summary>
        /// Updates the maneuver point based on the given state vector.
        /// </summary>
        /// <param name="stateVector">The state vector containing the necessary information.</param>
        public void UpdateManeuverPoint(StateVector stateVector)
        {
            ManeuverPoint = ManeuverPointComputation(stateVector);
            _latestManeuverPointUpdate = stateVector.Epoch;
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