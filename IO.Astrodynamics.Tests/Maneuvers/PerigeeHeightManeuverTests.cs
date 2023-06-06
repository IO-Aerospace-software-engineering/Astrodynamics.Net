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
    public class PerigeeHeightManeuverTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);

            CelestialBodyScenario sunScenario = new CelestialBodyScenario(sun, null, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);
            

            PerigeeHeightManeuver perigeeHeightManeuver = new PerigeeHeightManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, sc.Engines.First());

            Assert.Single(perigeeHeightManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), perigeeHeightManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), perigeeHeightManeuver.MinimumEpoch);
            Assert.Equal(sc, perigeeHeightManeuver.Spacecraft);
            Assert.Equal(151000000.0, perigeeHeightManeuver.TargetPerigeeHeight);
        }

        [Fact]
        public void CanExecute()
        {

            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
             CelestialBodyScenario sunScenario = new CelestialBodyScenario(sun, null, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);
            

            PerigeeHeightManeuver perigeeHeightManeuver = new PerigeeHeightManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, sc.Engines.First());


            var earlyOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, Constants.PI, sunScenario, new DateTime(2020, 12, 31), Frames.Frame.ECLIPTIC);
            Assert.False(perigeeHeightManeuver.CanExecute(earlyOrbit));

            var circularOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI, sunScenario, new DateTime(2021, 01, 02), Frames.Frame.ECLIPTIC);
            Assert.True(perigeeHeightManeuver.CanExecute(circularOrbit));

            var atTimeOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, Constants.PI, sunScenario, new DateTime(2021, 01, 02), Frames.Frame.ECLIPTIC);
            Assert.True(perigeeHeightManeuver.CanExecute(atTimeOrbit));

            var overToleranceOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, Constants.PI + 0.1, sunScenario, new DateTime(2021, 01, 02), Frames.Frame.ECLIPTIC);
            Assert.False(perigeeHeightManeuver.CanExecute(overToleranceOrbit));
        }

        [Fact]
        public void Execute()
        {

            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(6678.0, 0.0, 0.0, 0.0, 0.0, 0.0, earthScenario, DateTime.UtcNow, Frames.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);
            

            PerigeeHeightManeuver apogeeHeightManeuver = new PerigeeHeightManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 42164.0, sc.Engines.First());

            var orbit = new KeplerianElements(6678.0, 0.0, 0.0, 0.0, 0.0, Constants.PI, earthScenario, new DateTime(2021, 01, 01), Frames.Frame.ICRF);
            Assert.True(apogeeHeightManeuver.CanExecute(orbit));
            apogeeHeightManeuver.Execute(orbit);
            Assert.Equal(2.42576902830686, apogeeHeightManeuver.DeltaV.Magnitude());
            Assert.Equal(0.0, apogeeHeightManeuver.DeltaV.X, 12);
            Assert.Equal(-2.42576902830686, apogeeHeightManeuver.DeltaV.Y);
            Assert.Equal(0.0, apogeeHeightManeuver.DeltaV.Z);
        }
    }
}