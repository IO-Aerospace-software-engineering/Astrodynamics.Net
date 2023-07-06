using System;
using System.Collections.Generic;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Surface
{
    public class Site : ILocalizable, IEquatable<Site>
    {
        public int Id { get; }
        public int NaifId { get; }
        public string Name { get; }
        public CelestialBody Body { get; }
        public Planetodetic Planetodetic { get; }


        public Frame Frame { get; }


        public Site(int id, string name, CelestialBody body, in Planetodetic planetodetic = new Planetodetic())
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            Body = body;
            Planetodetic = planetodetic;
            Id = id;
            NaifId = body.NaifId * 1000 + id;
            Frame = new Frame(name.ToUpper() + "_TOPO");
        }

        /// <summary>
        /// Get horizontal coordinates
        /// </summary>
        /// <param name="epoch"></param>
        /// <param name="target"></param>
        /// <param name="aberration"></param>
        /// <returns></returns>
        public Horizontal GetHorizontalCoordinates(DateTime epoch, ILocalizable target, Aberration aberration)
        {
            var position = target.GetPosition(epoch, this, Frame, aberration);

            var az = -System.Math.Atan2(position.Y, position.X);
            if (az < 0)
            {
                az += Constants._2PI;
            }

            var el = System.Math.Asin(position.Z / position.Magnitude());

            return new Horizontal(az, el, position.Magnitude());
        }

        public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBody observer, Frame frame, Aberration aberration,
            TimeSpan stepSize)
        {
            return API.Instance.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
        }

        public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration)
        {
            return API.Instance.ReadEphemeris(epoch, observer, this, frame, aberration);
        }

        public Vector3 GetPosition(DateTime epoch, ILocalizable target, Frame frame, Aberration aberration)
        {
            var position = GetEphemeris(epoch, Body, frame, aberration).ToStateVector().Position;
            var targetPosition = target.GetEphemeris(epoch, Body, frame, aberration).ToStateVector().Position;
            return targetPosition - position;
        }

        public Vector3 GetVelocity(DateTime epoch, ILocalizable observer, Frame frame, Aberration aberration)
        {
            var velocity = GetEphemeris(epoch, Body, frame, aberration).ToStateVector().Velocity;
            var targetVelocity = observer.GetEphemeris(epoch, Body, frame, aberration).ToStateVector().Velocity;
            return targetVelocity - velocity;
        }

        public double AngularSeparation(DateTime epoch, ILocalizable target1, ILocalizable target2, Aberration aberration)
        {
            var target1Position = target1.GetPosition(epoch, this, Frame.ICRF, aberration);
            var target2Position = target2.GetPosition(epoch, this, Frame.ICRF, aberration);
            return target1Position.Angle(target2Position);
        }

        public IEnumerable<Window> FindWindowsOnDistanceConstraint(Window searchWindow, INaifObject observer, RelationnalOperator relationalOperator, double value,
            Aberration aberration, TimeSpan stepSize)
        {
            return API.Instance.FindWindowsOnDistanceConstraint(searchWindow, observer, this, relationalOperator, value, aberration, stepSize);
        }

        public IEnumerable<Window> FindWindowsOnOccultationConstraint(Window searchWindow, INaifObject target, ShapeType targetShape, INaifObject frontBody,
            ShapeType frontShape,
            OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
        {
            return API.Instance.FindWindowsOnOccultationConstraint(searchWindow, this, target, targetShape, frontBody, frontShape, occultationType, aberration, stepSize);
        }

        public IEnumerable<Window> FindWindowsOnCoordinateConstraint(Window searchWindow, INaifObject observer, Frame frame, CoordinateSystem coordinateSystem,
            Coordinate coordinate,
            RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize)
        {
            return API.Instance.FindWindowsOnCoordinateConstraint(searchWindow, observer, this, frame, coordinateSystem, coordinate, relationalOperator, value, adjustValue,
                aberration,
                stepSize);
        }

        public IEnumerable<Window> FindWindowsOnIlluminationConstraint(Window searchWindow, INaifObject observer, IlluminationAngle illuminationType,
            RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize, INaifObject illuminationSource,
            string method = "Ellipsoid")
        {
            return API.Instance.FindWindowsOnIlluminationConstraint(searchWindow, observer, Body, Body.Frame, Planetodetic, illuminationType, relationalOperator, value, adjustValue,
                aberration, stepSize, illuminationSource, method);
        }

        public bool Equals(Site other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NaifId == other.NaifId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Site)obj);
        }

        public override int GetHashCode()
        {
            return NaifId;
        }

        public static bool operator ==(Site left, Site right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Site left, Site right)
        {
            return !Equals(left, right);
        }
    }
}