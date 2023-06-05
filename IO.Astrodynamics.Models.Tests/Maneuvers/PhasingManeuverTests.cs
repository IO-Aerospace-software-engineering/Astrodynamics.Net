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
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);
            

            PhasingManeuver maneuver = new PhasingManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 3.0, 2, sc.Engines.First());

            Assert.Single(maneuver.Engines);
            Assert.Equal(TimeSpan.FromDays(1.0), maneuver.ManeuverHoldDuration);
            Assert.Equal(new DateTime(2021, 01, 01), maneuver.MinimumEpoch);
            Assert.Equal(sc, maneuver.Spacecraft);
            Assert.Equal(3.0, maneuver.TargetTrueLongitude);
            Assert.Equal((uint)2, maneuver.RevolutionNumber);
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
            Engine eng = new Engine("My engine", "model 1","sn1", 350.0, 50.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0);
            sc.AddFuelTank(fuelTank11, 4000.0);
            sc.AddPayload(pl1);
            sc.AddEngine(eng, fuelTank10);
            

            PhasingManeuver maneuver = new PhasingManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 3.0, 2, sc.Engines.First());

            var earlyOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2020, 12, 31), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(maneuver.CanExecute(earlyOrbit));

            var circularOrbit = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 4.5, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.True(maneuver.CanExecute(circularOrbit));

            var atTimeOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, 0.0, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.True(maneuver.CanExecute(atTimeOrbit));

            var overToleranceOrbit = new KeplerianElements(150000000.0, 0.1, 0.0, 0.0, 0.0, 0.1, sunScenario, new DateTime(2021, 01, 02), IO.Astrodynamics.Models.Frame.Frame.ECLIPTIC);
            Assert.False(maneuver.CanExecute(overToleranceOrbit));
        }

        [Fact]
        public void Execute()
        {

            IO.Astrodynamics.Models.Mission.Mission mission = new IO.Astrodynamics.Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            IO.Astrodynamics.Models.Body.CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScenario = new CelestialBodyScenario(earth, scenario);

            var ke = new KeplerianElements(42164.0, 0.0, 0.0, 0.0, 0.0, 0.0, earthScenario, DateTime.UtcNow, IO.Astrodynamics.Models.Frame.Frame.ICRF);
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
            

            PhasingManeuver maneuver = new PhasingManeuver(sc, new DateTime(2021, 01, 01), TimeSpan.FromDays(1.0), 345.0 * Constants.Deg2Rad, 3, sc.Engines.First());


            var orbit = new KeplerianElements(42164.0, 0.0, 0.0, 0.0, 0.0, 0.0, earthScenario, new DateTime(2021, 01, 01), IO.Astrodynamics.Models.Frame.Frame.ICRF);
            Assert.True(maneuver.CanExecute(orbit));
            maneuver.Execute(orbit);
            Assert.Equal(0.014039767904946654, maneuver.DeltaV.Magnitude());
            Assert.Equal(0.0, maneuver.DeltaV.X);
            Assert.Equal(0.014039767904946654, maneuver.DeltaV.Y);
            Assert.Equal(0.0, maneuver.DeltaV.Z);

            StateVector res = new StateVector(orbit.ToStateVector().Position, orbit.ToStateVector().Velocity + maneuver.DeltaV, orbit.CenterOfMotion, orbit.Epoch, orbit.Frame);
            var period = res.Period();
            var positionAtEnd = res.AtEpoch(orbit.Epoch + period * 3.0);
            Assert.Equal(42553.509220272856, positionAtEnd.SemiMajorAxis());
            Assert.Equal(0.009153398330948859, positionAtEnd.Eccentricity());
            Assert.Equal(0.0, positionAtEnd.Inclination());
            Assert.Equal(0.0, positionAtEnd.AscendingNode());
            Assert.Equal(0.0, positionAtEnd.ArgumentOfPeriapsis());
            Assert.Equal(6.283185307178145, positionAtEnd.TrueAnomaly());

            //Check 15° gap behind original orbit
            Assert.Equal(15.0, positionAtEnd.ToStateVector().Position.Angle(orbit.AtEpoch(positionAtEnd.Epoch).ToStateVector().Position) * Constants.Rad2Deg, 8);
        }
    }
}