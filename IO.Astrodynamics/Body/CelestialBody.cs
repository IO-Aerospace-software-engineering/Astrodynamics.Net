using System;
using System.Numerics;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using Vector3 = IO.Astrodynamics.Math.Vector3;


namespace IO.Astrodynamics.Body;

public class CelestialBody : CelestialItem
{
    public double PolarRadius { get; }
    public double EquatorialRadius { get; }
    public double Flattening { get; }

    public double SphereOfInfluence { get; private set; }
    public Frame Frame { get; }

    /// <summary>
    /// Instantiate celestial body from naif object with default parameters (Ecliptic J2000 at J2000 epoch)
    /// </summary>
    /// <param name="naifObject"></param>
    public CelestialBody(NaifObject naifObject) : this(naifObject.NaifId)
    {
    }

    /// <summary>
    /// Instantiate celestial body from naif id with default parameters (Ecliptic J2000 at J2000 epoch)
    /// </summary>
    /// <param name="naifId"></param>
    public CelestialBody(int naifId) : this(naifId, Frame.ECLIPTIC_J2000, DateTimeExtension.J2000)
    {
    }

    /// <summary>
    /// Instantiate celestial body from naif object with orbital parameters at given frame and epoch
    /// </summary>
    /// <param name="naifObject"></param>
    /// <param name="frame"></param>
    /// <param name="epoch"></param>
    public CelestialBody(NaifObject naifObject, Frame frame, DateTime epoch) : this(naifObject.NaifId, frame, epoch)
    {
    }

    /// <summary>
    /// Instantiate celestial body from naif id with orbital parameters at given frame and epoch
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="frame"></param>
    /// <param name="epoch"></param>
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
                "Celestial celestialItem frame can't be defined, please check if you have loaded associated kernels")
            : new Frame(ExtendedInformation.FrameName);

        UpdateSphereOfInfluence();
    }

    /// <summary>
    /// Instantiate celestial body from custom parameters
    /// </summary>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="mass"></param>
    /// <param name="polarRadius"></param>
    /// <param name="equatorialRadius"></param>
    /// <param name="initialOrbitalParameters"></param>
    protected CelestialBody(int naifId, string name, double mass, double polarRadius = 0.0, double equatorialRadius = 0.0,
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
    /// Compute celestialItem radius from geocentric latitude
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

    public TimeSpan SideralRotationPeriod(DateTime epoch)
    {
        return TimeSpan.FromSeconds(Constants._2PI / GetOrientation(Frame.ICRF, epoch).AngularVelocity.Magnitude());
    }

    public KeplerianElements GeosynchronousOrbit(double longitude, double latitude, DateTime epoch)
    {
        var sideralRotation2 = System.Math.Pow(SideralRotationPeriod(epoch).TotalSeconds, 2);
        var radius = System.Math.Cbrt((GM * sideralRotation2) / (4 * Constants.PI * Constants.PI));
        var bodyfFixedCoordinates = new Planetocentric(longitude, latitude, radius).ToCartesianCoordinates();
        var icrfPos = bodyfFixedCoordinates.Rotate(Frame.ToFrame(Frame.ICRF, epoch).Rotation);
        var icrfRot = Vector3.VectorZ.Rotate(Frame.ToFrame(Frame.ICRF, epoch).Rotation);
        var inertialVelocity = icrfRot.Cross(icrfPos).Normalize() * System.Math.Sqrt(GM / radius);
        var sv = new StateVector(icrfPos, inertialVelocity, this, epoch, Frame.ICRF);
        return new KeplerianElements(radius, 0.0, sv.Inclination(), sv.AscendingNode(), (sv.ArgumentOfPeriapsis() + sv.MeanAnomaly()) % Constants._2PI, 0.0, this, epoch, sv.Frame);
    }
}