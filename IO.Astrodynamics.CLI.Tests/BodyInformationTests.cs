using System.Text;
using IO.Astrodynamics.CLI.Commands;
using IO.Astrodynamics.CLI.Commands.Parameters;

namespace IO.Astrodynamics.CLI.Tests;

public class BodyInformationsTests
{
    [Fact]
    public void CelestialBodyInformation()
    {
        lock (Configuration.objLock)
        {
            var command = new BodyInformationCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.GetInformations("Data", 399);
            var res = sb.ToString();

            Assert.Equal(
                $"                          Type : Planet                          \r\n                    Identifier : 399                             \r\n                          Name : EARTH                           \r\n                     Mass (kg) : 5.972168E+024                   \r\n                   GM (m^3.s^2): 3.986004E+014                   \r\n                   Fixed frame : ITRF93                          \r\n         Equatorial radius (m) : 6.378137E+006                   \r\n              Polar radius (m) : 6.356752E+006                   \r\n                    Flattening : 0.0033528131084554157           \r\n                            J2 : 0.001082616                     \r\n                            J3 : -2.5388099999999996E-06         \r\n                            J4 : -1.65597E-06                    \r\n\r\n"
                , res);
        }
    }
}