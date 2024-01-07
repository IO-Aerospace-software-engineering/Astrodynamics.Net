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
        [Argument(Description = "Object identifier")]
        int objectId,
        [Argument("Kernels directory path")] string kernelsPath,
        [Option(shortName: 'o', Description = "Observer identifier")]
        int observerId,
        [Option(shortName: 'b', Description = "Begin epoch")]
        string begin,
        [Option(shortName: 'e', Description = "End epoch")]
        string end,
        [Option(shortName: 's', Description = "Step size in seconds")]
        double step,
        [Option(shortName: 'f', Description = "Frame")]
        string frame,
        [Option(shortName: 'a', Description = "Aberration")]
        string aberration,
        [Option(Description = "Output format - sv (state vector) or ke (keplerian)")]
        string outputFormat)
    {
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

        DateTime startDate = DateTime.Parse(begin);
        DateTime endDate = DateTime.Parse(end);
        TimeSpan stepSize = TimeSpan.FromSeconds(step);

        var res = celestialItem.GetEphemeris(new Window(startDate, endDate), observerItem, new Frame(frame), Enum.Parse<Aberration>(aberration, true), stepSize);

        if (outputFormat == "ke")
        {
            res = res.Select(x => x.ToKeplerianElements());
        }
    }
}