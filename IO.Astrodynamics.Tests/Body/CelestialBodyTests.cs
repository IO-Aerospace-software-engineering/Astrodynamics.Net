using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
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
        CelestialBody earth = new CelestialBody(399);

        Assert.Equal(399, earth.NaifId);
        Assert.Equal("EARTH", earth.Name);
        Assert.Equal("ITRF93", earth.Frame.Name);
        Assert.Equal(3.9860043543609594E+14, earth.GM);
        Assert.Equal(5.972168398724899E+24, earth.Mass);
        Assert.Equal(6356751.9, earth.PolarRadius);
        Assert.Equal(6378136.6, earth.EquatorialRadius);
        Assert.Equal(0.0033528131084554157, earth.Flatenning);
        Assert.Equal(1, earth.InitialOrbitalParameters.CenterOfMotion.Satellites.Count);
    }

    [Fact]
    public void CreateFromNaifObject()
    {
        CelestialBody moon = new CelestialBody(PlanetsAndMoons.MOON.NaifId);
        Assert.Equal("MOON", moon.Name);
        Assert.Equal(301, moon.NaifId);
        Assert.Equal(0.0, moon.Flatenning);
        Assert.Equal(1737400.0, moon.EquatorialRadius);
        Assert.Equal(4902800066163.796, moon.GM);
        Assert.Equal(1737400.0, moon.PolarRadius);
        Assert.Equal(66482231.94758831, moon.SphereOfInfluence);
        Assert.Equal(7.345789170645306E+22, moon.Mass);
        Assert.NotNull(moon.InitialOrbitalParameters);
        Assert.Equal(399, moon.InitialOrbitalParameters.CenterOfMotion.NaifId);
        Assert.Equal(new Vector3(-291608384.6334355, -274979741.16904783, 36271196.6337128), moon.InitialOrbitalParameters.ToStateVector().Position);
        Assert.Equal(new Vector3(643.5313877190328, -730.9839838563024, -11.506464582342058), moon.InitialOrbitalParameters.ToStateVector().Velocity);
        Assert.Equal(Frames.Frame.ECLIPTIC, moon.InitialOrbitalParameters.Frame);
    }

    [Fact]
    public void CreateExceptions()
    {
        Assert.Throws<ArgumentException>(() => new CelestialBody(-399));
    }

    [Fact]
    public void FindOccultationsEclipse()
    {
        var moon = TestHelpers.MoonAt20011214;
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
        var sun = TestHelpers.Sun;
        var earth = TestHelpers.EarthAtJ2000;
        var res = sun.AngularSize(earth.InitialOrbitalParameters.ToStateVector().Position.Magnitude());
        Assert.Equal(0.009459, res, 6);
    }
}