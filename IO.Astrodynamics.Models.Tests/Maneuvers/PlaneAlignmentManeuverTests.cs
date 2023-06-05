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
    public class PlaneAlignmentManeuverTests
    {
        [Fact]
        public void Create()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScenario = new CelestialBodyScenario(sun, scenario);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
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


            var targetOrbit = new KeplerianElements(150000000.0, 1.0, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);

            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.First());

            Assert.Single(planeAlignmentManeuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), planeAlignmentManeuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), planeAlignmentManeuver.MinimumEpoch);
            Assert.Equal(sc, planeAlignmentManeuver.Spacecraft);
            Assert.Equal(targetOrbit, planeAlignmentManeuver.GetTargetOrbit(new DateTime(2021, 01, 01)));
        }

        [Fact]
        public void CanExecute()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScenario = new CelestialBodyScenario(sun, scenario);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, DateTime.UtcNow, IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
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


            var targetOrbit = new KeplerianElements(150000000.0, 0.0, Constants.PI2 / 2.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.First());

            var earlyOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2020, 12, 31), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(planeAlignmentManeuver.CanExecute(earlyOrbit));

            var atTimeOrbitAscendingNode = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.True(planeAlignmentManeuver.CanExecute(atTimeOrbitAscendingNode));

            var atTimeOrbitDescendingNode = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.True(planeAlignmentManeuver.CanExecute(atTimeOrbitDescendingNode));

            var overToleranceOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI2 + 0.1, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(planeAlignmentManeuver.CanExecute(overToleranceOrbit));
        }

        [Fact]
        public void ExecuteAscendingNode()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(11480.0, 0.0, 60.0 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 0.0, 0.0, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);


            var targetOrbit = new KeplerianElements(11480.0, 0.0, 45.0 * Constants.Deg2Rad, 55.0 * Constants.Deg2Rad, 0.0, 0.0, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.First());

            var orbit = new KeplerianElements(11480.0, 0.0, 60.0 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 0.0, 2.197937654 + Constants.PI, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(planeAlignmentManeuver.CanExecute(orbit));
            planeAlignmentManeuver.Execute(orbit);
            Assert.Equal(true, planeAlignmentManeuver.ExecuteAtAscendingNode);
            Assert.Equal(0.6655681232947839, planeAlignmentManeuver.RelativeInclination.Value, 12);
            Assert.Equal(3.8498574531369796, planeAlignmentManeuver.DeltaV.Magnitude(), 12);
            Assert.Equal(-0.3859820921974442, planeAlignmentManeuver.DeltaV.Normalize().X, 12);
            Assert.Equal(0.6657494548199254, planeAlignmentManeuver.DeltaV.Normalize().Y, 12);
            Assert.Equal(-0.6385886687922483, planeAlignmentManeuver.DeltaV.Normalize().Z, 12);
        }

        [Fact]
        public void ExecuteDescendingNode()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(11480.0, 0.0, 60.0 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 0.0, 0.0, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);


            var targetOrbit = new KeplerianElements(11480.0, 0.0, 45.0 * Constants.Deg2Rad, 55.0 * Constants.Deg2Rad, 0.0, 0.0, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            PlaneAlignmentManeuver planeAlignmentManeuver = new PlaneAlignmentManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), targetOrbit, sc.Engines.First());

            var orbit = new KeplerianElements(11480.0, 0.0, 60.0 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 0.0, 2.197937654, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(planeAlignmentManeuver.CanExecute(orbit));
            planeAlignmentManeuver.Execute(orbit);
            Assert.Equal(true, planeAlignmentManeuver.ExecuteAtDescendingNode);
            Assert.Equal(0.6655681232947839, planeAlignmentManeuver.RelativeInclination.Value, 12);
            Assert.Equal(3.8498574531369796, planeAlignmentManeuver.DeltaV.Magnitude(), 12);
            Assert.Equal(0.38598209219744417, planeAlignmentManeuver.DeltaV.Normalize().X, 12);
            Assert.Equal(-0.6657494548199254, planeAlignmentManeuver.DeltaV.Normalize().Y, 12);
            Assert.Equal(0.6385886687922484, planeAlignmentManeuver.DeltaV.Normalize().Z, 12);
        }


    }
}