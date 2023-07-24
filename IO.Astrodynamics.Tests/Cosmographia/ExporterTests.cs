// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Cosmographia;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Surface;
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
        //Create mission
        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Cosmographia");
        
        //Create a scenario for the mission
        Scenario scenario = new Scenario("export2", mission, new Window(DateTimeExtension.J2000.AddYears(21), DateTimeExtension.J2000.AddYears(21).AddDays(1.0))); 
        
        //Configure a spacecraft
        Spacecraft spacecraft = new Spacecraft(-334, "Spacecraft", 1000.0, 2000.0, new Clock("clockspc1", 1 / 256.0),
            new KeplerianElements(6800000.0, 0.3, 1.0, 0.0, 0.0, 0.0, TestHelpers.MoonAtJ2000, DateTimeExtension.J2000, Frames.Frame.ICRF));
        
        //Configure and attach an instrument to the spacecraft
        var instrument = new Instrument(-334100, "camera_hires", "camdeluxe", 0.03, InstrumentShape.Rectangular, Vector3.VectorZ, Vector3.VectorY, Vector3.Zero, 0.048);
        spacecraft.AddInstrument(instrument);
        
        //Add a fuel tank
        spacecraft.AddFuelTank(new FuelTank("fuelTank1", "fuelTankModel", "456", 2000.0, 2000.0));
        
        //Add an engine
        spacecraft.AddEngine(new Engine("engine1", "engineModel", "1234", 450, 50.0, spacecraft.FuelTanks.First()));
        
        //Attach the spacecraft to the scenario
        scenario.AddSpacecraft(spacecraft);
        
        //Configure the first maneuver
        var initialManeuver = new InstrumentPointingToAttitude(scenario.Window.StartDate.AddMinutes(10.0), TimeSpan.FromHours(1.0), instrument,
            spacecraft.InitialOrbitalParameters.Observer, spacecraft.Engines.First());
        
        //Configure the second maneuver and link it to the first maneuver
        initialManeuver.SetNextManeuver(new InstrumentPointingToAttitude(scenario.Window.StartDate.AddHours(2.0), TimeSpan.FromHours(1.0), instrument,
            spacecraft.InitialOrbitalParameters.Observer, spacecraft.Engines.First()));
        
        //Set the first maneuver in standby
        spacecraft.SetStandbyManeuver(initialManeuver);
        
        //Run the simulation
        scenario.Simulate(Constants.OutputPath);

        //Export scenario to Cosmographia
        CosmographiaExporter exporter = new CosmographiaExporter();
        await exporter.ExportAsync(scenario, new DirectoryInfo("CosmographiaExport"));
    }

    [Fact]
    public async Task ExportWithObservationAndSitePropagation()
    {
        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Cosmographia");
        Scenario scenario = new Scenario("export3", mission, new Window(DateTimeExtension.J2000.AddYears(21), DateTimeExtension.J2000.AddYears(21).AddDays(1.0)));
        Site site3 = new Site(14, "DSS-14", TestHelpers.EarthAtJ2000);
        Site site = new Site(34, "DSS-34", TestHelpers.EarthAtJ2000);
        Site site2 = new Site(65, "DSS-65", TestHelpers.EarthAtJ2000);

        scenario.AddSite(site);
        scenario.AddSite(site2);
        scenario.AddSite(site3);
        Spacecraft spacecraft = new Spacecraft(-334, "Spacecraft", 1000.0, 2000.0, new Clock("clockspc1", 1 / 256.0),
            new KeplerianElements(11800000.0, 0.3, 1.0, 0.0, 0.0, 0.0, TestHelpers.EarthAtJ2000, DateTimeExtension.J2000, Frames.Frame.ICRF));
        var instrument = new Instrument(-334100, "Antenna", "antdeluxe", 0.2, InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorY, Vector3.Zero);
        spacecraft.AddInstrument(instrument);
        spacecraft.AddFuelTank(new FuelTank("fuelTank1", "fuelTankModel", "456", 2000.0, 2000.0));
        spacecraft.AddEngine(new Engine("engine1", "engineModel", "1234", 450, 50.0, spacecraft.FuelTanks.First()));
        scenario.AddSpacecraft(spacecraft);
        var initialManeuver = new InstrumentPointingToAttitude(scenario.Window.StartDate.AddHours(7.25), TimeSpan.FromHours(0.5), instrument, site, spacecraft.Engines.First());
        spacecraft.SetStandbyManeuver(initialManeuver);
        scenario.Simulate(Constants.OutputPath);

        CosmographiaExporter exporter = new CosmographiaExporter();
        await exporter.ExportAsync(scenario, new DirectoryInfo("CosmographiaExport"));
    }

    [Fact]
    public async Task ExportSpacecraftReachTarget()
    {
        DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB(); //3/2/2021 00:02:18
        DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB(); //3/3/2021 23:20:25
        DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB(); // 3/5/2021 00:01:09

        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
        Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
        scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);

        //Define parking orbit
        StateVector parkingOrbit = new StateVector(
            new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.EarthAtJ2000,
            start, Frames.Frame.ICRF);

        //Define target orbit
        StateVector targetOrbit = new StateVector(
            new Vector3(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265), TestHelpers.EarthAtJ2000,
            start, Frames.Frame.ICRF);

        //Create and configure spacecraft1
        Clock clock1 = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft1 =
            new Spacecraft(-1790, "Target", 1000.0, 10000.0, clock1, targetOrbit);

        FuelTank fuelTank1 = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine1 = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank1);
        spacecraft1.AddFuelTank(fuelTank1);
        spacecraft1.AddEngine(engine1);
        spacecraft1.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft1.AddInstrument(
            new Instrument(-1790601, "CAM601", "mod1", 10.0 * IO.Astrodynamics.Constants.Deg2Rad,
                InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX));
        scenario.AddSpacecraft(spacecraft1);

        //Create and configure spacecraft2
        Clock clock2 = new Clock("clk2", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft2 =
            new Spacecraft(-1791, "Chaser", 1000.0, 10000.0, clock2, parkingOrbit);

        FuelTank fuelTank2 = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine2 = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank2);
        spacecraft2.AddFuelTank(fuelTank2);
        spacecraft2.AddEngine(engine2);
        spacecraft2.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft2.AddInstrument(
            new Instrument(-1791602, "CAM602", "mod1", 10.0 * IO.Astrodynamics.Constants.Deg2Rad,
                InstrumentShape.Circular, Vector3.VectorY, Vector3.VectorX, Vector3.Zero));

        var planeAlignmentManeuver = new PlaneAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero,
            targetOrbit, spacecraft2.Engines.First());
        planeAlignmentManeuver.SetNextManeuver(new ApsidalAlignmentManeuver(DateTime.MinValue,
                TimeSpan.Zero, targetOrbit, spacecraft2.Engines.First()))
            .SetNextManeuver(new PhasingManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, 1,
                spacecraft2.Engines.First()))
            .SetNextManeuver(new ApogeeHeightManeuver(DateTime.MinValue, TimeSpan.Zero, 15866666.666666666,
                spacecraft2.Engines.First()));

        spacecraft2.SetStandbyManeuver(planeAlignmentManeuver);
        scenario.AddSpacecraft(spacecraft2);
        var summary = scenario.Simulate(Constants.OutputPath);
        CosmographiaExporter cosmographiaExporter = new CosmographiaExporter();
        await cosmographiaExporter.ExportAsync(scenario, new DirectoryInfo("CosmographiaExport"));
    }
}