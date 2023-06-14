using System;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Maneuvers
{
    public class PhasingManeuverTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            spc1.AddFuelTank(fuelTank10, 3000.0, "sn0");
            spc1.AddFuelTank(fuelTank11, 4000.0, "sn1");
            spc1.AddPayload(pl1);
            spc1.AddEngine(eng, fuelTank10, "sn1");


            PhasingManeuver maneuver = new PhasingManeuver(spc1, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 3.0, 2, spc1.Engines.First());

            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(spc1, maneuver.Spacecraft);
            Assert.Equal(3.0, maneuver.TargetTrueLongitude);
            Assert.Equal((uint)2, maneuver.RevolutionNumber);
        }
    }
}