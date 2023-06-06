using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.SeedWork;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Mission
{
    public abstract class BodyScenario : Entity, IEquatable<BodyScenario>, ILocalizable
    {
        public Scenario Scenario { get; private set; }
        public Body.Body PhysicalBody { get; protected set; }
        public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; private set; }

        public SortedDictionary<DateTime, StateVector> Trajectory { get; } =
            new SortedDictionary<DateTime, StateVector>();

        public Frame.Frame Frame { get; protected set; }
        private readonly HashSet<BodyScenario> _satellites = new();
        public IReadOnlyCollection<BodyScenario> Satellites => _satellites;

        protected BodyScenario()
        {
        }

        protected BodyScenario(Body.Body body, OrbitalParameters.OrbitalParameters initialOrbitalParameters,
            Frame.Frame frame, Scenario scenario, int id = default) : base(id)
        {
            PhysicalBody = body ?? throw new ArgumentNullException(nameof(body));
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            if (initialOrbitalParameters != null)
            {
                SetInitialOrbitalParameters(initialOrbitalParameters);
            }
        }

        protected BodyScenario(Body.Body body, Frame.Frame frame, Scenario scenario) : this(body, null, frame, scenario)
        {
        }

        /// <summary>
        /// Get ephemeris from trajectory. Ephemeris can be discrete or interpolated or extrapolated value.
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public StateVector GetEphemeris(Frame.Frame frame, in DateTime epoch, int accuracy = 9)
        {
            return GetEphemeris(epoch, accuracy)?.ToFrame(frame).ToStateVector();
        }

        /// <summary>
        /// Get ephemeris from trajectory. Ephemeris can be discrete or interpolated or extrapolated value.
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public StateVector GetEphemeris(in DateTime epoch, int accuracy = 9)
        {
            if (Trajectory.Any() && epoch >= Trajectory.First().Key && epoch <= Trajectory.Last().Key)
            {
                if (Trajectory.TryGetValue(epoch, out StateVector sv))
                {
                    return sv;
                }

                if (Trajectory.Count >= accuracy)
                {
                    DateTime ep = epoch; //Due to in parameters
                    StateVector[] data = Trajectory.Where(x => x.Key < ep).TakeLast(accuracy).Select(x => x.Value)
                        .ToArray();
                    return Lagrange.Interpolate(data, epoch).ToStateVector();
                }
            }

            if (InitialOrbitalParameters == null)
            {
                return null;
            }

            if (InitialOrbitalParameters.Epoch == epoch)
            {
                return InitialOrbitalParameters.ToStateVector();
            }

            return InitialOrbitalParameters.AtEpoch(epoch).ToStateVector();
        }

        public StateVector GetLatestEphemeris()
        {
            if (Trajectory.Count == 0)
            {
                return InitialOrbitalParameters?.ToStateVector();
            }

            return Trajectory.Last().Value;
        }

        public StateOrientation GetOrientationFromICRF(in DateTime epoch)
        {
            if (Frame.Length == 0)
            {
                throw new InvalidOperationException("Frame's orientation array is empty");
            }

            return Frame.FromICRF(epoch);
        }

        public virtual void AddStateVector(params StateVector[] stateVectors)
        {
            foreach (var stateVector in stateVectors)
            {
                if (stateVector == null)
                {
                    throw new ArgumentNullException(nameof(stateVector));
                }

                if (Trajectory.ContainsKey(stateVector.Epoch))
                {
                    Trajectory.Remove(stateVector.Epoch);
                }

                Trajectory.Add(stateVector.Epoch, stateVector);
            }

            //Update initialOrbitalParameters
            if (InitialOrbitalParameters == null)
            {
                SetInitialOrbitalParameters(Trajectory.First().Value);
            }
        }

        public virtual void SetInitialOrbitalParameters(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            InitialOrbitalParameters = orbitalParameters;
            InitialOrbitalParameters.CenterOfMotion._satellites.Add(this);
        }

        public virtual void AddStateOrientationFromICRF(StateOrientation orientation)
        {
            if (orientation == null)
            {
                throw new ArgumentNullException(nameof(orientation));
            }

            Frame.AddStateOrientationFromICRF(orientation);
        }

        /// <summary>
        /// Get state vector relative to target in given frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="targetBody"></param>
        /// <param name="epoch"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public StateVector RelativeStateVector(Frame.Frame frame, BodyScenario targetBody, in DateTime epoch)
        {
            if (targetBody is null)
            {
                throw new ArgumentNullException(nameof(targetBody));
            }

            if (targetBody == this)
            {
                throw new ArgumentException("targetBody must be different from this body");
            }

            var targetBodySv = targetBody.GetEphemeris(frame, epoch)?.ToStateVector();
            var currentSv = GetEphemeris(frame, epoch)?.ToStateVector();
            //Case is a satellite
            if (Satellites.Contains(targetBody))
            {
                return targetBodySv;
            }

            List<BodyScenario> parents = new List<BodyScenario>();
            BodyScenario parent = currentSv?.CenterOfMotion ?? this;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.InitialOrbitalParameters?.CenterOfMotion;
            }

            List<BodyScenario> targetParents = new List<BodyScenario>();
            BodyScenario targetParent = targetBodySv?.CenterOfMotion ?? targetBody;
            while (targetParent != null)
            {
                targetParents.Add(targetParent);
                targetParent = targetParent.InitialOrbitalParameters?.CenterOfMotion;
            }

            var commonBody = parents.Intersect(targetParents).FirstOrDefault();

            if (commonBody == null)
                throw new InvalidOperationException("Bodies don't have a common center of motion");

            //current body branch
            var centerOfMotion = currentSv?.CenterOfMotion;
            while (centerOfMotion != commonBody && centerOfMotion?.InitialOrbitalParameters != null)
            {
                currentSv += centerOfMotion.GetEphemeris(frame, epoch).ToStateVector();
                centerOfMotion = currentSv.CenterOfMotion;
            }

            //target body branch
            var targetSv = targetBody.GetEphemeris(frame, epoch)?.ToStateVector();
            var targetCenterOfMotion = targetSv?.CenterOfMotion;
            while (targetCenterOfMotion != commonBody && targetCenterOfMotion?.InitialOrbitalParameters != null)
            {
                targetSv += targetCenterOfMotion.GetEphemeris(frame, epoch).ToStateVector();
                targetCenterOfMotion = targetSv.CenterOfMotion;
            }

            if (targetSv != null && currentSv != null)
            {
                return targetSv - currentSv;
            }

            return targetSv ?? currentSv.Inverse();
        }

        /// <summary>
        /// State vector relative to target body
        /// </summary>
        /// <param name="targetBody"></param>
        /// <param name="epoch"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public StateVector RelativeStateVector(BodyScenario targetBody, in DateTime epoch)
        {
            return RelativeStateVector(IO.Astrodynamics.Models.Frame.Frame.ICRF, targetBody, epoch);
        }

        public double AngularSepration(BodyScenario target, ILocalizable observer, in DateTime epoch)
        {
            var targetPosition = observer.RelativeStateVector(IO.Astrodynamics.Models.Frame.Frame.ICRF, this, epoch).Position;
            var byPosition = observer.RelativeStateVector(IO.Astrodynamics.Models.Frame.Frame.ICRF, target, epoch).Position;
            return targetPosition.Angle(byPosition);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BodyScenario);
        }

        public bool Equals(BodyScenario other)
        {
            return other != null &&
                   EqualityComparer<Body.Body>.Default.Equals(PhysicalBody, other.PhysicalBody);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PhysicalBody?.NaifId);
        }

        public static bool operator ==(BodyScenario left, BodyScenario right)
        {
            return EqualityComparer<BodyScenario>.Default.Equals(left, right);
        }

        public static bool operator !=(BodyScenario left, BodyScenario right)
        {
            return !(left == right);
        }
    }
}