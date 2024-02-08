using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Maneuver
{
    public abstract class Maneuver
    {
        /// <summary>
        /// Gets or sets the ThrustWindow instance.
        /// </summary>
        /// <value>
        /// The ThrustWindow instance.
        /// </value>
        public Window ThrustWindow { get; internal set; }

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
        public DateTime MinimumEpoch { get; internal set; }

        /// <summary>
        /// Gets the duration for which a maneuver should be held.
        /// </summary>
        /// <remarks>
        /// This property represents the duration as a TimeSpan object.
        /// </remarks>
        /// <returns>The maneuver hold duration as a TimeSpan.</returns>
        public TimeSpan ManeuverHoldDuration { get; }

        public Engine Engine { get; }

        /// <summary>
        /// Gets or sets the next maneuver.
        /// </summary>
        public Maneuver NextManeuver { get; protected set; }

        

        /// <summary>
        /// Gets or sets the amount of fuel burned.
        /// </summary>
        public double FuelBurned { get; internal set; }

        protected bool IsInbound { get; set; }
        protected Vector3 ManeuverPoint { get; set; }
        

        protected Maneuver(DateTime minimumEpoch, TimeSpan maneuverHoldDuration, Engine engine)
        {
            MinimumEpoch = minimumEpoch;
            ManeuverHoldDuration = maneuverHoldDuration;
            Engine = engine;
        }

        public Maneuver SetNextManeuver(Maneuver maneuver)
        {
            NextManeuver = maneuver;
            return maneuver;
        }

        internal bool CanExecute(StateVector stateVector)
        {
            //Evaluate Epoch constraint
            if (stateVector.Epoch < MinimumEpoch)
            {
                return false;
            }

            //Compute the target point
            var maneuverPoint = ComputeManeuverPoint(stateVector);

            //Check if target point is reached
            var isInbound = stateVector.Position.Angle(maneuverPoint, stateVector.SpecificAngularMomentum()) > 0.0;
            if (isInbound == IsInbound)
            {
                return false;
            }

            IsInbound = isInbound;

            return isInbound == false;
        }

        protected abstract Vector3 ComputeManeuverPoint(StateVector stateVector);
        protected abstract Vector3 Execute(StateVector vector);
        internal abstract (StateVector sv, StateOrientation so) TryExecute(StateVector stateVector);

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