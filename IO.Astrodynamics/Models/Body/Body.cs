using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;


namespace IO.Astrodynamics.Models.Body;

public abstract class Body : ILocalizable
{
    public int NaifId { get; }
    public string Name { get; }
    public double Mass { get; private set; }

    public OrbitalParameters.OrbitalParameters InitialOrbitalParameters { get; private set; }
    public Frames.Frame Frame { get; }
    private readonly HashSet<Body> _satellites = new();
    public IReadOnlyCollection<Body> Satellites => _satellites;

    private readonly API _api = new API();

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
        InitialOrbitalParameters = initialOrbitalParameters ?? throw new ArgumentNullException(nameof(initialOrbitalParameters));
        Frame = frame ?? throw new ArgumentNullException(nameof(frame));
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
    public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow,Models.Body.CelestialBody observer, Frames.Frame frame, Aberration aberration,
        TimeSpan stepSize)
    {
        return _api.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
    }

    public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration)
    {
        return _api.ReadEphemeris(epoch, observer, this, frame, aberration);
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
    public IEnumerable<Window> FindOccultations(in Window searchWindow, INaifObject by, ShapeType byShape, INaifObject target, ShapeType targetShape,
        OccultationType occultationType, Aberration aberration, in TimeSpan coarseStepSize)
    {
        return _api.FindWindowsOnOccultationConstraint(searchWindow, this, target, targetShape, by, byShape, occultationType, aberration, coarseStepSize);
    }

    public double AngularSize(double distance)
    {
        return (this is CelestialBody body)
            ? 2.0 * System.Math.Asin((body.EquatorialRadius * 2.0) / (distance * 2.0))
            : 0.0;
    }

    //todo implement angular separation

    public override string ToString()
    {
        return Name;
    }

    public abstract double GetTotalMass();

    public override bool Equals(object obj)
    {
        return Equals(obj as Body);
    }

    public bool Equals(Body other)
    {
        return base.Equals(other) ||
               NaifId == other.NaifId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), NaifId);
    }

    public static bool operator ==(Body left, Body right)
    {
        return EqualityComparer<Body>.Default.Equals(left, right);
    }

    public static bool operator !=(Body left, Body right)
    {
        return !(left == right);
    }
}