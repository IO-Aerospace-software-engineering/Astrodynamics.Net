using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;
using ZenithAttitude = IO.Astrodynamics.Maneuver.ZenithAttitude;

namespace IO.Astrodynamics.Tests.Maneuvers;

public class ZenithAttitudeTests
{
    public ZenithAttitudeTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }
    [Fact]
    void Create()
    {
        CelestialBody sun = new CelestialBody(Stars.Sun.NaifId);

        var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
        Clock clk1 = new Clock("My clock", 1.0 / 256.0);
        Payload pl1 = new Payload("pl1", 300, "sn1");
        Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0,clk1,ke);
        FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
        FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021","sn1", 4000.0,4000.0);
        Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);
        spc1.AddFuelTank(fuelTank10);
        spc1.AddFuelTank(fuelTank11);
        spc1.AddPayload(pl1);
        spc1.AddEngine(eng);
        ZenithAttitude zenithAttitude = new ZenithAttitude(spc1, DateTime.MinValue, TimeSpan.FromHours(1.0), eng);
        Assert.Equal(DateTime.MinValue,zenithAttitude.MinimumEpoch);
        Assert.Equal(TimeSpan.FromHours(1.0),zenithAttitude.ManeuverHoldDuration);
        Assert.Single(zenithAttitude.Engines);
        Assert.Equal(eng,zenithAttitude.Engines.First());
    }
}