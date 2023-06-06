using System;
using System.Linq;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using Xunit;
namespace IO.Astrodynamics.Models.Tests.Body
{
    public class SpacecraftTests
    {
        [Fact]
        public void Create()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "My spacecraft", 1000.0);
            Assert.Equal(-1001, spc.NaifId);
            Assert.Equal("My spacecraft", spc.Name);
            Assert.Equal(1000.0, spc.DryOperatingMass);
        }

        [Fact]
        public void CreateExceptions()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Assert.Throws<ArgumentException>(() => new Spacecraft(-1001, "", 1000.0));
            Assert.Throws<ArgumentException>(() => new Spacecraft(-1001, "My spacecraft", 0.0));            
        }

        
    }
}