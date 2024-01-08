using System.Text;
using IO.Astrodynamics.CLI.Commands;

namespace IO.Astrodynamics.CLI.Tests;

public class EphemerisTests
{
    [Fact]
    public void CallWithAllOptions()
    {
        var command = new EphemerisCommand();
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        Console.SetOut(sw);
        command.Ephemeris("Data", 399, 10, new DateTime(2023, 01, 01, 1, 0, 0), new DateTime(2023, 01, 01, 2, 0, 0), TimeSpan.FromMinutes(1), "ICRF", "LT", "sv");
        var res = sb.ToString();
    }
}