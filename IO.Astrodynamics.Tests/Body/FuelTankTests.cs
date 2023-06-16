using System;
using IO.Astrodynamics.Body.Spacecraft;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class FuelTankTests
    {
        [Fact]
        public void Create()
        {
            FuelTank fuelTank = new FuelTank("ft", "model", 4000.0);
            Assert.Equal("ft", fuelTank.Name);
            Assert.Equal("model", fuelTank.Model);
            Assert.Equal(4000.0, fuelTank.Capacity);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new FuelTank("", "model", 4000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "", 4000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "model", 0.0));
        }
    }
}