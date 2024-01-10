using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class EpochConverterCommand
{
    public EpochConverterCommand()
    {
    }

    [Command("epoch-converter", Description = "Convert a time system into another")]
    public Task Orientation(
        [Argument("Kernels directory path")] string kernelsPath,
        [Argument(Description = "Epoch")] string epoch)
    {
        if (frame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            frame = "j2000";
        }
        
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        
        var celestialItem = Helpers.CreateOrientable(objectId);

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