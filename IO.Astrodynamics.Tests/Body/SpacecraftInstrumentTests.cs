using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
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

            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "Myspacecraft", 1000.0, 10000.0, clk,ke);
            
            Instrument instrument = new Instrument(600, "My instrument", "Model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            SpacecraftInstrument si = new SpacecraftInstrument(spc, instrument, new Vector3(1.0, 2.0, 3.0));

            Assert.Equal(spc, si.Spacecraft);
            Assert.Equal(instrument, si.Instrument);
            Assert.Equal(new Vector3(1.0, 2.0, 3.0), si.Orientation);
        }
    }
}