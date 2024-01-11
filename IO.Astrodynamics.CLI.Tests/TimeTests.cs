using System.Text;
using IO.Astrodynamics.CLI.Commands;

namespace IO.Astrodynamics.CLI.Tests;

public class TimeTests
{
    [Fact]
    public void DateToJDUTC()
    {
        lock (Configuration.objLock)
        {
            var command = new TimeConverterCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.TimeConverter("Data", "1950-01-01T00:00:00Z", false, true, true, false, false);
            var res = sb.ToString();

            Assert.Equal($"2433282.5 JD UTC{Environment.NewLine}", res);
        }
    }
    
    [Fact]
    public void DateToSecondsFromJ2000()
    {
        lock (Configuration.objLock)
        {
            var command = new TimeConverterCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.TimeConverter("Data", "2020-01-01T12:00:00Z", true, false, false, true, false);
            var res = sb.ToString();

            Assert.Equal($"631152069.1839999 TDB{Environment.NewLine}", res);
        }
    }

    
}