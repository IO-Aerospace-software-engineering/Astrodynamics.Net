using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers;

public class RetrogradeAttitudeTests
{
    public RetrogradeAttitudeTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }
    [Fact]
    void Create()
    {
        FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
        Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);
        RetrogradeAttitude attitude = new RetrogradeAttitude(DateTime.MinValue, TimeSpan.FromHours(1.0), eng);
        Assert.Equal(DateTime.MinValue,attitude.MinimumEpoch);
        Assert.Equal(TimeSpan.FromHours(1.0),attitude.ManeuverHoldDuration);
        Assert.Single(attitude.Engines);
        Assert.Equal(eng,attitude.Engines.First());
    }
}