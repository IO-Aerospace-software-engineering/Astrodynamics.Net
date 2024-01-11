using System.Globalization;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class TimeConverterCommand
{
    public TimeConverterCommand()
    {
    }

    [Command("time-converter", Description = "Convert a time system into another")]
    public Task TimeConverter(
        [Argument("Kernels directory path")] string kernelsPath,
        [Argument(Description = "Epoch")] string epoch,
        [Option('t', Description = "Convert to TDB")]
        bool toTDB,
        [Option('u', Description = "Convert to UTC")]
        bool toUTC,
        [Option('j', Description = "Convert to Julian date")]
        bool toJulian,
        [Option('e', Description = "Convert to elapsed seconds from J2000 epoch")]
        bool toSecondsFromJ2000,
        [Option('d', Description = "Convert to DateTime")]
        bool toDateTime)
    {
        var isutc = epoch.Contains("utc", StringComparison.InvariantCultureIgnoreCase) || epoch.Contains("z", StringComparison.InvariantCultureIgnoreCase);
        var isjd = epoch.Contains("jd", StringComparison.InvariantCultureIgnoreCase);

        //clean string
        epoch = epoch.Replace("utc", "", StringComparison.InvariantCultureIgnoreCase).Replace("tdb", "", StringComparison.InvariantCultureIgnoreCase).Trim();

        //Input
        DateTime input;
        if (isjd)
        {
            epoch = epoch.Replace("jd", "", StringComparison.InvariantCultureIgnoreCase).Trim();
            double value = double.Parse(epoch, CultureInfo.InvariantCulture);
            if (isutc)
            {
                input = DateTimeExtension.CreateUTCFromJD(value);
            }
            else
            {
                input = DateTimeExtension.CreateTDB(value);
            }
        }

        if (!DateTime.TryParse(epoch, out input))
        {
            double value = double.Parse(epoch, CultureInfo.InvariantCulture);
            if (isutc)
            {
                input = DateTimeExtension.CreateUTC(value);
            }
            else
            {
                input = DateTimeExtension.CreateTDB(value);
            }
        }

        //Output
        if (toTDB)
        {
            input = input.ToTDB();
        }
        else if (toUTC)
        {
            input = input.ToUTC();
        }

        string res = string.Empty;
        if (toJulian)
        {
            res = $"{input.ToJulianDate()} JD";
        }
        else if (toSecondsFromJ2000)
        {
            if (toUTC)
            {
                res = $"{input.SecondsFromJ2000UTC()}";
            }
            else
            {
                res = $"{input.SecondsFromJ2000TDB()}";
            }
        }
        else if (toDateTime)
        {
            res = input.ToFormattedString();
        }

        string suffix = toUTC ? "UTC" : "TDB";
        Console.WriteLine($"{res} {suffix}");
        return Task.CompletedTask;
    }
}