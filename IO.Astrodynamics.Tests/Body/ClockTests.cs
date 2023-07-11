using System;
using IO.Astrodynamics.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
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
        
        [Fact]
        public void Equality()
        {
            Clock clock = new Clock("clk", 1.0 / 256.0);
            Clock clock2 = new Clock("clk", 1.0 / 128.0);
            Assert.True(clock!=clock2);
            Assert.False(clock==clock2);
            Assert.False(clock.Equals(clock2));
            Assert.False(clock.Equals((object)clock2));
            Assert.False(clock.Equals(null));
            Assert.False(clock.Equals((object)null));
        }
    }
}