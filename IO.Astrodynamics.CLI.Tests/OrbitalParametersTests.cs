using System.Text;
using IO.Astrodynamics.CLI.Commands;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Tests;

public class OrbitalParametersTests
{
    [Fact]
    public void SvToSv()
    {
        lock (Configuration.objLock)
        {
            var command = new OrbitalParametersConverterCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.Converter("Data", "6800000.0 0.0 0.0 0.0 7000.0 0.0", 10, "0.0", "ICRF", true, false, false, false, true, false, false, "");
            var res = sb.ToString();

            Assert.Equal($"2433282.5 JD UTC{Environment.NewLine}", res);
        }
    }

    [Fact]
    public void Exceptions()
    {
        lock (Configuration.objLock)
        {
            var command = new TimeConverterCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", false, false, false, true, false, true));
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", true, true, false, true, false, true));
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", true, false, false, false, false, false));
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", true, false, false, true, true, false));
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", true, false, false, true, false, true));
            Assert.ThrowsAsync<ArgumentException>(() => command.TimeConverter("2020-01-01T12:00:00Z", true, false, false, false, true, true));
        }
    }
}