using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers;

public class InstrumentPointingAttitudeTests
{
    public InstrumentPointingAttitudeTests()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    void Create()
    {
        FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", "sn0", 4000.0, 3000.0);
        Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0, fuelTank10);
        var instrument=new Instrument(-172600, "CAM602", "mod1", 1.5, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorY, new Vector3(0.0, -System.Math.PI * 0.5, 0.0));
        InstrumentPointingToAttitude attitude =
            new InstrumentPointingToAttitude(DateTime.MinValue, TimeSpan.FromHours(1.0), instrument, TestHelpers.EarthAtJ2000, eng);
        Assert.Equal(DateTime.MinValue, attitude.MinimumEpoch);
        Assert.Equal(TimeSpan.FromHours(1.0), attitude.ManeuverHoldDuration);
        Assert.Single(attitude.Engine);
        Assert.Equal(eng, attitude.Engine.First());
        Assert.Equal(instrument, attitude.Instrument);
        Assert.Equal(TestHelpers.EarthAtJ2000, attitude.Target);
    }
}