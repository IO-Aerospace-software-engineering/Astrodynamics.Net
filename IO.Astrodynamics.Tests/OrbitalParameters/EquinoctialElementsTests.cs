using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.OrbitalParameters;

public class EquinoctialElementsTests
{
    [Fact]
    public void Create()
    {
        CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
        var epoch = DateTime.UtcNow;
        EquinoctialElements equ = new EquinoctialElements(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, earth, epoch, Frames.Frame.ICRF);
        Assert.Equal(1.0, equ.P);
        Assert.Equal(2.0, equ.F);
        Assert.Equal(3.0, equ.G);
        Assert.Equal(4.0, equ.H);
        Assert.Equal(5.0, equ.K);
        Assert.Equal(6.0, equ.L0);
        Assert.Equal(earth, equ.CenterOfMotion);
        Assert.Equal(epoch, equ.Epoch);
        Assert.Equal(Frames.Frame.ICRF, equ.Frame);
    }

    [Fact]
    public void ToEquinoctial()
    {
        CelestialBody earth = new CelestialBody(PlanetsAndMoons.EARTH.NaifId);
        KeplerianElements ke = new KeplerianElements(6800.81178582, 0.00134, 51.71 *IO.Astrodynamics.Constants.Deg2Rad, 32.57 *IO.Astrodynamics.Constants.Deg2Rad, 105.64 *IO.Astrodynamics.Constants.Deg2Rad, 46.029 *IO.Astrodynamics.Constants.Deg2Rad, earth, DateTime.UtcNow, Frames.Frame.ICRF);
        EquinoctialElements equ = ke.ToEquinoctial();
        Assert.Equal(equ.SemiMajorAxis(), ke.A);
        Assert.Equal(equ.Eccentricity(), ke.E);
        Assert.Equal(equ.Inclination(), ke.I);
        Assert.Equal(equ.AscendingNode() *IO.Astrodynamics.Constants.Rad2Deg, ke.RAAN *IO.Astrodynamics.Constants.Rad2Deg, 12);
        Assert.Equal(equ.ArgumentOfPeriapsis(), ke.AOP, 12);
        Assert.Equal(equ.MeanAnomaly(), ke.M, 6);
    }

}