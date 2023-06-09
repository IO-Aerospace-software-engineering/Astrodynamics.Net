using System;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class SpacecraftScenarioTests
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
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            Assert.Equal(spc1, sc.PhysicalBody);
            Assert.Null(sc.StandbyManeuver);
            Assert.Equal(ke, sc.InitialOrbitalParameters);
            Assert.Equal(clk1, sc.Clock);
        }

        [Fact]
        public void AddStateVector()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScenario = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScenario, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            var e = DateTime.UtcNow;
            var pos = new Vector3(1.0, 2.0, 3.0);
            var vel = new Vector3(4.0, 5.0, 6.0);
            sc.AddStateVector(new StateVector(pos, vel, sunScenario, e, Frames.Frame.ECLIPTIC));
            Assert.Single(sc.Trajectory);
            Assert.Equal(e, sc.Trajectory[e].Epoch);
            Assert.Equal(pos, sc.Trajectory[e].Position);
            Assert.Equal(vel, sc.Trajectory[e].Velocity);
            Assert.Equal(sunScenario, sc.Trajectory[e].CenterOfMotion);
            Assert.Equal(Frames.Frame.ECLIPTIC, sc.Trajectory[e].Frame);
        }

        [Fact]
        public void AddOrientation()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            var e = DateTime.UtcNow;
            //Todo tests
        }

        [Fact]
        public void GetOrientation()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, Frames.Frame.ICRF);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            var e = new DateTime(2000, 1, 1);

            //Todo tests
            Quaternion q = new Quaternion(1.0, 2.0, 3.0, 4.0);

            Quaternion q2 = new Quaternion(5.0, 6.0, 7.0, 8.0);

            //BEFORE
            var orientation = sc.GetOrientationFromICRF(e.AddSeconds(-10.0));
            Assert.Equal(e.AddSeconds(-10.0), orientation.Epoch);
            Assert.Equal(q, orientation.Rotation);
            Assert.Equal(Frames.Frame.ICRF, orientation.ReferenceFrame);

            //AT EPOCH
            orientation = sc.GetOrientationFromICRF(e);
            Assert.Equal(e, orientation.Epoch);
            Assert.Equal(q, orientation.Rotation);
            Assert.Equal(Frames.Frame.ICRF, orientation.ReferenceFrame);

            //AFTER
            orientation = sc.GetOrientationFromICRF(e.AddSeconds(10.0));
            Assert.Equal(e.AddSeconds(10.0), orientation.Epoch);
            Assert.Equal(q2, orientation.Rotation);
            Assert.Equal(Frames.Frame.ICRF, orientation.ReferenceFrame);
        }

        [Fact]
        public void GetEphemeris()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);

            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);
            var start = DateTime.UtcNow;

            for (int i = 0; i < 10; i++)
            {
                var pos = new Vector3(i * 3.0 + 1.0, i * 3.0 + 2.0, i * 3.0 + 3.0);
                var vel = new Vector3(i * 3.0 + 4.0, i * 3.0 + 5.0, i * 3.0 + 6.0);
                sc.AddStateVector(new StateVector(pos, vel, sunScn, start.AddSeconds(i), Frames.Frame.ECLIPTIC));
            }


            var eph = sc.GetEphemeris(start);
            Assert.Equal(start, eph.Epoch);
            Assert.Equal(new Vector3(1.0, 2.0, 3.0), eph.Position);
            Assert.Equal(new Vector3(4.0, 5.0, 6.0), eph.Velocity);
            Assert.Equal(sunScn, eph.CenterOfMotion);
            Assert.Equal(Frames.Frame.ECLIPTIC, eph.Frame);

            eph = sc.GetEphemeris(start.AddSeconds(3.0));
            Assert.Equal(start.AddSeconds(3.0), eph.Epoch);
            Assert.Equal(new Vector3(10.0, 11.0, 12.0), eph.Position);
            Assert.Equal(new Vector3(13.0, 14.0, 15.0), eph.Velocity);
            Assert.Equal(sunScn, eph.CenterOfMotion);
            Assert.Equal(Frames.Frame.ECLIPTIC, eph.Frame);
        }

        [Fact]
        public void AddEngine()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            sc.AddFuelTank(fuelTank, 4000.0, "sn0");
            sc.AddEngine(eng, fuelTank, "sn0");
            Assert.Equal(eng, sc.Engines.Single().Engine);
            Assert.Equal(fuelTank, sc.FuelTanks.Single().FuelTank);
        }

        [Fact]
        public void AddEngineWithUnknowFuelTank()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            Assert.Throws<InvalidOperationException>(() => sc.AddEngine(eng, fuelTank, "sn"));
        }

        [Fact]
        public void AddFuelTank()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            sc.AddFuelTank(fuelTank, 4000.0, "sn0");
            Assert.Equal(fuelTank, sc.FuelTanks.Single().FuelTank);
        }

        [Fact]
        public void AddPayload()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            Payload pl = new Payload("My payload", 1000.0, "sn0");
            sc.AddPayload(pl);
            Assert.Equal(pl, sc.Payloads.Single());
        }

        [Fact]
        public void AddInstrument()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            Instrument instrument = new Instrument(600, "My instrument", "Model", 1.57, InstrumentShape.Circular);
            sc.AddInstrument(instrument, new Quaternion(0.0, 0.0, 0.0, 0.0));
            Assert.Equal(instrument, sc.Intruments.Single().Instrument);
        }

        [Fact]
        public void GetTotalMass()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn0");

            Clock clk2 = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "My spacecraft", 2000.0, 10000.0);
            SpacecraftScenario sc2 = new SpacecraftScenario(spc2, clk, ke, scenario);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", 4000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", 4000.0);
            sc2.AddFuelTank(fuelTank20, 2000.0, "sn0");
            sc2.AddFuelTank(fuelTank21, 3000.0, "sn1");
            sc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn0");
            Payload pl31 = new Payload("pl31", 150, "sn1");
            Clock clk3 = new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "My spacecraft", 3000.0, 10000.0);
            SpacecraftScenario sc3 = new SpacecraftScenario(spc3, clk, ke, scenario);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", 4000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", 4000.0);
            sc3.AddFuelTank(fuelTank30, 1000.0, "sn0");
            sc3.AddFuelTank(fuelTank31, 3000.0, "sn1");
            sc3.AddPayload(pl3);
            sc3.AddPayload(pl31);

            sc3.SetChild(sc2);
            sc2.SetChild(sc);

            double mass3 = sc3.GetTotalMass();
            double mass2 = sc2.GetTotalMass();
            double mass1 = sc.GetTotalMass();

            Assert.Equal(22900.0, mass3);
            Assert.Equal(15700.0, mass2);
            Assert.Equal(8300.0, mass1);
        }

        [Fact]
        public void Dependancies()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, ke, scenario);

            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            sc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            sc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            sc.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn1");
            Clock clk2 = new Clock("My clock2", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "My spacecraft", 2000.0, 10000.0);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", 4000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", 4000.0);
            SpacecraftScenario sc2 = new SpacecraftScenario(spc2, clk2, ke, scenario);
            sc2.AddFuelTank(fuelTank20, 2000.0, "sn0");
            sc2.AddFuelTank(fuelTank21, 3000.0, "sn1");
            sc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn1");
            Payload pl31 = new Payload("pl31", 150, "sn2");
            Clock clk3 = new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "My spacecraft", 3000.0, 10000.0);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", 4000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", 4000.0);
            SpacecraftScenario sc3 = new SpacecraftScenario(spc3, clk3, ke, scenario);

            sc3.AddFuelTank(fuelTank30, 1000.0, "sn0");
            sc3.AddFuelTank(fuelTank31, 3000.0, "sn1");
            sc3.AddPayload(pl3);
            sc3.AddPayload(pl31);

            sc3.SetChild(sc2);
            sc2.SetChild(sc);

            Assert.Null(sc3.Parent);
            Assert.Equal(sc2, sc3.Child);
            Assert.Equal(sc3, sc2.Parent);
            Assert.Equal(sc, sc2.Child);
            Assert.Equal(sc2, sc.Parent);
            Assert.Null(sc.Child);
        }
    }
}