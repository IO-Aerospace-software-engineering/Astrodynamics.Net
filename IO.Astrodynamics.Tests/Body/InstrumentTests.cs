using System;
using System.Linq;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;
using FuelTank = IO.Astrodynamics.Body.Spacecraft.FuelTank;
using Instrument = IO.Astrodynamics.Body.Spacecraft.Instrument;
using Payload = IO.Astrodynamics.Body.Spacecraft.Payload;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;

namespace IO.Astrodynamics.Tests.Body
{
    public class InstrumentTests
    {
        public InstrumentTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void Create()
        {
            Instrument instrument = new Instrument(-1600, "inst", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX,
                new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0));
            Assert.Equal("inst", instrument.Name);
            Assert.Equal("model", instrument.Model);
            Assert.Equal(1.57, instrument.FieldOfView);
            Assert.Equal(InstrumentShape.Circular, instrument.Shape);
            Assert.Equal(-1600, instrument.NaifId);
            Assert.Equal(new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0), instrument.Orientation);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() =>
                new Instrument(-1600, "", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0)));
            Assert.Throws<ArgumentException>(() =>
                new Instrument(-1600, "inst", "", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0)));
            Assert.Throws<ArgumentException>(() => new Instrument(-1600, "inst", "model", 0.0, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX,
                new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0)));
        }

        [Fact]
        public void Equality()
        {
            Instrument instrument = new Instrument(-1600, "inst", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX,
                new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0));
            Instrument instrument2 = new Instrument(-1700, "inst", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX,
                new Vector3(0.0, Astrodynamics.Constants.PI2, 0.0));
            Assert.False(instrument == instrument2);
            Assert.True(instrument != instrument2);
            Assert.False(instrument == null);
            Assert.False(instrument.Equals((object)instrument2));
            Assert.True(instrument.Equals((object)instrument));
            Assert.False(instrument.Equals((object)null));
        }

        [Fact]
        public async Task FindWindowInFieldOfView()
        {
            DateTime start = DateTimeExtension.CreateUTC(676555130.80).ToTDB();
            DateTime end = start.AddSeconds(6448.0);

            //Configure scenario
            Scenario scenario = new Scenario("Scenario_A", new Astrodynamics.Mission.Mission("mission06"),
                new Astrodynamics.Time.Window(start, end));
            scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);

            //Define parking orbit
            StateVector parkingOrbit = new StateVector(
                new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000,
                start, Frames.Frame.ICRF);

            //Configure spacecraft
            Clock clock = new Clock("clk1", 65536);
            Spacecraft spacecraft =
                new Spacecraft(-172, "DRAGONFLY2", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
            Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
            spacecraft.AddFuelTank(fuelTank);
            spacecraft.AddEngine(engine);
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddInstrument(new Instrument(-172600, "CAM602", "mod1", 1.5, InstrumentShape.Circular,
                Vector3.VectorZ, Vector3.VectorY,
                new Vector3(0.0, -System.Math.PI * 0.5, 0.0)));
            scenario.AddSpacecraft(spacecraft);

            var root = Constants.OutputPath.CreateSubdirectory(scenario.Mission.Name).CreateSubdirectory(scenario.Name);

            //Execute scenario
            await scenario.SimulateAsync(root, false, false, TimeSpan.FromSeconds(1.0));

            //Find windows when the earth is in field of view of camera 600 
            var res = spacecraft.Intruments.First().FindWindowsInFieldOfViewConstraint(
                new Astrodynamics.Time.Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
                TestHelpers.EarthAtJ2000, TestHelpers.EarthAtJ2000.Frame,
                ShapeType.Ellipsoid, Aberration.LT,
                TimeSpan.FromHours(1.0)).ToArray();

            //Read results
            Assert.Equal(2, res.Count());
            Assert.Equal("2021-06-10T00:00:00.0000000 (TDB)", res.ElementAt(0).StartDate.ToFormattedString());
            Assert.Equal("2021-06-10T00:29:05.3691494 (TDB)", res.ElementAt(0).EndDate.ToFormattedString());
            Assert.Equal("2021-06-10T01:03:45.4719345 (TDB)", res.ElementAt(1).StartDate.ToFormattedString());
            Assert.Equal("2021-06-10T01:47:27.0000000 (TDB)", res.ElementAt(1).EndDate.ToFormattedString());

            Assert.Throws<ArgumentNullException>(() => spacecraft.Intruments.First().FindWindowsInFieldOfViewConstraint(
                new Astrodynamics.Time.Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), null,
                TestHelpers.EarthAtJ2000, TestHelpers.EarthAtJ2000.Frame, ShapeType.Ellipsoid, Aberration.LT, TimeSpan.FromHours(1.0)));
            Assert.Throws<ArgumentNullException>(() => spacecraft.Intruments.First().FindWindowsInFieldOfViewConstraint(
                new Astrodynamics.Time.Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
                null, TestHelpers.EarthAtJ2000.Frame, ShapeType.Ellipsoid, Aberration.LT, TimeSpan.FromHours(1.0)));
            Assert.Throws<ArgumentNullException>(() => spacecraft.Intruments.First().FindWindowsInFieldOfViewConstraint(
                new Astrodynamics.Time.Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
                TestHelpers.EarthAtJ2000, null, ShapeType.Ellipsoid, Aberration.LT, TimeSpan.FromHours(1.0)));
        }
    }
}