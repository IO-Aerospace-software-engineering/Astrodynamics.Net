using System;
using IO.Astrodynamics.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class EngineTests
    {
        [Fact]
        public void Create()
        {
            Engine engine = new Engine("eng", "model", 350.0, 50.0);
            Assert.Equal("eng", engine.Name);
            Assert.Equal("model", engine.Model);
            Assert.Equal(350.0, engine.ISP);
            Assert.Equal(50.0, engine.FuelFlow);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new Engine("", "model", 350.0, 50.0));
            Assert.Throws<ArgumentException>(() => new Engine("eng", "", 350.0, 50.0));
            Assert.Throws<ArgumentException>(() => new Engine("eng", "model", 0.0, 50.0));
            Assert.Throws<ArgumentException>(() => new Engine("eng", "model", 350.0, 0.0));
        }
    }
}