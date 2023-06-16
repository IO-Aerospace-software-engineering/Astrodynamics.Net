using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Mission
{
    public class SpacecraftInstrumentTests
    {
        [Fact]
        public void Create()
        {
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.GetSun(), DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "Myspacecraft", 1000.0, 10000.0, clk,ke);
            
            Instrument instrument = new Instrument(-1001600, "My instrument", "Model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            SpacecraftInstrument si = new SpacecraftInstrument(spc, instrument, new Vector3(1.0, 2.0, 3.0));

            Assert.Equal(spc, si.Spacecraft);
            Assert.Equal(instrument, si.Instrument);
            Assert.Equal(new Vector3(1.0, 2.0, 3.0), si.Orientation);
        }
    }
}