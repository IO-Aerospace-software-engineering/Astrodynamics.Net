using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers;

public class ProgradeAttitudeTests
{
    public ProgradeAttitudeTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }
    [Fact]
    void Create()
    {
        FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
        Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);
        ProgradeAttitude attitude = new ProgradeAttitude(DateTime.MinValue, TimeSpan.FromHours(1.0), eng);
        Assert.Equal(DateTime.MinValue,attitude.MinimumEpoch);
        Assert.Equal(TimeSpan.FromHours(1.0),attitude.ManeuverHoldDuration);
        Assert.Single(attitude.Engine);
        Assert.Equal(eng,attitude.Engine.First());
    }
}