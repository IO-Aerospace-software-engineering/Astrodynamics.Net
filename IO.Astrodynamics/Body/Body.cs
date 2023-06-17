using System;
using System.Collections.Generic;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Body;

public abstract class Body : ILocalizable, IEquatable<Body>
{
    public int NaifId { get; }
    public string Name { get; protected set; }
    public double Mass { get; protected set; }

    public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; protected set; }
    public Frame Frame { get; protected set; }

    protected readonly HashSet<Body> _satellites = new();
    public IReadOnlyCollection<Body> Satellites => _satellites;

    protected Body(int naifId)
    {
        NaifId = naifId;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="mass"></param>
    /// <param name="initialOrbitalParameters"></param>
    /// <param name="frame"></param>
    protected Body(int naifId, string name, double mass, OrbitalParameters.OrbitalParameters initialOrbitalParameters, Frame frame)
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
        Frame = frame ?? throw new ArgumentNullException(nameof(frame));
        if (InitialOrbitalParameters != null)
        {
            initialOrbitalParameters.CenterOfMotion._satellites.Add(this);
        }
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
    public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBody observer, Frame frame, Aberration aberration,
        TimeSpan stepSize)
    {
        return API.Instance.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
    }

    public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration)
    {
        return API.Instance.ReadEphemeris(epoch, observer, this, frame, aberration);
    }

    public Math.Vector3 GetPosition(DateTime epoch, ILocalizable observer, Frame frame, Aberration aberration)
    {
        var centerOfMotion = InitialOrbitalParameters?.CenterOfMotion ?? this as CelestialBody;
        if (centerOfMotion == null)
        {
            throw new InvalidOperationException("Center of motion can not be defined");
        }

        var targetPosition = GetEphemeris(epoch, centerOfMotion, frame, aberration).ToStateVector().Position;
        var observerPosition = observer.GetEphemeris(epoch, centerOfMotion, frame, aberration).ToStateVector().Position;
        return targetPosition - observerPosition;
    }

    public Math.Vector3 GetVelocity(DateTime epoch, ILocalizable observer, Frame frame, Aberration aberration)
    {
        var centerOfMotion = InitialOrbitalParameters?.CenterOfMotion ?? this as CelestialBody;
        if (centerOfMotion == null)
        {
            throw new InvalidOperationException("Center of motion can not be defined");
        }

        var velocity = GetEphemeris(epoch, centerOfMotion, frame, aberration).ToStateVector().Velocity;
        var targetVelocity = observer.GetEphemeris(epoch, centerOfMotion, frame, aberration).ToStateVector().Velocity;
        return targetVelocity - velocity;
    }

    /// <summary>
    /// Get orientation relative to reference frame
    /// </summary>
    /// <param name="referenceFrame"></param>
    /// <param name="epoch"></param>
    /// <returns></returns>
    public StateOrientation GetOrientation(Frame referenceFrame, in DateTime epoch)
    {
        return referenceFrame.ToFrame(Frame, epoch);
    }

    public virtual void SetInitialOrbitalParameters(OrbitalParameters.OrbitalParameters orbitalParameters)
    {
        if (InitialOrbitalParameters.CenterOfMotion != null)
        {
            InitialOrbitalParameters.CenterOfMotion._satellites.Remove(this);
        }

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
    public IEnumerable<Window> FindOccultations(in Window searchWindow, INaifObject by, ShapeType byShape, INaifObject target, ShapeType targetShape,
        OccultationType occultationType, Aberration aberration, in TimeSpan coarseStepSize)
    {
        return API.Instance.FindWindowsOnOccultationConstraint(searchWindow, this, target, targetShape, by, byShape, occultationType, aberration, coarseStepSize);
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
        var target1Position = target1.GetPosition(epoch, this, Frame.ICRF, aberration);
        var target2Position = target2.GetPosition(epoch, this, Frame.ICRF, aberration);
        return target1Position.Angle(target2Position);
    }

    public IEnumerable<Window> FindWindowsOnDistanceConstraint(Window searchWindow, INaifObject observer, RelationnalOperator relationalOperator, double value,
        Aberration aberration, TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnDistanceConstraint(searchWindow, observer, this, relationalOperator, value, aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnOccultationConstraint(Window searchWindow, INaifObject observer, ShapeType targetShape, INaifObject frontBody,
        ShapeType frontShape, OccultationType occultationType, Aberration aberration, TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnOccultationConstraint(searchWindow, observer, this, targetShape, frontBody, frontShape, occultationType, aberration, stepSize);
    }

    public IEnumerable<Window> FindWindowsOnCoordinateConstraint(Window searchWindow, INaifObject observer, Frame frame, CoordinateSystem coordinateSystem,
        Coordinate coordinate,
        RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize)
    {
        return API.Instance.FindWindowsOnCoordinateConstraint(searchWindow, observer, this, frame, coordinateSystem, coordinate, relationalOperator, value, adjustValue, aberration,
            stepSize);
    }

    public IEnumerable<Window> FindWindowsOnIlluminationConstraint(Window searchWindow, INaifObject observer, Geodetic geodetic,
        IlluminationAngle illuminationType, RelationnalOperator relationalOperator, double value, double adjustValue, Aberration aberration, TimeSpan stepSize,
        INaifObject illuminationSource, string method = "Ellipsoid")
    {
        return API.Instance.FindWindowsOnIlluminationConstraint(searchWindow, observer, this, Frame, geodetic, illuminationType, relationalOperator, value, adjustValue,
            aberration, stepSize, illuminationSource, method);
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