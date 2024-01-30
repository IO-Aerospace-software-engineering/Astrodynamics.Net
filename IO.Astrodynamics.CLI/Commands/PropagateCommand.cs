using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.CLI.Commands.Parameters;
using IO.Astrodynamics.Cosmographia;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class PropagateCommand
{
    public PropagateCommand()
    {
    }

    [Command("propagate", Description = "Propagate a small body")]
    public async Task Propagate(
        [Argument(Description = "Kernels directory path")]
        string kernelsPath,
        [Argument(Description = "Body id")] int bodyId,
        Parameters.OrbitalParameters orbitalParameters,
        WindowParameters windowParameters,
        [Argument(Description = "Additional celestial bodies involved into the simulation")]
        int[] celestialBodies,
        [Argument(Description = "Output directory")]
        string outputDirectory,
        [Option('r', Description = "Include solar radiation pressure perturbation")]
        bool useSolarRadiationPressure,
        [Option('a', Description = "Include atmospheric drag perturbation (Earth and Mars only)")]
        bool useAtmosphericDrag,
        [Option('n', Description = "Number of degrees used by earth geopotential model (Max. 100")]
        ushort earthGeopotentialDegree = 10
    )
    {
        //Load kernels
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));
        
        //Initialize date
        Clock clock = new Clock($"clock{bodyId}", 65536);
        Spacecraft spacecraft = new Spacecraft(bodyId, $"body{bodyId}", 100.0, 100.0, clock, Helpers.ConvertToOrbitalParameters(orbitalParameters));
        List<CelestialBody> bodies = new List<CelestialBody>([spacecraft.InitialOrbitalParameters.Observer as CelestialBody]);
        bodies.AddRange(celestialBodies.Select(x => new CelestialBody(x)));

        //Build scenario
        Scenario scenario = new Scenario($"Scenario body{bodyId}", new Mission.Mission($"Mission {bodyId}"), Helpers.ConvertWindowInput(windowParameters));
        scenario.AddSpacecraft(spacecraft);
        bodies.ForEach((b)=>scenario.AddAdditionalCelestialBody(b));
        
        //Simulate
        var qualifiedOutputDirectory = new DirectoryInfo(outputDirectory);
        await scenario.SimulateWithoutManeuverAsync(qualifiedOutputDirectory, useAtmosphericDrag, useSolarRadiationPressure);
        
        //Export to cosmographia
        CosmographiaExporter cosmographiaExporter = new CosmographiaExporter();
        
        await cosmographiaExporter.ExportAsync(scenario, qualifiedOutputDirectory);
        scenario.RootDirectory.Parent?.Delete(true);
        
        Console.WriteLine($"Propagation completed. Now you can use generated kernels or visualize simulation in cosmographia.{Environment.NewLine}Output location : {qualifiedOutputDirectory.FullName}");
    }
}