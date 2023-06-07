using System;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Maneuvers
{
    public class CombinedManeuverTest
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);


            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario, Astrodynamics.Tests.Constants.SpacecraftPath);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10, "sn");


            CombinedManeuver maneuver = new CombinedManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, 1.0, sc.Engines.First());

            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(sc, maneuver.Spacecraft);
            Assert.Equal(151000000.0, maneuver.TargetPerigeeHeight);
            Assert.Equal(1.0, maneuver.TargetInclination);
        }
    }
}