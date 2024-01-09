using System.Text;
using IO.Astrodynamics.CLI.Commands;

namespace IO.Astrodynamics.CLI.Tests;

public class EphemerisTests
{
    [Fact]
    public void CallWithAllOptions()
    {
        lock (Configuration.objLock)
        {
            var command = new EphemerisCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.Ephemeris("Data", 399, 10, new DateTime(2023, 01, 01, 1, 0, 0), new DateTime(2023, 01, 01, 1, 1, 0), TimeSpan.FromMinutes(1), "ICRF", "LT", "ke");
            var res = sb.ToString();
            Assert.Equal(
                "Epoch : 2023-01-01T01:00:00.0000000 (TDB) A : 149548023692.8589 Ecc. : 0.016383768660595866 Inc. : 0.4090475386632512 AN : 6.283111166910646 AOP : 1.8230020944251075 M : 6.208211132464601 Frame : j2000\nEpoch : 2023-01-01T01:01:00.0000000 (TDB) A : 149548005605.26834 Ecc. : 0.016383648084550295 Inc. : 0.4090475387953283 AN : 6.283111165029856 AOP : 1.8230010654683242 M : 6.20822406354363 Frame : j2000\n"
                , res);
        }
    }

    [Fact]
    public void CallWithoutOptions()
    {
        lock (Configuration.objLock)
        {
            var command = new EphemerisCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.Ephemeris("Data", 399, 10, new DateTime(2023, 01, 01, 1, 0, 0), new DateTime(2023, 01, 01, 1, 1, 0), TimeSpan.FromMinutes(1));
            var res = sb.ToString();
            Assert.Equal(
                "Epoch : 2023-01-01T01:00:00.0000000 (TDB) Position : Vector3 { X = -25577262731.326492, Y = 132913320450.42278, Z = 57617007553.115654 } Velocity : Vector3 { X = -29812.532391293124, Y = -4864.249418137372, Z = -2109.60702632249 } Frame : j2000\nEpoch : 2023-01-01T01:01:00.0000000 (TDB) Position : Vector3 { X = -25579051481.302353, Y = 132913028585.51353, Z = 57616880972.382614 } Velocity : Vector3 { X = -29812.466803377927, Y = -4864.580889886812, Z = -2109.7507418164614 } Frame : j2000\n"
                , res);
        }
    }
}