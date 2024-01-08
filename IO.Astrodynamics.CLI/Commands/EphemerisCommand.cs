using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class EphemerisCommand
{
    public EphemerisCommand()
    {
    }

    [Command("ephemeris", Description = "Compute ephemeris of given object")]
    public Task Ephemeris(
        [Argument("Kernels directory path")] string kernelsPath,
        [Argument(Description = "Object identifier")] int objectId,
        [Argument(Description = "Observer identifier")] int observerId,
        [Option(shortName: 'b', Description = "Begin epoch")] DateTime begin,
        [Option(shortName: 'e', Description = "End epoch")] DateTime end,
        [Option(shortName: 's', Description = "Step size in seconds")] TimeSpan step,
        [Option(shortName: 'f', Description = "Frame - ICRF by default")] string frame="ICRF",
        [Option(shortName: 'a', Description = "Aberration - None by default")] string aberration= "None",
        [Option(Description = "Output format - sv for state vector(default) or ke for keplerian")] string outputFormat="sv")
    {
        if (frame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            frame = "j2000";
        }
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        
        CelestialItem celestialItem;
        if (int.IsPositive(objectId))
        {
            celestialItem = new CelestialBody(objectId);
        }
        else
        {
            celestialItem = new Spacecraft(objectId, "spc", 1.0, 1.0, new Clock("clk", 1 / 65536.0), null);
        }

        CelestialItem observerItem;
        if (int.IsPositive(observerId))
        {
            observerItem = new CelestialBody(observerId);
        }
        else
        {
            observerItem = new Spacecraft(observerId, "spc2", 1.0, 1.0, new Clock("clk2", 1 / 65536.0), null);
        }

        var ephemeris = celestialItem.GetEphemeris(new Window(begin, end), observerItem, new Frame(frame), Enum.Parse<Aberration>(aberration, true), step);

        if (outputFormat == "ke")
        {
            ephemeris = ephemeris.Select(x => x.ToKeplerianElements());
        }

        foreach (var eph in ephemeris)
        {
            Console.WriteLine(eph.ToString());
        }

        return Task.CompletedTask;
    }
}