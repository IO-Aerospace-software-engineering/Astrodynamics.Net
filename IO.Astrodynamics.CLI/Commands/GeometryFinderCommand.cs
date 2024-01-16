using System;
using System.IO;
using System.Threading.Tasks;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.CLI.Commands.Parameters;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class GeometryFinderCommand
{
    public GeometryFinderCommand()
    {
    }

    [Command("find-windows-from-coordinate-constraint", Description = "Find time windows when constraint occurs")]
    public Task CoordinateConstraint(
        [Argument(Description = "Kernels directory path")]
        string kernelsPath,
        [Argument(Description = "Observer identifier")]
        int observerId,
        [Argument(Description = "Target identifier")]
        int targetId,
        [Argument(Description = "CoordinateSystem")]
        string coordinateSystem,
        [Argument(Description = "Coordinate")] string coordinate,
        [Argument(Description = "Relational operator used to define search constraint")]
        string relationalOperator,
        [Argument(Description = "Value used by operator")]
        double value,
        WindowParameters window,
        [Argument(Description = "Initial step size")]
        TimeSpan step,
        [Option(shortName: 'a', Description = "Aberration - None by default")]
        string aberration = "None",
        [Option(shortName: 'd', Description = "Adjust value - 0.0 by default")]
        double adjustValue = 0.0,
        [Option(shortName: 'f', Description = "Frame - ICRF by default")]
        string frame = "ICRF")
    {
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        if (frame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            frame = "j2000";
        }

        var win = new Window(Helpers.ConvertDateTimeInput(window.Begin.Epoch), Helpers.ConvertDateTimeInput(window.End.Epoch));
        var abe = Enum.Parse<Aberration>(aberration, true);
        var coorSys = Enum.Parse<CoordinateSystem>(coordinateSystem, true);
        var coor = Enum.Parse<Coordinate>(coordinate, true);
        var op = Enumeration.GetValueFromDescription<RelationnalOperator>(relationalOperator);

        var res = API.Instance.FindWindowsOnCoordinateConstraint(win, observerId, targetId, new Frame(frame), coorSys, coor, op, value, adjustValue, abe, step);

        foreach (var windowResult in res)
        {
            Console.WriteLine(windowResult);
        }

        return Task.CompletedTask;
    }

    [Command("find-windows-from-distance-constraint", Description = "Find time windows where constraint occurs")]
    public Task DistanceConstraint(
        [Argument(Description = "Kernels directory path")]
        string kernelsPath,
        [Argument(Description = "Observer identifier")]
        int observerId,
        [Argument(Description = "Target identifier")]
        int targetId,
        [Argument(Description = "Relational operator used to define search constraint")]
        string relationalOperator,
        [Argument(Description = "Value used by operator")]
        double value,
        WindowParameters window,
        [Argument(Description = "Initial step size")]
        TimeSpan step,
        [Option(shortName: 'a', Description = "Aberration - None by default")]
        string aberration = "None"
        )
    {
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));

        var win = new Window(Helpers.ConvertDateTimeInput(window.Begin.Epoch), Helpers.ConvertDateTimeInput(window.End.Epoch));
        var abe = Enum.Parse<Aberration>(aberration, true);
        var op = Enumeration.GetValueFromDescription<RelationnalOperator>(relationalOperator);

        var res = API.Instance.FindWindowsOnDistanceConstraint(win, observerId, targetId, op, value, abe, step);

        foreach (var windowResult in res)
        {
            Console.WriteLine(windowResult);
        }

        return Task.CompletedTask;
    }
}