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

    public CelestialBody(int naifId) : this(naifId, Frame.ECLIPTIC, DateTimeExtension.J2000)
    {
    }

    public CelestialBody(int naifId, Frame frame, DateTime epoch) : base(naifId, frame, epoch)
    {
        GM = ExtendedInformation.GM;
        PolarRadius = ExtendedInformation.Radii.Z;
        EquatorialRadius = ExtendedInformation.Radii.X;
        Flatenning = (EquatorialRadius - PolarRadius) / EquatorialRadius;
        if (double.IsNaN(Flatenning))
        {
            Flatenning = double.PositiveInfinity;
        }

        UpdateSphereOfInfluence();
    }

    private void UpdateSphereOfInfluence()
    {
        SphereOfInfluence = InitialOrbitalParameters != null
            ? SphereOfInluence(InitialOrbitalParameters.SemiMajorAxis(), Mass,
                InitialOrbitalParameters.CenterOfMotion.Mass)
            : double.PositiveInfinity;
    }

    private double SphereOfInluence(double a, double minorMass, double majorMass)
    {
        return a * System.Math.Pow(minorMass / majorMass, 2.0 / 5.0);
    }

    public override double GetTotalMass()
    {
        return Mass;
    }

    /// <summary>
    /// Compute body radius from geocentric latitude
    /// </summary>
    /// <param name="latitude">Geocentric latitude</param>
    /// <returns></returns>
    public double RadiusFromGeocentricLatitude(double latitude)
    {
        double r2 = EquatorialRadius * EquatorialRadius;
        double s2 = System.Math.Sin(latitude) * System.Math.Sin(latitude);
        double f2 = (1 - Flatenning) * (1 - Flatenning);
        return System.Math.Sqrt(r2 / (1 + (1 / f2 - 1) * s2));
    }
}