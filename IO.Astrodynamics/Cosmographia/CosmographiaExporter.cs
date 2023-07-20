// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IO.Astrodynamics.Cosmographia.Models;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.Cosmographia;

public class CosmographiaExporter
{
    public async Task Export(Scenario scenario, DirectoryInfo outputDirectory)
    {
        ExportRawData(scenario, outputDirectory);

        await ExportSpiceKernelsAsync(scenario, outputDirectory);

        await ExportSpacecraftsAsync(scenario, outputDirectory);

        await ExportSensors(scenario, outputDirectory);

        await ExportObservations(scenario, outputDirectory);

        await ExportLoader(scenario, outputDirectory);
    }

    private static async Task ExportLoader(Scenario scenario, DirectoryInfo outputDirectory)
    {
        LoadRootObject loaderJson = new LoadRootObject();
        loaderJson.name = $"{scenario.Mission.Name}_{scenario.Name}";
        loaderJson.version = "1.0";
        loaderJson.require = new[]
        {
            Path.Combine(outputDirectory.FullName, "spice.json"),
            Path.Combine(outputDirectory.FullName, "spacecrafts.json"),
            Path.Combine(outputDirectory.FullName, "sensors.json"),
            Path.Combine(outputDirectory.FullName, "observations.json")
        };
        await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, $"{scenario.Mission.Name}_{scenario.Name}.json"));
        await JsonSerializer.SerializeAsync(fileStream, loaderJson);
    }

    private static async Task ExportObservations(Scenario scenario, DirectoryInfo outputDirectory)
    {
        ObservationRootObject observationJson = new ObservationRootObject();
        observationJson.name = $"{scenario.Mission.Name}_{scenario.Name}";
        observationJson.version = "1.0";
        foreach (var spacecraft in scenario.Spacecrafts)
        {
            var maneuvers = spacecraft.GetManeuvers().Values.OfType<InstrumentPointingToAttitude>().ToArray();
            var maneuversGroupedByInstruments = maneuvers.GroupBy(x => x.Instrument);
            int idx = 0;
            foreach (var maneuverByInstrument in maneuversGroupedByInstruments)
            {
                //add instrument item
                observationJson.items[idx].trajectoryFrame = new ObservationTrajectoryFrame();
                observationJson.items[idx].trajectoryFrame.type = "BodyFixed";
                observationJson.items[idx].trajectoryFrame.body = spacecraft.InitialOrbitalParameters.Observer.Name;
                observationJson.items[idx].bodyFrame.type = "BodyFixed";
                observationJson.items[idx].bodyFrame.body = spacecraft.InitialOrbitalParameters.Observer.Name;

                observationJson.items[idx].geometry = new ObservationGeometry();
                observationJson.items[idx].geometry.type = "Observations";
                observationJson.items[idx].geometry.sensor = maneuverByInstrument.Key.Name;
                Random rnd = new Random();
                observationJson.items[idx].geometry.footprintColor = new[]
                {
                    rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()
                };
                observationJson.items[idx].geometry.footprintOpacity = 0.4;
                observationJson.items[idx].geometry.showResWithColor = false;
                observationJson.items[idx].geometry.sideDivisions = 125;
                observationJson.items[idx].geometry.alongTrackDivisions = 500;
                observationJson.items[idx].geometry.shadowVolumeScaleFactor = 1.75;
                observationJson.items[idx].geometry.fillInObservations = false;
                int groupIdx = 0;
                foreach (var maneuver in maneuverByInstrument)
                {
                    observationJson.items[idx].geometry.groups[groupIdx] = new ObservationGroup();
                    observationJson.items[idx].geometry.groups[groupIdx].startTime = maneuver.ManeuverWindow.StartDate.ToFormattedString();
                    observationJson.items[idx].geometry.groups[groupIdx].endTime = maneuver.ManeuverWindow.EndDate.ToFormattedString();
                    observationJson.items[idx].geometry.groups[groupIdx].obsRate = 0;
                    groupIdx++;
                }

                idx++;
            }

            await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, "observations.json"));
            await JsonSerializer.SerializeAsync(fileStream, observationJson);
        }
    }

    private static async Task ExportSensors(Scenario scenario, DirectoryInfo outputDirectory)
    {
        SensorRootObject sensorJson = new SensorRootObject();
        sensorJson.name = $"{scenario.Mission.Name}_{scenario.Name}";
        sensorJson.version = "1.0";
        foreach (var spacecraft in scenario.Spacecrafts)
        {
            int idx = 0;
            foreach (var instrument in spacecraft.Intruments)
            {
                sensorJson.items[idx] = new SensorItem();
                sensorJson.items[idx].center = spacecraft.Name;
                sensorJson.items[idx].name = instrument.Name;
                sensorJson.items[idx].startTime = scenario.Window.StartDate.ToFormattedString();
                sensorJson.items[idx].endTime = scenario.Window.EndDate.ToFormattedString();
                sensorJson.items[idx].parent = spacecraft.Name;
                sensorJson.items[idx].sensorClass = "sensor";

                sensorJson.items[idx].geometry = new SensorGeometry();
                sensorJson.items[idx].geometry.type = "Spice";
                sensorJson.items[idx].geometry.instrName = instrument.Name;
                sensorJson.items[idx].geometry.target = spacecraft.InitialOrbitalParameters.Observer.Name;
                sensorJson.items[idx].geometry.range = 100000;
                sensorJson.items[idx].geometry.rangeTracking = true;
                Random rnd = new Random();
                sensorJson.items[idx].geometry.frustumColor = new[]
                {
                    rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()
                };
                sensorJson.items[idx].geometry.frustumBaseLineWidth = 3;
                sensorJson.items[idx].geometry.frustumOpacity = 0.3;
                sensorJson.items[idx].geometry.gridOpacity = 1;
                sensorJson.items[idx].geometry.footprintOpacity = 0.8;
                sensorJson.items[idx].geometry.sideDivisions = 300;
                sensorJson.items[idx].geometry.onlyVisibleDuringObs = false;
                idx++;
            }
        }

        await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, "sensors.json"));
        await JsonSerializer.SerializeAsync(fileStream, sensorJson);
    }

    private static async Task ExportSpacecraftsAsync(Scenario scenario, DirectoryInfo outputDirectory)
    {
        Models.SpacecraftRootObject spacecraftJson = new SpacecraftRootObject();
        spacecraftJson.name = $"{scenario.Mission.Name}_{scenario.Name}";
        spacecraftJson.version = "1.0";

        int idx = 0;
        foreach (var spacecraft in scenario.Spacecrafts)
        {
            spacecraftJson.items[idx] = new SpacecraftItem();
            spacecraftJson.items[idx].name = spacecraft.Name;
            spacecraftJson.items[idx].center = spacecraft.InitialOrbitalParameters.Observer.Name;
            spacecraftJson.items[idx].spacecraftClass = "spacecraft";
            spacecraftJson.items[idx].startTime = scenario.Window.StartDate.ToFormattedString();
            spacecraftJson.items[idx].endTime = scenario.Window.EndDate.ToFormattedString();

            spacecraftJson.items[idx].trajectory = new SpacecraftTrajectory();
            spacecraftJson.items[idx].trajectory.center = spacecraft.InitialOrbitalParameters.Observer.Name;
            spacecraftJson.items[idx].trajectory.target = spacecraft.Name;
            spacecraftJson.items[idx].trajectory.type = "Spice";

            spacecraftJson.items[idx].bodyFrame = new SpacecraftBodyFrame();
            spacecraftJson.items[idx].bodyFrame.name = spacecraft.Frame.Name;
            spacecraftJson.items[idx].bodyFrame.type = "spice";

            spacecraftJson.items[idx].geometry = new SpacecraftGeometry();
            spacecraftJson.items[idx].geometry.type = "Globe";
            spacecraftJson.items[idx].geometry.radii = new[] { 0.001, 0.001, 0.001 };

            spacecraftJson.items[idx].label = new SpacecraftLabel();
            Random rnd = new Random();
            spacecraftJson.items[idx].label.color = new[]
            {
                rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble()
            };
            spacecraftJson.items[idx].label.showText = true;

            spacecraftJson.items[idx].trajectoryPlot = new SpacecraftTrajectoryPlot();
            spacecraftJson.items[idx].trajectoryPlot.color = spacecraftJson.items[0].label.color;
            spacecraftJson.items[idx].trajectoryPlot.duration = $"{spacecraft.InitialOrbitalParameters.Period().Days} d";
            spacecraftJson.items[idx].trajectoryPlot.fade = 1;
            spacecraftJson.items[idx].trajectoryPlot.lead = $"{spacecraft.InitialOrbitalParameters.Period().Days * 0.1} d";
            spacecraftJson.items[idx].trajectoryPlot.visible = true;
            spacecraftJson.items[idx].trajectoryPlot.lineWidth = 5;

            idx++;
        }

        await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, "spacecrafts.json"));
        await JsonSerializer.SerializeAsync(fileStream, spacecraftJson);
    }

    private static async Task ExportSpiceKernelsAsync(Scenario scenario, DirectoryInfo outputDirectory)
    {
        var files = scenario.RootDirectory.GetFiles("*.*", SearchOption.AllDirectories);
        Models.SpiceRootObject spiceJson = new SpiceRootObject();
        spiceJson.version = "1.0";
        spiceJson.name = $"{scenario.Mission.Name}_{scenario.Name}";
        spiceJson.spiceKernels = files.Select(x => x.FullName).ToArray();
        await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, "spice.json"));
        await JsonSerializer.SerializeAsync(fileStream, spiceJson);
    }

    private static void ExportRawData(Scenario scenario, DirectoryInfo outputDirectory)
    {
        CopyDirectory(scenario.RootDirectory, outputDirectory, true);
    }

    static void CopyDirectory(DirectoryInfo sourceDir, DirectoryInfo destinationDir, bool recursive = false)
    {
        // Check if the source directory exists
        if (!sourceDir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = sourceDir.GetDirectories();

        // Create the destination directory
        destinationDir.Create();

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in sourceDir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir.FullName, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                var newDestinationDir = new DirectoryInfo(Path.Combine(destinationDir.FullName, subDir.Name));
                CopyDirectory(subDir, newDestinationDir, true);
            }
        }
    }

    static IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo, bool recursive)
    {
        return directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
    }
}