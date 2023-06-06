using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Body
{
    public class ClockTests
    {
        [Fact]
        public void CreateClock()
        {
            Clock clock = new Clock("clk", 1.0 / 256.0);
            Assert.Equal("clk", clock.Name);
            Assert.Equal(1.0 / 256.0, clock.Resolution);
        }

        [Fact]
        public void CreateInvalidClock()
        {
            Assert.Throws<ArgumentException>(() => new Clock("", 1.0 / 256.0));
            Assert.Throws<ArgumentException>(() => new Clock("clk", 0.0));
        }
    }
}