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
            Instrument instrument = new Instrument(600, "inst", "model", 1.57, InstrumentShape.Circular);
            Assert.Equal("inst", instrument.Name);
            Assert.Equal("model", instrument.Model);
            Assert.Equal(1.57, instrument.FieldOfView);
            Assert.Equal(InstrumentShape.Circular, instrument.Shape);
            Assert.Equal(600, instrument.NaifId);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Instrument(600, "", "model", 1.57, InstrumentShape.Circular));
            Assert.Throws<ArgumentException>(() => new Instrument(600, "inst", "", 1.57, InstrumentShape.Circular));
            Assert.Throws<ArgumentException>(() => new Instrument(600, "inst", "model", 0.0, InstrumentShape.Circular));
        }
    }
}