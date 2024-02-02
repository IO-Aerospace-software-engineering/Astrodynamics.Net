using System;
using System.Globalization;
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

public class BodyInformationCommand
{
    public BodyInformationCommand()
    {
    }

    [Command("celestial-body-info", Description = "Get celestial body informations")]
    public Task GetInformations(
        [Argument(Description = "Kernels directory path")]
        string kernelsPath,
        [Argument(Description = "Celestial body identifier (Naif Identifier)")]
        int id)
    {
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        CelestialBody body = new CelestialBody(id);
        Console.WriteLine(body.ToString());
        return Task.CompletedTask;
    }
}