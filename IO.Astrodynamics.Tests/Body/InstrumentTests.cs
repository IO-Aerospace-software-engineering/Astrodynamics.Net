using System;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class InstrumentTests
    {
        [Fact]
        public void Create()
        {
            Instrument instrument = new Instrument(-1600, "inst", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX);
            Assert.Equal("inst", instrument.Name);
            Assert.Equal("model", instrument.Model);
            Assert.Equal(1.57, instrument.FieldOfView);
            Assert.Equal(InstrumentShape.Circular, instrument.Shape);
            Assert.Equal(-1600, instrument.NaifId);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Instrument(-1600, "", "model", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX));
            Assert.Throws<ArgumentException>(() => new Instrument(-1600, "inst", "", 1.57, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX));
            Assert.Throws<ArgumentException>(() => new Instrument(-1600, "inst", "model", 0.0, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX));
        }
    }
}