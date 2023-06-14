using System;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.OrbitalParameters;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Body
{
    public class SpacecraftTests
    {
        [Fact]
        public void Create()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF));
            Assert.Equal(-1001, spc.NaifId);
            Assert.Equal("MySpacecraft", spc.Name);
            Assert.Equal(1000.0, spc.DryOperatingMass);
            Assert.Equal(10000.0, spc.MaximumOperatingMass);
        }

        [Fact]
        public void CreateExceptions()
        {
            Clock clk = new Clock("My clock", 1.0 / 256.0);
            Assert.Throws<ArgumentException>(() => new Spacecraft(1001, "MySpacecraft", 1000.0, 10000.0, clk, new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentException>(() => new Spacecraft(-1001, "", 1000.0, 10000.0, clk, new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, null, new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, null)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clk, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Spacecraft(-1001, "MySpacecraft", 11000.0, 10000.0, clk, new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.GetEarthAtJ2000(), DateTime.MinValue, Frames.Frame.ICRF)));
        }
    }
}