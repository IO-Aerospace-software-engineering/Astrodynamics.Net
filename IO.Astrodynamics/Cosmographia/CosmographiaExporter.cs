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
    public async Task ExportAsync(Scenario scenario, DirectoryInfo outputDirectory)
    {
        if (!scenario.IsSimulated)
        {
            throw new InvalidOperationException("The scenario has not yet been simulated. Call the Simulate method on the scenario before calling the cosmographia exporter.");
        }

        var missionOutputDirectory = outputDirectory.CreateSubdirectory($"{scenario.Mission.Name}_{scenario.Name}");

        missionOutputDirectory.Delete(true);

        ExportRawData(scenario, missionOutputDirectory);

        await ExportSpiceKernelsAsync(scenario, missionOutputDirectory);

        await ExportSpacecraftsAsync(scenario, missionOutputDirectory);

        await ExportSensors(scenario, missionOutputDirectory);

        await ExportObservations(scenario, missionOutputDirectory);

        await ExportLoader(scenario, missionOutputDirectory);
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
            observationJson.items = new ObservationItems[maneuversGroupedByInstruments.Count()];
            int idx = 0;
            foreach (var maneuverByInstrument in maneuversGroupedByInstruments)
            {
                observationJson.items[idx] = new ObservationItems();
                observationJson.items[idx].obsClass = "observation";
                observationJson.items[idx].name = $"{spacecraft.Name.ToUpper()}_{maneuverByInstrument.Key.Name.ToUpper()}_OBSERVATIONS";
                observationJson.items[idx].startTime = scenario.Window.StartDate.ToFormattedString();
                observationJson.items[idx].endTime = scenario.Window.EndDate.ToFormattedString();
                observationJson.items[idx].center = char.ToUpper(spacecraft.InitialOrbitalParameters.Observer.Name[0]) +
                                                    spacecraft.InitialOrbitalParameters.Observer.Name.Substring(1).ToLower();
                observationJson.items[idx].trajectoryFrame = new ObservationTrajectoryFrame();
                observationJson.items[idx].trajectoryFrame.type = "BodyFixed";
                observationJson.items[idx].trajectoryFrame.body = char.ToUpper(spacecraft.InitialOrbitalParameters.Observer.Name[0]) +
                                                                  spacecraft.InitialOrbitalParameters.Observer.Name.Substring(1).ToLower();

                observationJson.items[idx].bodyFrame = new ObservationBodyFrame();
                observationJson.items[idx].bodyFrame.type = "BodyFixed";
                observationJson.items[idx].bodyFrame.body = char.ToUpper(spacecraft.InitialOrbitalParameters.Observer.Name[0]) +
                                                            spacecraft.InitialOrbitalParameters.Observer.Name.Substring(1).ToLower();;

                observationJson.items[idx].geometry = new ObservationGeometry();
                observationJson.items[idx].geometry.type = "Observations";
                observationJson.items[idx].geometry.sensor = $"{spacecraft.Name.ToUpper()}_{maneuverByInstrument.Key.Name.ToUpper()}";
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

                observationJson.items[idx].geometry.groups = new ObservationGroup[maneuverByInstrument.Count()];
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
        sensorJson.items = new SensorItem[scenario.Spacecrafts.Sum(x => x.Intruments.Count)];
        int idx = 0;
        foreach (var spacecraft in scenario.Spacecrafts)
        {
            foreach (var instrument in spacecraft.Intruments)
            {
                sensorJson.items[idx] = new SensorItem();
                sensorJson.items[idx].center = spacecraft.Name.ToUpper();
                sensorJson.items[idx].name = $"{spacecraft.Name.ToUpper()}_{instrument.Name.ToUpper()}";
                sensorJson.items[idx].startTime = scenario.Window.StartDate.ToFormattedString();
                sensorJson.items[idx].endTime = scenario.Window.EndDate.ToFormattedString();
                sensorJson.items[idx].parent = spacecraft.Name.ToUpper();
                sensorJson.items[idx].sensorClass = "sensor";

                sensorJson.items[idx].geometry = new SensorGeometry();
                sensorJson.items[idx].geometry.type = "Spice";
                sensorJson.items[idx].geometry.instrName = $"{spacecraft.Name.ToUpper()}_{instrument.Name.ToUpper()}";
                sensorJson.items[idx].geometry.target = char.ToUpper(spacecraft.InitialOrbitalParameters.Observer.Name[0]) +
                                                        spacecraft.InitialOrbitalParameters.Observer.Name.Substring(1).ToLower();
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
        spacecraftJson.items = new SpacecraftItem[scenario.Spacecrafts.Count];
        int idx = 0;
        foreach (var spacecraft in scenario.Spacecrafts)
        {
            spacecraftJson.items[idx] = new SpacecraftItem();
            spacecraftJson.items[idx].name = spacecraft.Name.ToUpper();
            spacecraftJson.items[idx].center = char.ToUpper(spacecraft.InitialOrbitalParameters.Observer.Name[0]) +
                                               spacecraft.InitialOrbitalParameters.Observer.Name.Substring(1).ToLower();
            spacecraftJson.items[idx].spacecraftClass = "spacecraft";
            spacecraftJson.items[idx].startTime = scenario.Window.StartDate.ToFormattedString();
            spacecraftJson.items[idx].endTime = scenario.Window.EndDate.ToFormattedString();

            spacecraftJson.items[idx].trajectory = new SpacecraftTrajectory();
            spacecraftJson.items[idx].trajectory.center = spacecraftJson.items[idx].center;
            spacecraftJson.items[idx].trajectory.target = spacecraft.Name.ToUpper();
            spacecraftJson.items[idx].trajectory.type = "Spice";

            spacecraftJson.items[idx].bodyFrame = new SpacecraftBodyFrame();
            spacecraftJson.items[idx].bodyFrame.name = spacecraft.Frame.Name.ToUpper();
            spacecraftJson.items[idx].bodyFrame.type = "Spice";

            spacecraftJson.items[idx].geometry = new SpacecraftGeometry();
            spacecraftJson.items[idx].geometry.type = "Globe";
            spacecraftJson.items[idx].geometry.radii = new[] { 0.001, 0.001, 0.001 };

            spacecraftJson.items[idx].label = new SpacecraftLabel();
            Random rnd = new Random();
            spacecraftJson.items[idx].label.color = new[]
            {
                rnd.NextDouble() * 1.3, rnd.NextDouble() * 1.3, rnd.NextDouble() * 1.3
            };
            spacecraftJson.items[idx].label.showText = true;

            spacecraftJson.items[idx].trajectoryPlot = new SpacecraftTrajectoryPlot();
            spacecraftJson.items[idx].trajectoryPlot.color = spacecraftJson.items[0].label.color;
            spacecraftJson.items[idx].trajectoryPlot.duration = $"{spacecraft.InitialOrbitalParameters.Period().TotalDays} d";
            spacecraftJson.items[idx].trajectoryPlot.fade = 1;
            spacecraftJson.items[idx].trajectoryPlot.lead = $"{spacecraft.InitialOrbitalParameters.Period().TotalDays * 0.1} d";
            spacecraftJson.items[idx].trajectoryPlot.visible = true;
            spacecraftJson.items[idx].trajectoryPlot.lineWidth = 3;

            idx++;
        }

        await using var fileStream = File.Create(Path.Combine(outputDirectory.FullName, "spacecrafts.json"));
        await JsonSerializer.SerializeAsync(fileStream, spacecraftJson);
    }

    private static async Task ExportSpiceKernelsAsync(Scenario scenario, DirectoryInfo outputDirectory)
    {
        var files = outputDirectory.GetFiles("*.*", SearchOption.AllDirectories);
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