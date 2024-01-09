using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class OrientationCommand
{
    public OrientationCommand()
    {
    }

    [Command("orientation", Description = "Compute orientations of given object")]
    public Task Orientation(
        [Argument("Kernels directory path")] string kernelsPath,
        [Argument(Description = "Object identifier")] int objectId,
        [Option(shortName: 'b', Description = "Begin epoch")] DateTime begin,
        [Option(shortName: 'e', Description = "End epoch")] DateTime end,
        [Option(shortName: 's', Description = "Step size in seconds")] TimeSpan step,
        [Option(shortName: 'f', Description = "Frame - ICRF by default")] string frame="ICRF")
    {
        if (frame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            frame = "j2000";
        }
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        
        IOrientable celestialItem;
        if (int.IsPositive(objectId))
        {
            celestialItem = new CelestialBody(objectId);
        }
        else
        {
            celestialItem = new Spacecraft(objectId, "spc", 1.0, 1.0, new Clock("clk", 1 / 65536.0), null);
        }

        List<StateOrientation> orientations = new List<StateOrientation>();
        Frame targetFrame = new Frame(frame);
        for (DateTime epoch = begin; epoch <= end; epoch+=step)
        {
            orientations.Add(celestialItem.GetOrientation(targetFrame,epoch));
        }

        foreach (var orientation in orientations)
        {
            Console.WriteLine(orientation.ToString());
        }

        return Task.CompletedTask;
    }
}