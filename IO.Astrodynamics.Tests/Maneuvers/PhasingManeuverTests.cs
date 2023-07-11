using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers
{
    public class PhasingManeuverTests
    {
        public PhasingManeuverTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }
        [Fact]
        public void Create()
        {
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);

            PhasingManeuver maneuver = new PhasingManeuver(new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 3.0, 2, eng);

            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(3.0, maneuver.TargetTrueLongitude);
            Assert.Equal((uint)2, maneuver.RevolutionNumber);
        }
    }
}