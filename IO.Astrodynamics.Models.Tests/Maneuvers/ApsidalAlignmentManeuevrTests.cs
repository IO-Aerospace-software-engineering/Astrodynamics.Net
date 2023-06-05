using Xunit;
using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Linq;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Tests.Maneuvers
{
    public class ApsidalAlignmentManeuevrTests
    {
        [Fact]
        public void Create()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);

            var targetOrbit = new KeplerianElements(150000000.0, 1.0, 0.0, 0.0, 0.0, 0.0, sunScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);

            ApsidalAlignmentManeuver maneuver = new ApsidalAlignmentManeuver(sc, new DateTime(2021, 1, 1), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.ToArray());
            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(sc, maneuver.Spacecraft);
            Assert.Equal(targetOrbit, maneuver.GetTargetOrbit(new DateTime(2021, 01, 01)));
        }

        [Fact]
        public void CanExecuteAtP()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);

            var targetOrbit = new KeplerianElements(18000.0, 0.5, 0.0, 0.0, 30.0 * Constants.Deg2Rad, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            ApsidalAlignmentManeuver maneuver = new ApsidalAlignmentManeuver(sc, new DateTime(2021, 1, 1), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.ToArray());

            var orbit = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, ke.MeanAnomaly(156.41 * Constants.Deg2Rad), earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(maneuver.CanExecute(orbit));
            Assert.Equal(0.5235987755982987, maneuver.Theta, 9);
        }

        [Fact]
        public void CanExecuteAtQ()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);


            var targetOrbit = new KeplerianElements(18000.0, 0.5, 0.0, 0.0, 30.0 * Constants.Deg2Rad, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            ApsidalAlignmentManeuver maneuver = new ApsidalAlignmentManeuver(sc, new DateTime(2021, 1, 1), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.ToArray());

            var orbit = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, ke.MeanAnomaly(341.77 * Constants.Deg2Rad), earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(maneuver.CanExecute(orbit));
            Assert.Equal(0.5235987755982983, maneuver.Theta);
        }

        [Fact]
        public void ExecuteAtP()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);


            var targetOrbit = new KeplerianElements(18000.0, 0.5, 0.0, 0.0, 30.0 * Constants.Deg2Rad, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            ApsidalAlignmentManeuver maneuver = new ApsidalAlignmentManeuver(sc, new DateTime(2021, 1, 1), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.ToArray());

            var orbit = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, ke.MeanAnomaly(156.41 * Constants.Deg2Rad), earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(maneuver.CanExecute(orbit));
            maneuver.Execute(orbit);

            Assert.Equal(0.5235987755982987, maneuver.Theta, 9);
            Assert.Equal(1.4570605894735735, maneuver.DeltaV.Magnitude(), 9);
            Assert.Equal(-1.3446197892473304, maneuver.DeltaV.X, 9);
            Assert.Equal(0.5612692613724208, maneuver.DeltaV.Y, 9);
            Assert.Equal(0.0, maneuver.DeltaV.Z);

        }

        [Fact]
        public void ExecuteAtQ()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);

            var targetOrbit = new KeplerianElements(18000.0, 0.5, 0.0, 0.0, 30.0 * Constants.Deg2Rad, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            ApsidalAlignmentManeuver maneuver = new ApsidalAlignmentManeuver(sc, new DateTime(2021, 1, 1), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.ToArray());

            var orbit = new KeplerianElements(15000.0, 0.33333, 0.0, 0.0, 0.0, ke.MeanAnomaly(341.77 * Constants.Deg2Rad), earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(maneuver.CanExecute(orbit));
            maneuver.Execute(orbit);

            Assert.Equal(0.5235987755982983, maneuver.Theta);
            Assert.Equal(1.45652255310639, maneuver.DeltaV.Magnitude());
            Assert.Equal(new IO.Astrodynamics.Models.Math.Vector3(-1.3687011626051788, 0.4981115087917445, 0.0), maneuver.DeltaV);
        }
    }
}