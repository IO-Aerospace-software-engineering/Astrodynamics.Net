using System;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.OrbitalParameters;

public class EquinoctialElementsTests
{
    [Fact]
    public void Create()
    {
        IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
        Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
        CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
        CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
        var epoch = DateTime.UtcNow;
        EquinoctialElements equ = new EquinoctialElements(1.0, 2.0, 3.0, 4.0, 5.0, 6.0, earthScn, epoch, IO.Astrodynamics.Models.Frame.Frame.ICRF);
        Assert.Equal(1.0, equ.P);
        Assert.Equal(2.0, equ.F);
        Assert.Equal(3.0, equ.G);
        Assert.Equal(4.0, equ.H);
        Assert.Equal(5.0, equ.K);
        Assert.Equal(6.0, equ.L0);
        Assert.Equal(earthScn, equ.CenterOfMotion);
        Assert.Equal(epoch, equ.Epoch);
        Assert.Equal(IO.Astrodynamics.Models.Frame.Frame.ICRF, equ.Frame);
    }

    [Fact]
    public void ToEquinoctial()
    {
        IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
        Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
        CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
        CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
        KeplerianElements ke = new KeplerianElements(6800.81178582, 0.00134, 51.71 * Constants.Deg2Rad, 32.57 * Constants.Deg2Rad, 105.64 * Constants.Deg2Rad, 46.029 * Constants.Deg2Rad, earthScn, DateTime.UtcNow, IO.Astrodynamics.Models.Frame.Frame.ICRF);
        EquinoctialElements equ = ke.ToEquinoctial();
        Assert.Equal(equ.SemiMajorAxis(), ke.A);
        Assert.Equal(equ.Eccentricity(), ke.E);
        Assert.Equal(equ.Inclination(), ke.I);
        Assert.Equal(equ.AscendingNode() * Constants.Rad2Deg, ke.RAAN * Constants.Rad2Deg, 12);
        Assert.Equal(equ.ArgumentOfPeriapsis(), ke.AOP, 12);
        Assert.Equal(equ.MeanAnomaly(), ke.M, 6);
    }

}