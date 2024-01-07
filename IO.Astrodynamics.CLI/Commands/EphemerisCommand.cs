using Cocona;

namespace IO.Astrodynamics.CLI.Commands;

public class EphemerisCommand
{
    public EphemerisCommand()
    {
    }

    [Command("ephemeris", Description = "Compute ephemeris of given object")]
    public Task Ephemeris(
        [Argument(Description = "Object identifier")] int objectId, 
        [Argument("Kernels directory path")] string kernelsPath,
        [Option(shortName: 'b', Description = "Begin epoch")] string begin, 
        [Option(shortName: 'e',Description = "End epoch")] string end,
        [Option(shortName: 's',Description = "Step size in seconds")] double step,
        [Option(shortName: 'f',Description = "Frame")] string frame,
        [Option(shortName: 'a',Description = "Aberration")] string aberration)
    {
    }
}