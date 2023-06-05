using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Body
{
    public class InstrumentTests
    {
        [Fact]
        public void Create()
        {
            Instrument instrument = new Instrument("inst", "model", 1.57);
            Assert.Equal("inst", instrument.Name);
            Assert.Equal("model", instrument.Model);
            Assert.Equal(1.57, instrument.FieldOfView);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Instrument("", "model", 1.57));
            Assert.Throws<ArgumentException>(() => new Instrument("inst", "", 1.57));
            Assert.Throws<ArgumentException>(() => new Instrument("inst", "model", 0.0));
        }
    }
}