using System;
using System.Linq;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Body
{
    public class SpacecraftTests
    {
        [Fact]
        public void Create()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF));
            Assert.Equal(-1001, spc.NaifId);
            Assert.Equal("MySpacecraft", spc.Name);
            Assert.Equal(1000.0, spc.DryOperatingMass);
            Assert.Equal(10000.0, spc.MaximumOperatingMass);
        }

        [Fact]
        public void CreateExceptions()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Spacecraft(1001, "MySpacecraft", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentException>(() => new Spacecraft(-1001, "", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentNullException>(() => new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, null,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
        }

        [Fact]
        public void Create2()
        {
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            spc1.AddFuelTank(fuelTank10, 3000.0, "sn0");
            spc1.AddFuelTank(fuelTank11, 4000.0, "sn1");
            spc1.AddPayload(pl1);
            Assert.Null(spc1.StandbyManeuver);
            Assert.Equal(ke, spc1.InitialOrbitalParameters);
            Assert.Equal(clk1, spc1.Clock);
        }

        [Fact]
        public void GetOrientation()
        {
            //Initialize API
            API api = new API();

            //Load solar system kernels
            api.LoadKernels(Astrodynamics.Tests.Constants.SolarSystemKernelPath);

            DateTime start = DateTimeExtension.CreateUTC(676555130.80).ToTDB();
            DateTime end = start.AddSeconds(6448.0);

            //Configure scenario
            Scenario scenario = new Scenario("Scenario_A", new Models.Mission.Mission("mission01"), new Window(start, end));
            scenario.AddBody(TestHelpers.GetSun());
            scenario.AddBody(TestHelpers.GetEarthAtJ2000());
            scenario.AddBody(TestHelpers.GetMoonAtJ2000());

            //Define parking orbit

            StateVector parkingOrbit = new StateVector(
                new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.GetEarthAtJ2000(),
                start, Frames.Frame.ICRF);

            //Configure spacecraft
            Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
            Spacecraft spacecraft =
                new Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", 9000.0);
            Engine engine = new Engine("engine1", "model1", 450.0, 50.0);
            spacecraft.AddFuelTank(fuelTank, 9000.0, "fuelTank1");
            spacecraft.AddEngine(engine, fuelTank, "serialNumber1");
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddInstrument(new Instrument(600, "CAM600", "mod1", 1.5,
                InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX), Vector3.VectorX);
            scenario.AddBody(spacecraft);

            //Execute scenario
            api.PropagateScenario(scenario, Astrodynamics.Tests.Constants.OutputPath);
            var orientation = spacecraft.GetOrientation(Frames.Frame.ICRF, start);
            var res = Vector3.VectorY.Rotate(orientation.Rotation);
        }


        [Fact]
        public void AddEngine()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            spc.AddFuelTank(fuelTank, 4000.0, "sn0");
            spc.AddEngine(eng, fuelTank, "sn0");
            Assert.Equal(eng, spc.Engines.Single().Engine);
            Assert.Equal(fuelTank, spc.FuelTanks.Single().FuelTank);
        }

        [Fact]
        public void AddEngineWithUnknowFuelTank()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            Assert.Throws<InvalidOperationException>(() => spc.AddEngine(eng, fuelTank, "sn"));
        }

        [Fact]
        public void AddFuelTank()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", 4000.0);
            Engine eng = new Engine("My engine", "model 1", 350.0, 50.0);
            spc.AddFuelTank(fuelTank, 4000.0, "sn0");
            Assert.Equal(fuelTank, spc.FuelTanks.Single().FuelTank);
        }

        [Fact]
        public void AddPayload()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            Payload pl = new Payload("My payload", 1000.0, "sn0");
            spc.AddPayload(pl);
            Assert.Equal(pl, spc.Payloads.Single());
        }

        [Fact]
        public void AddInstrument()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            Instrument instrument = new Instrument(600, "My instrument", "Model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            spc.AddInstrument(instrument, Vector3.VectorX);
            Assert.Equal(instrument, spc.Intruments.Single().Instrument);
        }

        [Fact]
        public void GetTotalMass()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);

            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);
            Payload pl1 = new Payload("pl1", 300, "sn1");

            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            spc.AddFuelTank(fuelTank10, 3000.0, "sn0");
            spc.AddFuelTank(fuelTank11, 4000.0, "sn1");
            spc.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn0");
            Clock clk2 = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "MySpacecraft", 2000.0, 10000.0, clk, ke);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", 4000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", 4000.0);
            spc2.AddFuelTank(fuelTank20, 2000.0, "sn0");
            spc2.AddFuelTank(fuelTank21, 3000.0, "sn1");
            spc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn0");
            Payload pl31 = new Payload("pl31", 150, "sn1");
            Clock clk3 = new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "MySpacecraft", 3000.0, 10000.0, clk, ke);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", 4000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", 4000.0);
            spc3.AddFuelTank(fuelTank30, 1000.0, "sn0");
            spc3.AddFuelTank(fuelTank31, 3000.0, "sn1");
            spc3.AddPayload(pl3);
            spc3.AddPayload(pl31);

            spc3.SetChild(spc2);
            spc2.SetChild(spc);

            double mass3 = spc3.GetTotalMass();
            double mass2 = spc2.GetTotalMass();
            double mass1 = spc.GetTotalMass();

            Assert.Equal(22900.0, mass3);
            Assert.Equal(15700.0, mass2);
            Assert.Equal(8300.0, mass1);
        }

        [Fact]
        public void Dependancies()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);


            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", 4000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", 4000.0);
            spc1.AddFuelTank(fuelTank10, 3000.0, "sn0");
            spc1.AddFuelTank(fuelTank11, 4000.0, "sn1");
            spc1.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn1");
            Clock clk2 = new Clock("My clock2", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "MySpacecraft", 2000.0, 10000.0, clk2, ke);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", 4000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", 4000.0);
            spc2.AddFuelTank(fuelTank20, 2000.0, "sn0");
            spc2.AddFuelTank(fuelTank21, 3000.0, "sn1");
            spc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn1");
            Payload pl31 = new Payload("pl31", 150, "sn2");
            Clock clk3 = new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "MySpacecraft", 3000.0, 10000.0, clk3, ke);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", 4000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", 4000.0);

            spc3.AddFuelTank(fuelTank30, 1000.0, "sn0");
            spc3.AddFuelTank(fuelTank31, 3000.0, "sn1");
            spc3.AddPayload(pl3);
            spc3.AddPayload(pl31);

            spc3.SetChild(spc2);
            spc2.SetChild(spc1);

            Assert.Null(spc3.Parent);
            Assert.Equal(spc2, spc3.Child);
            Assert.Equal(spc3, spc2.Parent);
            Assert.Equal(spc1, spc2.Child);
            Assert.Equal(spc2, spc1.Parent);
            Assert.Null(spc1.Child);
        }
    }
}