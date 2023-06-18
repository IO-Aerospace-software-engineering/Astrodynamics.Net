using System;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class SpacecraftTests
    {
        private API _api = API.Instance;

        public SpacecraftTests()
        {
            _api.LoadKernels(Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void Create()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.EarthAtJ2000, DateTime.MinValue, Frames.Frame.ICRF));
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
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.EarthAtJ2000, DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentException>(() => new Spacecraft(-1001, "", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.EarthAtJ2000, DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentNullException>(() => new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, null,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.EarthAtJ2000, DateTime.MinValue, Frames.Frame.ICRF)));
        }

        [Fact]
        public void Create2()
        {
            CelestialBody sun = new CelestialBody(Stars.Sun.NaifId);

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);
            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", "sn0", 4000.0, 3000);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", "sn1", 4000.0, 4000.0);
            spc1.AddFuelTank(fuelTank10);
            spc1.AddFuelTank(fuelTank11);
            spc1.AddPayload(pl1);
            Assert.Null(spc1.StandbyManeuver);
            Assert.Equal(ke, spc1.InitialOrbitalParameters);
            Assert.Equal(clk1, spc1.Clock);
        }

        [Fact]
        public void GetOrientation()
        {
            DateTime start = DateTimeExtension.CreateUTC(676555130.80).ToTDB();
            DateTime end = start.AddSeconds(6448.0);

            //Configure scenario
            Scenario scenario = new Scenario("Scenario_A", new IO.Astrodynamics.Mission.Mission("mission01"), new Window(start, end));
            scenario.AddBody(TestHelpers.Sun);
            scenario.AddBody(TestHelpers.EarthAtJ2000);
            scenario.AddBody(TestHelpers.MoonAtJ2000);

            //Define parking orbit

            StateVector parkingOrbit = new StateVector(
                new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000,
                start, Frames.Frame.ICRF);

            //Configure spacecraft
            Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
            Spacecraft spacecraft =
                new Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
            Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
            spacecraft.AddFuelTank(fuelTank);
            spacecraft.AddEngine(engine);
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddInstrument(new Instrument(-178600, "CAM600", "mod1", 1.5, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX));
            scenario.AddBody(spacecraft);

            //Execute scenario
            _api.PropagateScenario(scenario, Constants.OutputPath);
            var orientation = spacecraft.GetOrientation(Frames.Frame.ICRF, start);
            Vector3.VectorY.Rotate(orientation.Rotation);
        }


        [Fact]
        public void AddEngine()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", "sn1", 4000.0, 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0, fuelTank);
            spc.AddFuelTank(fuelTank);
            spc.AddEngine(eng);
            Assert.Equal(eng, spc.Engines.Single());
            Assert.Equal(fuelTank, spc.FuelTanks.Single());
        }

        [Fact]
        public void AddEngineWithUnknowFuelTank()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", "sn1", 4000.0, 4000.0);
            Engine eng = new Engine("My engine", "model 1", "sn1", 350.0, 50.0, fuelTank);
            Assert.Throws<InvalidOperationException>(() => spc.AddEngine(eng));
        }

        [Fact]
        public void AddFuelTank()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            FuelTank fuelTank = new FuelTank("My fuel tank", "ft2021", "sn1", 4000.0, 4000.0);
            new Engine("My engine", "model 1", "sn1", 350.0, 50.0, fuelTank);
            spc.AddFuelTank(fuelTank);
            Assert.Equal(fuelTank, spc.FuelTanks.Single());
        }

        [Fact]
        public void AddPayload()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            Payload pl = new Payload("My payload", 1000.0, "sn0");
            spc.AddPayload(pl);
            Assert.Equal(pl, spc.Payloads.Single());
        }

        [Fact]
        public void AddInstrument()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);

            Instrument instrument = new Instrument(-1001600, "My instrument", "Model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX);
            spc.AddInstrument(instrument);
            Assert.Equal(instrument, spc.Intruments.Single());
        }

        [Fact]
        public void GetTotalMass()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);

            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, ke);
            Payload pl1 = new Payload("pl1", 300, "sn1");

            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", "sn1", 4000.0, 3000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", "sn2", 4000.0, 4000.0);
            spc.AddFuelTank(fuelTank10);
            spc.AddFuelTank(fuelTank11);
            spc.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn0");
            new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "MySpacecraft", 2000.0, 10000.0, clk, ke);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", "sn1", 4000.0, 2000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", "sn2", 4000.0, 3000.0);
            spc2.AddFuelTank(fuelTank20);
            spc2.AddFuelTank(fuelTank21);
            spc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn0");
            Payload pl31 = new Payload("pl31", 150, "sn1");
            new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "MySpacecraft", 3000.0, 10000.0, clk, ke);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", "sn0", 4000.0, 1000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", "sn1", 4000.0, 3000.0);
            spc3.AddFuelTank(fuelTank30);
            spc3.AddFuelTank(fuelTank31);
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
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.Sun, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Payload pl1 = new Payload("pl1", 300, "sn1");
            Spacecraft spc1 = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk1, ke);


            FuelTank fuelTank10 = new FuelTank("My fuel tank10", "ft2021", "sn0", 4000.0, 3000.0);
            FuelTank fuelTank11 = new FuelTank("My fuel tank11", "ft2021", "sn1", 4000.0, 4000.0);
            spc1.AddFuelTank(fuelTank10);
            spc1.AddFuelTank(fuelTank11);
            spc1.AddPayload(pl1);

            Payload pl2 = new Payload("pl2", 400, "sn1");
            Clock clk2 = new Clock("My clock2", 1.0 / 256.0);
            Spacecraft spc2 = new Spacecraft(-1002, "MySpacecraft", 2000.0, 10000.0, clk2, ke);
            FuelTank fuelTank20 = new FuelTank("My fuel tank20", "ft2021", "sn0", 4000.0, 2000.0);
            FuelTank fuelTank21 = new FuelTank("My fuel tank21", "ft2021", "sn1", 4000.0, 3000.0);
            spc2.AddFuelTank(fuelTank20);
            spc2.AddFuelTank(fuelTank21);
            spc2.AddPayload(pl2);

            Payload pl3 = new Payload("pl3", 50, "sn1");
            Payload pl31 = new Payload("pl31", 150, "sn2");
            Clock clk3 = new Clock("My clock3", 1.0 / 256.0);
            Spacecraft spc3 = new Spacecraft(-1003, "MySpacecraft", 3000.0, 10000.0, clk3, ke);
            FuelTank fuelTank30 = new FuelTank("My fuel tank30", "ft2021", "sn0", 4000.0, 1000.0);
            FuelTank fuelTank31 = new FuelTank("My fuel tank31", "ft2021", "sn1", 4000.0, 3000.0);

            spc3.AddFuelTank(fuelTank30);
            spc3.AddFuelTank(fuelTank31);
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