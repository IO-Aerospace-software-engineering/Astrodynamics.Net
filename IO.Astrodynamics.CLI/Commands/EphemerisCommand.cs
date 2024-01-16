using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.CLI.Commands.Parameters;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class EphemerisCommand
{
    public EphemerisCommand()
    {
    }

    [Command("ephemeris", Description = "Compute ephemeris of given object")]
    public Task Ephemeris(
        [Argument(Description = "Kernels directory path")]
        string kernelsPath,
        [Argument(Description = "Object identifier")]
        int objectId,
        [Argument(Description = "Observer identifier")]
        int observerId,
        WindowParameters windowParameters,
        [Option(shortName: 's', Description = "Step size")]
        TimeSpan step,
        [Option(shortName: 'f', Description = "Frame - ICRF by default")]
        string frame = "ICRF",
        [Option(shortName: 'a', Description = "Aberration - None by default")]
        string aberration = "None",
        [Option(Description = "Output format - sv for state vector(default) or ke for keplerian")]
        string outputFormat = "sv")
    {
        if (frame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            frame = "j2000";
        }

        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));

        var localizableObject = Helpers.CreateLocalizable(objectId);

        var observerItem = Helpers.CreateLocalizable(observerId);

        var ephemeris = localizableObject.GetEphemeris(Helpers.ConvertWindowInput(windowParameters.Begin.Epoch, windowParameters.End.Epoch), observerItem, new Frame(frame),
            Enum.Parse<Aberration>(aberration, true), step);

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