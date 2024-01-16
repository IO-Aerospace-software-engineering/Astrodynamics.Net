using System;
using System.Collections.Generic;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;
using Window = IO.Astrodynamics.Time.Window;

namespace IO.Astrodynamics.Body;

public abstract class CelestialItem : ILocalizable, IEquatable<CelestialItem>
{
    protected const int TITLE_WIDTH = 32;
    protected const int VALUE_WIDTH = 32;
    public int NaifId { get; }
    public string Name { get; }
    public double Mass { get; }
    public double GM { get; }

    public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; internal set; }

    private readonly HashSet<CelestialItem> _satellites = new();
    public IReadOnlyCollection<CelestialItem> Satellites => _satellites;

    //Used for performance improvement and avoid duplicated call in Celestial celestialItem API
    protected DTO.CelestialBody ExtendedInformation;

    public bool IsSpacecraft => NaifId < 0;
    public bool IsBarycenter => NaifId is >= 0 and < 10;
    public bool IsSun => NaifId == 10;
    public bool IsPlanet => NaifId is > 100 and < 1000 && (NaifId % 100) == 99;
    public bool IsMoon => NaifId is > 100 and < 1000 && (NaifId % 100) != 99;
    public bool IsAsteroid => this.NaifId is > 1000 and < 1000000000;
    public bool IsStar => this.NaifId > 1000000000;
    public bool IsLagrangePoint => this.NaifId is 391 or 392 or 393 or 394 or 395;

    /// <summary>
    /// Instantiate celestial item from naif id with orbital parameters at given frame and epoch
    /// </summary>
    /// <param name="naifId">Naif identifier</param>
    /// <param name="frame">Initial orbital parameters frame</param>
    /// <param name="epoch">Epoch</param>
    protected CelestialItem(int naifId, Frame frame, DateTime epoch)
    {
        ExtendedInformation = API.Instance.GetCelestialBodyInfo(naifId);

        NaifId = naifId;
        Name = string.IsNullOrEmpty(ExtendedInformation.Name)
            ? throw new InvalidOperationException(
                "Celestial celestialItem name can't be defined, please check if you have loaded associated kernels")
            : ExtendedInformation.Name;

        Mass = ExtendedInformation.GM / Constants.G;
        GM = ExtendedInformation.GM;

        if (NaifId == Stars.Sun.NaifId || NaifId == Barycenters.SOLAR_SYSTEM_BARYCENTER.NaifId) return;
        if (IsPlanet || IsMoon || IsBarycenter)
            InitialOrbitalParameters = GetEphemeris(epoch, new Barycenter(ExtendedInformation.BarycenterOfMotionId), frame, Aberration.None);

        (InitialOrbitalParameters?.Observer as CelestialItem)?._satellites.Add(this);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="mass"></param>
    /// <param name="initialOrbitalParameters"></param>
    protected CelestialItem(int naifId, string name, double mass, OrbitalParameters.OrbitalParameters initialOrbitalParameters)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("CelestialItem must have a name");
        }

        if (mass < 0)
        {
            throw new ArgumentException("CelestialItem can't have a negative mass");
        }

        NaifId = naifId;
        Name = name;
        Mass = mass;
        GM = mass * Constants.G;
        InitialOrbitalParameters = initialOrbitalParameters;
        (InitialOrbitalParameters?.Observer as CelestialBody)?._satellites.Add(this);
    }

    internal void AddSatellite(CelestialItem celestialItem)
    {
        _satellites.Add(celestialItem);
    }

    internal void RemoveSatellite(CelestialItem celestialItem)
    {
        _satellites.Remove(celestialItem);
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

    /// <summary>
    /// GetEphemeris
    /// </summary>
    /// <param name="epoch"></param>
    /// <param name="observer"></param>
    /// <param name="frame"></param>
    /// <param name="aberration"></param>
    /// <returns></returns>
    public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, ILocalizable observer, Frame frame,
        Aberration aberration)
    {
        return API.Instance.ReadEphemeris(epoch, observer, this, frame, aberration);
    }

    /// <summary>
    /// Return the angular size of a celestialItem relative to the distance
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

        if (InitialOrbitalParameters?.Observer == null) return celestialBodies;
        celestialBodies.Add(InitialOrbitalParameters.Observer);
        celestialBodies.AddRange(InitialOrbitalParameters.Observer.GetCentersOfMotion());

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
        return $"{"Identifier :",TITLE_WIDTH} {NaifId,-VALUE_WIDTH}{Environment.NewLine}" +
               $"{"Name :",TITLE_WIDTH} {Name,-VALUE_WIDTH}{Environment.NewLine}" +
               $"{"Mass (kg) :",TITLE_WIDTH} {Mass,-VALUE_WIDTH:E}{Environment.NewLine}" +
               $"{"GM (m^3.s^2):",TITLE_WIDTH} {GM,-VALUE_WIDTH:E}{Environment.NewLine}";
    }

    public bool Equals(CelestialItem other)
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
        return Equals((CelestialItem)obj);
    }

    public override int GetHashCode()
    {
        return NaifId;
    }

    public static bool operator ==(CelestialItem left, CelestialItem right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CelestialItem left, CelestialItem right)
    {
        return !Equals(left, right);
    }
}