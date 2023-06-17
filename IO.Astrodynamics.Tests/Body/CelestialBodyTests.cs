using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Body;

public class CelestialBodyTests
{
    public CelestialBodyTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void Create()
    {
        CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
        var ke = new KeplerianElements(1500000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
        CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+14, 6356751.9, 6378136.6, new Frames.Frame("ITRF93"), ke);

        Assert.Equal(399, earth.NaifId);
        Assert.Equal("earth", earth.Name);
        Assert.Equal("ITRF93", earth.Frame.Name);
        Assert.Equal(3.986004418E+14, earth.GM);
        Assert.Equal(5.972168494074286E+24, earth.Mass);
        Assert.Equal(6356751.9, earth.PolarRadius);
        Assert.Equal(6378136.6, earth.EquatorialRadius);
        Assert.Equal(0.0033528131084554157, earth.Flatenning);
        Assert.Equal(1, sun.Satellites.Count);
    }

    [Fact]
    public void CreateFromNaifObject()
    {
        CelestialBody moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);
        throw new NotImplementedException();
    }

    [Fact]
    public void CreateExceptions()
    {
        Assert.Throws<ArgumentException>(() => new CelestialBody(-399, "earth", 3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "", 3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", -3.986004418E+5, 6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", 3.986004418E+5, -6356.7519, 6378.1366));
        Assert.Throws<ArgumentException>(() => new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, -6378.1366));
    }

    [Fact]
    public void FindOccultationsEclipse()
    {
        var moon = TestHelpers.GetMoonAt20011214();
        var earth = moon.InitialOrbitalParameters.CenterOfMotion;
        var sun = earth.InitialOrbitalParameters.CenterOfMotion;
        var res = earth.FindOccultations(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938), DateTimeExtension.CreateTDB(61646464.183445148)), moon,
            ShapeType.Ellipsoid, sun, ShapeType.Ellipsoid, OccultationType.Any, Aberration.None, TimeSpan.FromMinutes(1.0)).ToArray();
        Assert.Single(res);

        Assert.Equal(new DateTime(2001, 12, 14, 20, 10, 50, 573), res[0].StartDate);
        Assert.Equal(new DateTime(2001, 12, 14, 21, 36, 33, 019), res[0].EndDate);
    }

    [Fact]
    public void AngularSize()
    {
        var sun = TestHelpers.GetSun();
        var earth = TestHelpers.GetEarthAtJ2000();
        var res = sun.AngularSize(earth.InitialOrbitalParameters.ToStateVector().Position.Magnitude());
        Assert.Equal(0.009456, res, 6);
    }
}