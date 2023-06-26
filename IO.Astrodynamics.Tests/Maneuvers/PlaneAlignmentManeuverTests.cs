using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;

namespace IO.Astrodynamics.Tests.Maneuvers
{
    public class PlaneAlignmentManeuverTests
    {
        public PlaneAlignmentManeuverTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }
        [Fact]
        public void Create()
        {
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021","sn0", 4000.0,3000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0, fuelTank10);


            var targetOrbit = new KeplerianElements(150000000.0, 1.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, new DateTime(2021, 01, 01), Frames.Frame.ECLIPTIC);

            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, eng);

            Assert.Single(planeAlignmentManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), planeAlignmentManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), planeAlignmentManeuver.MinimumEpoch);
            Assert.Equal(targetOrbit, planeAlignmentManeuver.TargetOrbit.AtEpoch(new DateTime(2021, 01, 01)));
            Assert.Equal(eng, planeAlignmentManeuver.Engines.First());
        }
    }
}