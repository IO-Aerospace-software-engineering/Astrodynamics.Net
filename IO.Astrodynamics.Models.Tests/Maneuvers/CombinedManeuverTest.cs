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
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);


            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
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


            CombinedManeuver maneuver = new CombinedManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, 1.0, sc.Engines.First());

            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(sc, maneuver.Spacecraft);
            Assert.Equal(151000000.0, maneuver.TargetPerigeeHeight);
            Assert.Equal(1.0, maneuver.TargetInclination);
        }

        [Fact]
        public void CanExecute()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
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


            CombinedManeuver maneuver = new CombinedManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 151000000.0, 1.0, sc.Engines.First());

            var earlyOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI, sunScn, new DateTime(2020, 12, 31), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(maneuver.CanExecute(earlyOrbit));

            var atTimeOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI, sunScn, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.True(maneuver.CanExecute(atTimeOrbit));

            var overToleranceOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, Constants.PI + 0.1, sunScn, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(maneuver.CanExecute(overToleranceOrbit));
        }

        [Fact]
        public void ExecuteIncreasePerigee()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(24421.0, 0.726546824, 28.5 * Constants.Deg2Rad, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);


            CombinedManeuver combinedManeuver = new CombinedManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 42164.0, 0.0, sc.Engines.First());

            var orbit = new KeplerianElements(24421.0, 0.726546824, 28.5 * Constants.Deg2Rad, 0.0, 0.0, Constants.PI, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(combinedManeuver.CanExecute(orbit));
            combinedManeuver.Execute(orbit);
            Assert.Equal(1.8302347036050164, combinedManeuver.DeltaV.Magnitude());
            Assert.Equal(3.435197646715404E-16, combinedManeuver.DeltaV.X);
            Assert.Equal(-1.661679900856469, combinedManeuver.DeltaV.Y);
            Assert.Equal(0.7671890101987755, combinedManeuver.DeltaV.Z);

            //Check orbital parameters
            Vector3 v = orbit.ToStateVector().Velocity + combinedManeuver.DeltaV;
            StateVector res = new StateVector(orbit.ToStateVector().Position, v, orbit.CenterOfMotion, orbit.Epoch, orbit.Frame);
            Assert.Equal(0.0, res.Inclination());
            Assert.Equal(42164.0, res.PerigeeVector().Magnitude(), 3);
            Assert.Equal(42164.0, res.ApogeeVector().Magnitude(), 3);
            Assert.Equal(0.0, res.Eccentricity(), 9);
            Assert.Equal(0.0, res.AscendingNode(), 9);
            Assert.Equal(Constants._2PI, res.ArgumentOfPeriapsis(), 6);
            Assert.Equal(orbit.MeanAnomaly(), res.MeanAnomaly(), 6);
            Assert.Equal(orbit.Epoch, res.Epoch);
        }
        [Fact]
        public void ExecuteDecreasePerigee()
        {
            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(24421.0, 0.726546824, 28.5 * Constants.Deg2Rad, 0.0, 0.0, 0.0, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
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


            CombinedManeuver combinedManeuver = new CombinedManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 6600.0, 0.0, sc.Engines.First());

            var orbit = new KeplerianElements(24421.0, 0.726546824, 28.5 * Constants.Deg2Rad, 0.0, 0.0, Constants.PI, earthScn, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(combinedManeuver.CanExecute(orbit));
            combinedManeuver.Execute(orbit);
            Assert.Equal(0.7895799359286544, combinedManeuver.DeltaV.Magnitude());
            Assert.Equal(-3.663810144308247E-18, combinedManeuver.DeltaV.X, 12);
            Assert.Equal(-0.18670162787539146, combinedManeuver.DeltaV.Y);
            Assert.Equal(0.7671890101987755, combinedManeuver.DeltaV.Z);

            //Check orbital parameters
            Vector3 v = orbit.ToStateVector().Velocity + combinedManeuver.DeltaV;
            StateVector res = new StateVector(orbit.ToStateVector().Position, v, orbit.CenterOfMotion, orbit.Epoch, orbit.Frame);
            Assert.Equal(0.0, res.Inclination());
            Assert.Equal(6600.0, res.PerigeeVector().Magnitude(), 11);
            Assert.Equal(42164.0, res.ApogeeVector().Magnitude(), 4);
            Assert.Equal(0.7293085062135265, res.Eccentricity());
            Assert.Equal(0.0, res.AscendingNode(), 9);
            Assert.Equal(Constants._2PI, res.ArgumentOfPeriapsis());
            Assert.Equal(orbit.MeanAnomaly(), res.MeanAnomaly());
            Assert.Equal(orbit.Epoch, res.Epoch);
        }

    }
}