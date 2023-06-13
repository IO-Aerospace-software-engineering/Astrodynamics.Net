using System;
using System.Collections.Generic;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using IO.Astrodynamics.SolarSystemObjects;

namespace IO.Astrodynamics.Models.Mission
{
    public abstract class BodyScenario : IEquatable<BodyScenario>, ILocalizable
    {
        public Scenario Scenario { get;  }
        public Body.Body PhysicalBody { get; protected set; }
        public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; private set; }
        public Frames.Frame Frame { get; }
        private readonly HashSet<BodyScenario> _satellites = new();
        public IReadOnlyCollection<BodyScenario> Satellites => _satellites;

        private readonly API _api = new API();

        protected BodyScenario(Body.Body body, OrbitalParameters.OrbitalParameters initialOrbitalParameters,
            Frames.Frame frame, Scenario scenario)
        {
            PhysicalBody = body ?? throw new ArgumentNullException(nameof(body));
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            if (initialOrbitalParameters != null)
            {
                SetInitialOrbitalParameters(initialOrbitalParameters);
            }

            Scenario.AddBody(this);
        }

        protected BodyScenario(Body.Body body, Frames.Frame frame, Scenario scenario) : this(body, null, frame,
            scenario)
        {
        }

        /// <summary>
        /// Get ephemeris
        /// </summary>
        /// <param name="searchWindow"></param>
        /// <param name="observer"></param>
        /// <param name="frame"></param>
        /// <param name="aberration"></param>
        /// <param name="stepSize"></param>
        /// <returns></returns>
        public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBodyScenario observer, Frames.Frame frame, Aberration aberration, TimeSpan stepSize)
        {
            return _api.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
        }

        public StateOrientation GetOrientationFromICRF(in DateTime epoch)
        {
            return Frames.Frame.ICRF.ToFrame(Frame, epoch);
        }

        public virtual void SetInitialOrbitalParameters(OrbitalParameters.OrbitalParameters orbitalParameters)
        {
            InitialOrbitalParameters = orbitalParameters;
            InitialOrbitalParameters.CenterOfMotion._satellites.Add(this);
        }


        /// <summary>
        /// FindOccultations
        /// </summary>
        /// <param name="searchWindow"></param>
        /// <param name="by"></param>
        /// <param name="byShape"></param>
        /// <param name="target"></param>
        /// <param name="targetShape"></param>
        /// <param name="occultationType"></param>
        /// <param name="aberration"></param>
        /// <param name="coarseStepSize"></param>
        /// <returns></returns>
        public IEnumerable<Window> FindOccultations(in Window searchWindow, INaifObject by,ShapeType byShape, INaifObject target,ShapeType targetShape,OccultationType occultationType,Aberration aberration, in TimeSpan coarseStepSize)
        {
            return _api.FindWindowsOnOccultationConstraint(searchWindow, this, target, targetShape, by, byShape, occultationType, aberration, coarseStepSize);
        }

        public double AngularSize(double distance)
        {
            return (this is CelestialBodyScenario body)
                ? 2.0 * System.Math.Asin((body.PhysicalBody.EquatorialRadius * 2.0) / (distance * 2.0))
                : 0.0;
        }

        public double AngularSeparation(ILocalizable target1, ILocalizable target2,Aberration aberration, in DateTime epoch)
        {
            Window searchWindow = new Window(epoch, epoch);
            var target1Position = target1.GetEphemeris(searchWindow, this, Frame.ICRF, aberration, TimeSpan.FromTicks(1)).First().ToStateVector().Position;
            var target2Position = target2.GetEphemeris(searchWindow, this, Frame.ICRF, aberration, TimeSpan.FromTicks(1)).First().ToStateVector().Position;
            return target1Position.Angle(target2Position);
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