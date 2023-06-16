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
            FuelTank fuelTank = new FuelTank("ft", "model", "sn1", 4000.0, 2000.0);
            Assert.Equal("ft", fuelTank.Name);
            Assert.Equal("model", fuelTank.Model);
            Assert.Equal(4000.0, fuelTank.Capacity);
            Assert.Equal(2000.0, fuelTank.InitialQuantity);
            Assert.Equal("sn1", fuelTank.SerialNumber);
        }

        [Fact]
        public void CreateInvalid()
        {
            Assert.Throws<ArgumentException>(() => new FuelTank("", "model", "sn1", 4000.0, 2000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "", "sn1", 4000.0, 2000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "model", "sn1", 0.0, 2000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "model", "", 4000.0, 2000.0));
            Assert.Throws<ArgumentException>(() => new FuelTank("ft", "model", "sn1", 4000.0, -10.0));
        }
    }
}