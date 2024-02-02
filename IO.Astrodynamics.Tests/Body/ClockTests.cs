using System;
using System.IO;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using Xunit;

namespace IO.Astrodynamics.Tests.Body
{
    public class ClockTests
    {
        public ClockTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }
        [Fact]
        public void CreateClock()
        {
            Clock clock = new Clock("clk", 1.0 / 256.0);
            Assert.Equal("clk", clock.Name);
            Assert.Equal(1.0 / 256.0, clock.Resolution);
        }

        [Fact]
        public async Task WriteClock()
        {
            Clock clock = new Clock("clk", 1.0 / 256.0);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 1000.0, 10000.0, clock,
                new StateVector(new Vector3(1.0, 2.0, 3.0), new Vector3(1.0, 2.0, 3.0), TestHelpers.EarthAtJ2000,
                    DateTime.MinValue, Frames.Frame.ICRF));
            await clock.WriteAsync(new FileInfo("clock.tsc"));
            TextReader tr = new StreamReader("clock.tsc");
            var res = await tr.ReadToEndAsync();
            Assert.Equal($"KPL/SCLK{Environment.NewLine}\\begindata{Environment.NewLine}SCLK_KERNEL_ID           = ( @1957-01-01/00:00:00.0 ){Environment.NewLine}SCLK_DATA_TYPE_-1001        = ( 1 ){Environment.NewLine}SCLK01_TIME_SYSTEM_-1001    = ( 1 ){Environment.NewLine}SCLK01_N_FIELDS_-1001       = ( 2 ){Environment.NewLine}SCLK01_MODULI_-1001         = ( 4294967296 256 ){Environment.NewLine}SCLK01_OFFSETS_-1001        = ( 0 0 ){Environment.NewLine}SCLK01_OUTPUT_DELIM_-1001   = ( 2 ){Environment.NewLine}SCLK_PARTITION_START_-1001  = ( 0.0000000000000E+00 ){Environment.NewLine}SCLK_PARTITION_END_-1001    = ( 2.8147497671065E+14 ){Environment.NewLine}SCLK01_COEFFICIENTS_-1001   = ( 0.0000000000000E+00     -1.3569552000000E+09     1.0000000000000E+00 ){Environment.NewLine}\\begintext", res);
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
            Assert.True(clock != clock2);
            Assert.False(clock == clock2);
            Assert.False(clock.Equals(clock2));
            Assert.False(clock.Equals((object)clock2));
            Assert.False(clock.Equals(null));
            Assert.False(clock.Equals((object)null));
        }
    }
}