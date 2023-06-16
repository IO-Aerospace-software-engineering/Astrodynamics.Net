using System;
using System.Collections.Generic;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;

using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Surface
{
    public class Site : ILocalizable, INaifObject
    {
        public int Id { get; }
        public int NaifId { get; }
        public string Name { get; }
        public CelestialBody Body { get; }
        public Geodetic Geodetic { get; }


        public Frame Frame { get; }

        private readonly API _api = API.Instance;

        public Site(int id, string name, CelestialBody body, in Geodetic geodetic = new Geodetic())
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            Body = body;
            Geodetic = geodetic;
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
                az +=Constants._2PI;
            }
            
            var el = System.Math.Asin(position.Z / position.Magnitude());

            return new Horizontal(az, el, position.Magnitude());
        }

        public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBody observer, Frame frame, Aberration aberration,
            TimeSpan stepSize)
        {
            return _api.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
        }

        public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration)
        {
            return _api.ReadEphemeris(epoch, observer, this, frame, aberration);
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
        public IEnumerable<Window> FindWindowsOnDistanceConstraint(Window searchWindow, INaifObject observer, INaifObject target, RelationnalOperator relationalOperator, double value,
        Aberration aberration, TimeSpan stepSize)
    {
        return _api.FindWindowsOnDistanceConstraint(searchWindow, observer, target, relationalOperator, value, aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnOccultationConstraint(Window searchWindow, INaifObject observer, INaifObject target, ShapeType targetShape, INaifObject frontBody,
        ShapeType frontShape,
        OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
    {
        return _api.FindWindowsOnOccultationConstraint(searchWindow, observer, target, targetShape, frontBody, frontShape, occultationType, aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnCoordinateConstraint(Window searchWindow, INaifObject observer, INaifObject target, Frame frame, CoordinateSystem coordinateSystem,
        Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize)
    {
        return _api.FindWindowsOnCoordinateConstraint(searchWindow, observer, target, frame, coordinateSystem, coordinate, relationalOperator, value, adjustValue, aberration,
            stepSize);
    }

    public IEnumerable<Window> FindWindowsOnIlluminationConstraint(Window searchWindow, INaifObject observer, INaifObject targetBody, Frame fixedFrame, Geodetic geodetic,
        IlluminationAngle illuminationType, RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize,
        INaifObject illuminationSource, string method = "Ellipsoid")
    {
        return _api.FindWindowsOnIlluminationConstraint(searchWindow, observer, targetBody, fixedFrame, geodetic, illuminationType, relationalOperator, value, adjustValue,
            aberration, stepSize, illuminationSource, method);
    }
    }
}