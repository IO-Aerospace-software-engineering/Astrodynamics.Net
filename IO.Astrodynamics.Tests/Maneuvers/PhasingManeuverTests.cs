using System;
using System.Linq;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
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
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", "sn0", 4000.0, 3000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0, fuelTank10);

            PhasingManeuver maneuver = new PhasingManeuver(new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 3.0, 2, eng);

            Assert.NotNull(maneuver.Engine);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(3.0, maneuver.TargetTrueLongitude);
            Assert.Equal((uint)2, maneuver.RevolutionNumber);
        }

        [Fact]
        public void CanExecute()
        {
            var orbitalParams = new KeplerianElements(13600000.0, 0.5, 0.0, 0.0, 0.0, 0.0, TestHelpers.EarthAtJ2000, DateTimeExtension.J2000, Frames.Frame.ICRF);
            var targtOrbitalParams = new KeplerianElements(13600000.0, 0.5, 0.0, 0.0, 0.0, 30.0 * Astrodynamics.Constants.Deg2Rad, TestHelpers.EarthAtJ2000,
                DateTimeExtension.J2000, Frames.Frame.ICRF);
            var spc = new Spacecraft(-666, "GenericSpacecraft", 1000.0, 3000.0, new Clock("GenericClk", 65536), orbitalParams);
            spc.AddFuelTank(new FuelTank("ft", "ftA", "123456", 1000.0, 900.0));
            spc.AddEngine(new Engine("eng", "engmk1", "12345", 450, 50, spc.FuelTanks.First()));
            PhasingManeuver maneuver = new PhasingManeuver(DateTime.MinValue, TimeSpan.Zero, targtOrbitalParams, 3, spc.Engines.First());

            //Execute at descending node
            Assert.False(maneuver.CanExecute(orbitalParams.ToStateVector(DateTimeExtension.J2000.AddSeconds(-10))));
            Assert.False(maneuver.CanExecute(orbitalParams.ToStateVector(DateTimeExtension.J2000.AddSeconds(-1))));
            Assert.True(maneuver.CanExecute(orbitalParams.ToStateVector(DateTimeExtension.J2000.AddSeconds(1))));
            Assert.False(maneuver.CanExecute(orbitalParams.ToStateVector(DateTimeExtension.J2000.AddSeconds(2))));
        }
    }
}