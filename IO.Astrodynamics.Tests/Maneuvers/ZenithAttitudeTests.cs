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
        FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
        Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);
        ZenithAttitude zenithAttitude = new ZenithAttitude(DateTime.MinValue, TimeSpan.FromHours(1.0), eng);
        Assert.Equal(DateTime.MinValue,zenithAttitude.MinimumEpoch);
        Assert.Equal(TimeSpan.FromHours(1.0),zenithAttitude.ManeuverHoldDuration);
        Assert.Single(zenithAttitude.Engines);
        Assert.Equal(eng,zenithAttitude.Engines.First());
    }
}