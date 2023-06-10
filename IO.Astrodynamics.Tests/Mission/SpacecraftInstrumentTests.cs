using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class SpacecraftInstrumentTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));

            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "Myspacecraft", 1000.0, 10000.0);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, ke, scenario);
            Instrument instrument = new Instrument(600, "My instrument", "Model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            SpacecraftInstrument si = new SpacecraftInstrument(sc, instrument, new Vector3(1.0, 2.0, 3.0));

            Assert.Equal(sc, si.Spacecraft);
            Assert.Equal(instrument, si.Instrument);
            Assert.Equal(new Vector3(1.0, 2.0, 3.0), si.Orientation);
        }

        [Fact]
        public void IsInFieldOfView()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));

            var earth = TestHelpers.GetEarthAtJ2000();
            var epoch = earth.InitialOrbitalParameters.Epoch;
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "Myspacecraft", 1000.0, 10000.0);
            double a = 6800.0;
            double v = System.Math.Sqrt(earth.PhysicalBody.GM / a);
            var sv = new StateVector(new Vector3(a, 0.0, 0.0), new Vector3(0.0, v, 0.0), earth, epoch, Frames.Frame.ICRF);
            SpacecraftScenario sc = new SpacecraftScenario(spc, clk, sv, scenario);
            Instrument instrument = new Instrument(600, "My instrument", "Model", Constants.PI, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            SpacecraftInstrument si = new SpacecraftInstrument(sc, instrument, Vector3.VectorX);

            Assert.True(si.IsInFieldOfView(earth, epoch));
            Assert.True(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.125));
            Assert.True(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.24));
            Assert.False(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.3));
            Assert.False(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.5));
            Assert.False(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.7));
            Assert.True(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.76));
            Assert.True(si.IsInFieldOfView(earth, epoch + sv.Period() * 0.9));
            Assert.Equal(instrument, si.Instrument);
            Assert.Equal(Vector3.VectorX, si.Orientation);
        }
    }
}