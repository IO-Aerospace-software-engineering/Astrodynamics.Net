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
        Assert.Equal(1, (earth.InitialOrbitalParameters.Observer as Astrodynamics.Body.Body)?.Satellites.Count);
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
        Assert.Equal(1, (earth.InitialOrbitalParameters.Observer as Astrodynamics.Body.Body)?.Satellites.Count);
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
        Assert.Equal(399, moon.InitialOrbitalParameters.Observer.NaifId);
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
        var earth = TestHelpers.EarthAtJ2000;
        var sun = TestHelpers.Sun;
        var res = sun.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938), DateTimeExtension.CreateTDB(61646464.183445148)), earth,
            ShapeType.Ellipsoid, moon, ShapeType.Ellipsoid, OccultationType.Any, Aberration.None, TimeSpan.FromMinutes(1.0)).ToArray();
        Assert.Single(res);

        Assert.Equal(new DateTime(2001, 12, 14, 20, 10, 50, 573), res[0].StartDate);
        Assert.Equal(new DateTime(2001, 12, 14, 21, 36, 33, 019), res[0].EndDate);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraint()
    {
        var res = TestHelpers.EarthAtJ2000.FindWindowsOnDistanceConstraint(
            new Window(DateTimeExtension.CreateTDB(220881665.18391809), DateTimeExtension.CreateTDB(228657665.18565452)),
            TestHelpers.MoonAtJ2000, RelationnalOperator.Greater, 400000000, Aberration.None, TimeSpan.FromSeconds(86400.0));
        var windows = res as Window[] ?? res.ToArray();
        Assert.Equal(4, windows.Count());
        Assert.Equal("2007-01-08T00:11:07.6290000 (TDB)", windows.ElementAt(0).StartDate.ToFormattedString());
        Assert.Equal("2007-01-13T06:37:47.9480000 (TDB)", windows.ElementAt(0).EndDate.ToFormattedString());
        Assert.Equal("2007-02-04T07:02:35.2840000 (TDB)", windows.ElementAt(1).StartDate.ToFormattedString());
        Assert.Equal("2007-02-10T09:31:01.8380000 (TDB)", windows.ElementAt(1).EndDate.ToFormattedString());
    }

    [Fact]
    public void FindWindowsOnCoordinateConstraint()
    {
        var res = TestHelpers.EarthAtJ2000.FindWindowsOnCoordinateConstraint(new Window(DateTime.Parse("2005-10-03").ToTDB(), DateTime.Parse("2005-11-03").ToTDB()),
            TestHelpers.MoonAtJ2000, TestHelpers.MoonAtJ2000.Frame, CoordinateSystem.Latitudinal, Coordinate.Latitude, RelationnalOperator.Greater, 0.0, 0.0, Aberration.None,
            TimeSpan.FromSeconds(60.0));

        var windows = res as Window[] ?? res.ToArray();
        Assert.Equal(2, windows.Length);
        Assert.Equal("2005-10-03T17:24:29.0990000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2005-10-16T17:50:20.7050000 (TDB)", windows[0].EndDate.ToFormattedString());
        Assert.Equal("2005-10-31T00:27:02.6710000 (TDB)", windows[1].StartDate.ToFormattedString());
        Assert.Equal("2005-11-03T00:00:00.0000000 (TDB)", windows[1].EndDate.ToFormattedString());
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
        Assert.True(earth1.Equals(earth2));
        Assert.True(earth1.Equals(earth1));
        Assert.True(earth1.Equals((object)earth2));
        Assert.True(earth1.Equals((object)earth1));
        Assert.False(earth1.Equals(null));
        Assert.False(earth1.Equals((object)null));
        Assert.False(earth1.Equals("null"));
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

    [Fact]
    public void AngularSeparation()
    {
        var res = TestHelpers.EarthAtJ2000.AngularSeparation(DateTimeExtension.J2000, TestHelpers.MoonAtJ2000, TestHelpers.Sun, Aberration.None);
        Assert.Equal(0.9984998794278185, res);
    }

    [Fact]
    public void SubObserverPoint()
    {
        var moon = TestHelpers.MoonAtJ2000;
        var res = moon.SubObserverPoint(TestHelpers.EarthAtJ2000, DateTimeExtension.J2000, Aberration.None);
        Assert.Equal(-10.898058558227296, res.Latitude * Constants.RAD_DEG);
        Assert.Equal(-57.74660136367262, res.Longitude * Constants.RAD_DEG);
        Assert.Equal(396071271.39556694, res.Radius);
    }

    [Fact]
    public void RadiusFromLatitude()
    {
        var earth = TestHelpers.EarthAtJ2000;
        var res1 = earth.RadiusFromPlanetocentricLatitude(0.0);
        var res2 = earth.RadiusFromPlanetocentricLatitude(Astrodynamics.Constants.PI2);
        var res3 = earth.RadiusFromPlanetocentricLatitude(-Astrodynamics.Constants.PI2);
        Assert.Equal(earth.EquatorialRadius, res1, 6);
        Assert.Equal(earth.PolarRadius, res2, 6);
        Assert.Equal(earth.PolarRadius, res3, 6);
    }
}