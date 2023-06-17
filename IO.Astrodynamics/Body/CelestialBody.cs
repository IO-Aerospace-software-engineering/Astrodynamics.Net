using System;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Time;


namespace IO.Astrodynamics.Body;

public class CelestialBody : Body
{
    public double PolarRadius { get; }
    public double EquatorialRadius { get; }
    public double Flatenning { get; }
    public double GM { get; }
    public double SphereOfInfluence { get; private set; }

    public CelestialBody(int naifId) : base(naifId)
    {
        var info = API.Instance.GetCelestialBodyInfo(naifId);
        Name = info.Name;
        Frame = new Frame(info.FrameName);
        GM = info.GM;
        PolarRadius = info.Radii.Z;
        EquatorialRadius = info.Radii.X;
        Flatenning = (EquatorialRadius - PolarRadius) / EquatorialRadius;
        Mass = GM / Constants.G;
        if (double.IsNaN(Flatenning))
        {
            Flatenning = double.PositiveInfinity;
        }

        if (NaifId != 10)
        {
            InitialOrbitalParameters = this.GetEphemeris(DateTimeExtension.J2000, new CelestialBody(info.CenterOfMotionId), Frame.ECLIPTIC, Aberration.None);

            if (InitialOrbitalParameters != null)
            {
                InitialOrbitalParameters.CenterOfMotion._satellites.Add(this);
            }
        }

        SphereOfInfluence = InitialOrbitalParameters != null
            ? SphereOfInluence(InitialOrbitalParameters.SemiMajorAxis(), Mass,
                InitialOrbitalParameters.CenterOfMotion.Mass)
            : double.PositiveInfinity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="GM">Km3/s2</param>
    /// <param name="polarRadius"></param>
    /// <param name="equatorialRadius"></param>
    /// <returns></returns>
    public CelestialBody(int naifId, string name, double GM, double polarRadius, double equatorialRadius) : this(naifId,
        name, GM, polarRadius, equatorialRadius, new Frame(naifId == 399 ? "ITRF93" : "IAU_" + name.ToUpper()), null)
    {
    }

    public CelestialBody(int naifId, string name, double GM, double polarRadius, double equatorialRadius, Frame frame, OrbitalParameters.OrbitalParameters initialOrbitalParameters)
        : base(naifId, name, GM / Constants.G, initialOrbitalParameters, frame)
    {
        if (naifId < 0)
        {
            throw new ArgumentException("Naif Id must be a positive number");
        }

        if (GM < 0.0)
        {
            throw new ArgumentException("Invalid GM");
        }

        if (polarRadius < 0.0)
        {
            throw new ArgumentException("Invalid polar Radius");
        }

        if (equatorialRadius < 0.0)
        {
            throw new ArgumentException("Invalid equatorial Radius");
        }

        PolarRadius = polarRadius;
        EquatorialRadius = equatorialRadius;
        Flatenning = (equatorialRadius - polarRadius) / equatorialRadius;
        this.GM = GM;
        if (double.IsNaN(Flatenning))
        {
            Flatenning = double.PositiveInfinity;
        }

        SphereOfInfluence = initialOrbitalParameters != null
            ? SphereOfInluence(initialOrbitalParameters.SemiMajorAxis(), Mass,
                initialOrbitalParameters.CenterOfMotion.Mass)
            : double.PositiveInfinity;
    }

    private double SphereOfInluence(double a, double minorMass, double majorMass)
    {
        return a * System.Math.Pow(minorMass / majorMass, 2.0 / 5.0);
    }

    public override void SetInitialOrbitalParameters(OrbitalParameters.OrbitalParameters orbitalParameters)
    {
        if (orbitalParameters == null) throw new ArgumentNullException(nameof(orbitalParameters));
        base.SetInitialOrbitalParameters(orbitalParameters);
        SphereOfInfluence = SphereOfInluence(orbitalParameters.SemiMajorAxis(), Mass,
            orbitalParameters.CenterOfMotion.Mass);
    }

    public override double GetTotalMass()
    {
        return Mass;
    }
}