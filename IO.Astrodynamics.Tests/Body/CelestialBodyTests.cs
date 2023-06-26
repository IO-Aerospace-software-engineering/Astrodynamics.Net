using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
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
    public void Create2()
    {
        CelestialBody earth = new CelestialBody(399, Frames.Frame.ECLIPTIC, DateTimeExtension.J2000);

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
        Assert.Throws<InvalidOperationException>(() => new CelestialBody(-399));
    }

    [Fact]
    public void FindOccultationsEclipse()
    {
        var moon = TestHelpers.MoonAt20011214;
        var earth = moon.InitialOrbitalParameters.CenterOfMotion;
        var sun = earth.InitialOrbitalParameters.CenterOfMotion;
        var res = sun.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938), DateTimeExtension.CreateTDB(61646464.183445148)), earth,
            ShapeType.Ellipsoid, moon, ShapeType.Ellipsoid, OccultationType.Any, Aberration.None, TimeSpan.FromMinutes(1.0)).ToArray();
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

    [Fact]
    public void GetMass()
    {
        var earth = TestHelpers.EarthAtJ2000;
        var res = earth.GetTotalMass();
        Assert.Equal(5.972168398724899E+24, res);
    }

    [Fact]
    public void CelestialBodyToString()
    {
        var earth = TestHelpers.EarthAtJ2000;
        Assert.Equal("EARTH", earth.ToString());
    }

    [Fact]
    public void Equality()
    {
        var earth1 = new CelestialBody(399);
        var earth2 = new CelestialBody(399);
        var moon = new CelestialBody(301);
        Assert.Equal(earth1, earth2);
        Assert.NotEqual(earth1, moon);
        Assert.False(earth1 == null);
    }

    [Fact]
    public void GetEphemeris()
    {
        var earth = new CelestialBody(399);
        var res = earth.GetEphemeris(new Window(DateTimeExtension.J2000, TimeSpan.FromDays(1.0)), TestHelpers.Sun, Frames.Frame.ICRF, Aberration.None,
            TimeSpan.FromDays(1.0)).ToArray();
        Assert.Equal(2, res.Length);
        Assert.Equal(TestHelpers.EarthAtJ2000.InitialOrbitalParameters, res.First());
        Assert.Equal(
            new StateVector(new Vector3(-29069076368.64741, 132303142494.37561, 57359794320.98976), new Vector3(-29695.854459557304, -5497.347182651619, -2382.9422283991967),
                TestHelpers.Sun, DateTimeExtension.J2000 + TimeSpan.FromDays(1.0), Frames.Frame.ICRF), res.ElementAt(1));
    }
}