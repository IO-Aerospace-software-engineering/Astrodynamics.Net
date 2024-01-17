using System;
using System.Collections.Generic;
using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Surface
{
    public class Site : ILocalizable, IEquatable<Site>
    {
        public int Id { get; }
        public int NaifId { get; }
        public string Name { get; }
        public CelestialBody CelestialBody { get; }
        public Planetodetic Planetodetic { get; }
        public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; }

        public IEnumerable<ILocalizable> GetCentersOfMotion()
        {
            List<ILocalizable> celestialBodies = new List<ILocalizable>();

            if (InitialOrbitalParameters?.Observer == null) return celestialBodies;
            celestialBodies.Add(InitialOrbitalParameters.Observer);
            celestialBodies.AddRange(InitialOrbitalParameters.Observer.GetCentersOfMotion());

            return celestialBodies;
        }

        

        public Frame Frame { get; }
        public double GM { get; } = 0.0;
        public double Mass { get; } = 0.0;

        public Site(int id, string name, CelestialBody celestialItem) : this(id, name, celestialItem, new Planetodetic(double.NaN, double.NaN, double.NaN))
        {
        }

        public Site(int id, string name, CelestialBody celestialItem, Planetodetic planetodetic)
        {
            if (celestialItem == null) throw new ArgumentNullException(nameof(celestialItem));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            CelestialBody = celestialItem;
            Id = id;
            NaifId = celestialItem.NaifId * 1000 + id;
            Frame = new Frame(name.ToUpper() + "_TOPO");
            if (double.IsNaN(planetodetic.Latitude))
            {
                InitialOrbitalParameters = GetEphemeris(DateTimeExtension.J2000, CelestialBody, CelestialBody.Frame, Aberration.None);
                Planetodetic = GetPlanetocentricCoordinates().ToPlanetodetic(CelestialBody.Flattening, CelestialBody.EquatorialRadius);
            }
            else
            {
                Planetodetic = planetodetic;
                InitialOrbitalParameters = new StateVector(Planetodetic.ToPlanetocentric(CelestialBody.Flattening, CelestialBody.EquatorialRadius).ToCartesianCoordinates(), Vector3.Zero, CelestialBody,
                    DateTimeExtension.J2000, CelestialBody.Frame);
            }
        }


        private Planetocentric GetPlanetocentricCoordinates()
        {
            var position = InitialOrbitalParameters.ToStateVector().Position;

            var lon = System.Math.Atan2(position.Y, position.X);

            var lat = System.Math.Asin(position.Z / position.Magnitude());

            return new Planetocentric(lon, lat, position.Magnitude());
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
            var position = target.GetEphemeris(epoch, this, Frame, aberration).ToStateVector().Position;

            var az = -System.Math.Atan2(position.Y, position.X);
            if (az < 0)
            {
                az += Constants._2PI;
            }

            var el = System.Math.Asin(position.Z / position.Magnitude());

            return new Horizontal(az, el, position.Magnitude());
        }

        public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, ILocalizable observer, Frame frame, Aberration aberration,
            TimeSpan stepSize)
        {
            return API.Instance.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
        }

        public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, ILocalizable observer, Frame frame, Aberration aberration)
        {
            return API.Instance.ReadEphemeris(epoch, observer, this, frame, aberration);
        }

        public double AngularSeparation(DateTime epoch, ILocalizable target1, ILocalizable target2, Aberration aberration)
        {
            var target1Position = target1.GetEphemeris(epoch, this, Frame.ICRF, aberration).ToStateVector().Position;
            var target2Position = target2.GetEphemeris(epoch, this, Frame.ICRF, aberration).ToStateVector().Position;
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
            return API.Instance.FindWindowsOnIlluminationConstraint(searchWindow, observer, CelestialBody, CelestialBody.Frame, Planetodetic, illuminationType, relationalOperator, value,
                adjustValue,
                aberration, stepSize, illuminationSource, method);
        }

        public void Propagate(Window window, DirectoryInfo outputDirectoryInfo)
        {
            API.Instance.PropagateSite(window, this, outputDirectoryInfo);
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
            return obj.GetType() == this.GetType() && Equals((Site)obj);
        }
        
        public Planetocentric SubObserverPoint(CelestialBody target, DateTime epoch, Aberration aberration)
        {
            var position = GetEphemeris(epoch, target, target.Frame, aberration).ToStateVector().Position;

            var lon = System.Math.Atan2(position.Y, position.X);

            var lat = System.Math.Asin(position.Z / position.Magnitude());

            return new Planetocentric(lon, lat, position.Magnitude());
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