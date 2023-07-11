using System;
using System.Collections.Generic;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.SolarSystemObjects;
using Window = IO.Astrodynamics.Time.Window;

namespace IO.Astrodynamics.Body;

public abstract class Body : ILocalizable, IEquatable<Body>
{
    public int NaifId { get; }
    public string Name { get; }
    public double Mass { get; }
    public double GM { get; }

    public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; internal set; }
    

    private readonly HashSet<Body> _satellites = new();
    public IReadOnlyCollection<Body> Satellites => _satellites;

    //Used for performance improvement and avoid duplicated call in Celestial body
    protected DTO.CelestialBody ExtendedInformation;

    public bool IsBarycenter => NaifId is >= 0 and < 10;

    public bool IsSun => NaifId == 10;

    public bool IsPlanet => NaifId is > 100 and < 1000 && (NaifId % 100) == 99;

    public bool IsMoon => NaifId is > 100 and < 1000 && (NaifId % 100) != 99;

    public bool IsAsteroid => this.NaifId > 1000;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="naifId">Naif identifier</param>
    /// <param name="frame">Initial orbital parameters frame</param>
    /// <param name="epoch">Epoch</param>
    protected Body(int naifId, Frame frame, DateTime epoch)
    {
        ExtendedInformation = API.Instance.GetCelestialBodyInfo(naifId);

        NaifId = naifId;
        Name = string.IsNullOrEmpty(ExtendedInformation.Name)
            ? throw new InvalidOperationException(
                "Celestial body name can't be defined, please check if you have loaded associated kernels")
            : ExtendedInformation.Name;

        Mass = ExtendedInformation.GM / Constants.G;
        GM = ExtendedInformation.GM;

        if (NaifId != Stars.Sun.NaifId && NaifId != Barycenters.SOLAR_SYSTEM_BARYCENTER.NaifId)
        {
            if (IsBarycenter || IsPlanet || IsMoon)
                InitialOrbitalParameters = GetEphemeris(epoch, new Barycenter(ExtendedInformation.CenterOfMotionId), frame, Aberration.None);

            (InitialOrbitalParameters?.Observer as Body)?._satellites.Add(this);
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="mass"></param>
    /// <param name="initialOrbitalParameters"></param>
    protected Body(int naifId, string name, double mass, OrbitalParameters.OrbitalParameters initialOrbitalParameters)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Body must have a name");
        }

        if (mass <= 0)
        {
            throw new ArgumentException("Body must have a mass");
        }

        NaifId = naifId;
        Name = name;
        Mass = mass;
        InitialOrbitalParameters = initialOrbitalParameters;
        (InitialOrbitalParameters?.Observer as CelestialBody)?._satellites.Add(this);
    }

    internal void AddSatellite(Body body)
    {
        _satellites.Add(body);
    }

    internal void RemoveSatellite(Body body)
    {
        _satellites.Remove(body);
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
    public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, ILocalizable observer,
        Frame frame, Aberration aberration,
        TimeSpan stepSize)
    {
        return API.Instance.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
    }

    public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, ILocalizable observer, Frame frame,
        Aberration aberration)
    {
        return API.Instance.ReadEphemeris(epoch, observer, this, frame, aberration);
    }

    /// <summary>
    /// Return the angular size of a body relative to the distance
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public double AngularSize(double distance)
    {
        return (this is CelestialBody body)
            ? 2.0 * System.Math.Asin((body.EquatorialRadius * 2.0) / (distance * 2.0))
            : 0.0;
    }

    /// <summary>
    /// Compute the angular separation between two localizable objects
    /// </summary>
    /// <param name="epoch"></param>
    /// <param name="target1"></param>
    /// <param name="target2"></param>
    /// <param name="aberration"></param>
    /// <returns></returns>
    public double AngularSeparation(DateTime epoch, ILocalizable target1, ILocalizable target2, Aberration aberration)
    {
        var target1Position = target1.GetEphemeris(epoch, this, Frame.ICRF, aberration).ToStateVector().Position;
        var target2Position = target2.GetEphemeris(epoch, this, Frame.ICRF, aberration).ToStateVector().Position;
        return target1Position.Angle(target2Position);
    }

    public IEnumerable<Window> FindWindowsOnDistanceConstraint(Window searchWindow, INaifObject observer,
        RelationnalOperator relationalOperator, double value,
        Aberration aberration, TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnDistanceConstraint(searchWindow, observer, this, relationalOperator, value,
            aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnOccultationConstraint(Window searchWindow, INaifObject observer,
        ShapeType targetShape, INaifObject frontBody,
        ShapeType frontShape, OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnOccultationConstraint(searchWindow, observer, this, targetShape, frontBody,
            frontShape, occultationType, aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnCoordinateConstraint(Window searchWindow, INaifObject observer, Frame frame,
        CoordinateSystem coordinateSystem,
        Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration,
        TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnCoordinateConstraint(searchWindow, observer, this, frame, coordinateSystem,
            coordinate, relationalOperator, value, adjustValue, aberration,
            stepSize);
    }

    //Return all centers of motion up to the root 
    public IEnumerable<ILocalizable> GetCentersOfMotion()
    {
        List<ILocalizable> celestialBodies = new List<ILocalizable>();

        if (InitialOrbitalParameters?.Observer != null)
        {
            celestialBodies.Add(InitialOrbitalParameters.Observer);
            celestialBodies.AddRange(InitialOrbitalParameters.Observer.GetCentersOfMotion());
        }

        return celestialBodies;
    }

    /// <summary>
    /// Return the sub-observer coordinates based on ellipsoid interception 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="epoch"></param>
    /// <param name="aberration"></param>
    /// <returns></returns>
    public Planetocentric SubObserverPoint(CelestialBody target, DateTime epoch, Aberration aberration)
    {
        var position = GetEphemeris(epoch, target, target.Frame, aberration).ToStateVector().Position;

        var lon = System.Math.Atan2(position.Y, position.X);

        var lat = System.Math.Asin(position.Z / position.Magnitude());

        return new Planetocentric(lon, lat, position.Magnitude());
    }

    public override string ToString()
    {
        return Name;
    }

    public abstract double GetTotalMass();


    public bool Equals(Body other)
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
        return Equals((Body)obj);
    }

    public override int GetHashCode()
    {
        return NaifId;
    }

    public static bool operator ==(Body left, Body right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Body left, Body right)
    {
        return !Equals(left, right);
    }
}