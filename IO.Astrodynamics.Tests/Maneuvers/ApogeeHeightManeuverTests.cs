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
    public class ApogeeHeightManeuverTests
    {
        public ApogeeHeightManeuverTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }
        [Fact]
        public void Create()
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

            ApogeeHeightManeuver apogeeHeightManeuver = new ApogeeHeightManeuver(spc1, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, spc1.Engines.First());

            Assert.Single(apogeeHeightManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), apogeeHeightManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), apogeeHeightManeuver.MinimumEpoch);
            Assert.Equal(151000000.0, apogeeHeightManeuver.TargetApogee);
        }

        
    }
}