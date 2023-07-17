using System;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;


namespace IO.Astrodynamics.Body;

public class CelestialBody : Body
{
    public double PolarRadius { get; }
    public double EquatorialRadius { get; }
    public double Flattening { get; }

    public double SphereOfInfluence { get; private set; }
    public Frame Frame { get; }

    public CelestialBody(NaifObject naifObject) : this(naifObject.NaifId)
    {
    }

    public CelestialBody(int naifId) : this(naifId, Frame.ECLIPTIC_J2000, DateTimeExtension.J2000)
    {
    }

    public CelestialBody(NaifObject naifObject, Frame frame, DateTime epoch) : this(naifObject.NaifId, frame, epoch)
    {
    }

    public CelestialBody(int naifId, Frame frame, DateTime epoch) : base(naifId, frame, epoch)
    {
        PolarRadius = ExtendedInformation.Radii.Z;
        EquatorialRadius = ExtendedInformation.Radii.X;
        Flattening = (EquatorialRadius - PolarRadius) / EquatorialRadius;
        if (double.IsNaN(Flattening))
        {
            Flattening = 0.0;
        }

        Frame = string.IsNullOrEmpty(ExtendedInformation.FrameName)
            ? throw new InvalidOperationException(
                "Celestial body frame can't be defined, please check if you have loaded associated kernels")
            : new Frame(ExtendedInformation.FrameName);

        UpdateSphereOfInfluence();
    }

    public CelestialBody(int naifId, string name, double mass, double polarRadius = 0.0, double equatorialRadius = 0.0,
        OrbitalParameters.OrbitalParameters initialOrbitalParameters = null) : base(
        naifId, name, mass, initialOrbitalParameters)
    {
        PolarRadius = polarRadius;
        EquatorialRadius = equatorialRadius;
        Flattening = (EquatorialRadius - PolarRadius) / EquatorialRadius;
        if (double.IsNaN(Flattening))
        {
            Flattening = 0.0;
        }

        SphereOfInfluence = double.PositiveInfinity;
    }

    private void UpdateSphereOfInfluence()
    {
        SphereOfInfluence = double.PositiveInfinity;
        if (InitialOrbitalParameters == null) return;
        var mainBody = new CelestialBody(ExtendedInformation.CenterOfMotionId);
        var a = this.GetEphemeris(InitialOrbitalParameters.Epoch, mainBody, Frame.ECLIPTIC_J2000, Aberration.None).SemiMajorAxis();
        SphereOfInfluence = InitialOrbitalParameters != null ? SphereOfInluence(a, Mass, mainBody.Mass) : double.PositiveInfinity;
    }

    private static double SphereOfInluence(double a, double minorMass, double majorMass)
    {
        return a * System.Math.Pow(minorMass / majorMass, 2.0 / 5.0);
    }

    /// <summary>
    /// Compute body radius from geocentric latitude
    /// </summary>
    /// <param name="latitude">Geocentric latitude</param>
    /// <returns></returns>
    public double RadiusFromPlanetocentricLatitude(double latitude)
    {
        double r2 = EquatorialRadius * EquatorialRadius;
        double s2 = System.Math.Sin(latitude) * System.Math.Sin(latitude);
        double f2 = (1 - Flattening) * (1 - Flattening);
        return System.Math.Sqrt(r2 / (1 + (1 / f2 - 1) * s2));
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
}