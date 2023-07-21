// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Cosmographia;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;
using Xunit;
using Xunit.Abstractions;

namespace IO.Astrodynamics.Tests.Cosmographia;

public class ExporterTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExporterTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public async Task ExportSimplePropagation()
    {
        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Cosmographia");
        Scenario scenario = new Scenario("export1", mission, new Window(DateTimeExtension.J2000.AddYears(21), DateTimeExtension.J2000.AddYears(21).AddDays(1.0)));
        Spacecraft spacecraft = new Spacecraft(-333, "spc1", 1000.0, 2000.0, new Clock("clockspc1", 1 / 256.0),
            new KeplerianElements(6800000.0, 0.0, 0.0, 0.0, 0.0, 0.0, TestHelpers.EarthAtJ2000, DateTimeExtension.J2000, Frames.Frame.ICRF));
        scenario.AddSpacecraft(spacecraft);
        scenario.Simulate(Constants.OutputPath);

        CosmographiaExporter exporter = new CosmographiaExporter();
        await exporter.ExportAsync(scenario, new DirectoryInfo("CosmographiaExport"));
    }

    [Fact]
    public async Task ExportWithObservationPropagation()
    {
        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Cosmographia");
        Scenario scenario = new Scenario("export2", mission, new Window(DateTimeExtension.J2000.AddYears(21), DateTimeExtension.J2000.AddYears(21).AddDays(1.0)));
        Spacecraft spacecraft = new Spacecraft(-334, "Lois", 1000.0, 2000.0, new Clock("clockspc1", 1 / 256.0),
            new KeplerianElements(40800000.0, 0.3, 1.0, 0.0, 0.0, 0.0, TestHelpers.EarthAtJ2000, DateTimeExtension.J2000, Frames.Frame.ICRF));
        var instrument = new Instrument(-334100, "camera_hires", "camdeluxe", 0.5, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorY, Vector3.Zero);
        spacecraft.AddInstrument(instrument);
        spacecraft.AddFuelTank(new FuelTank("fuelTank1", "fuelTankModel", "456", 2000.0, 2000.0));
        spacecraft.AddEngine(new Engine("engine1", "engineModel", "1234", 450, 50.0, spacecraft.FuelTanks.First()));
        scenario.AddSpacecraft(spacecraft);
        spacecraft.SetStandbyManeuver(new InstrumentPointingToAttitude(scenario.Window.StartDate.AddMinutes(10.0), TimeSpan.FromHours(1.0), instrument,
            spacecraft.InitialOrbitalParameters.Observer,
            spacecraft.Engines.First()));
        scenario.Simulate(Constants.OutputPath);

        var res = spacecraft.GetEphemeris(new Window(new DateTime(2021, 1, 1, 13, 7, 0, DateTimeKind.Utc), new DateTime(2021, 1, 1, 13, 8, 0, DateTimeKind.Utc)),
            spacecraft.InitialOrbitalParameters.Observer, Frames.Frame.ICRF, Aberration.None, TimeSpan.FromSeconds(1.0));

        foreach (var sv in res)
        {
            _testOutputHelper.WriteLine(sv.ToStateVector().Position.ToString());
        }

        CosmographiaExporter exporter = new CosmographiaExporter();
        await exporter.ExportAsync(scenario, new DirectoryInfo("CosmographiaExport"));
    }
}