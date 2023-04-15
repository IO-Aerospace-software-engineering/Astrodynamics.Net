using System;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;
using Xunit;
using System.Linq;

namespace IO.SDK.Net.Tests;

public class APITest
{
    const string SpacecraftPath = "Data/User/Spacecrafts";
    const string SolarSystemKernelPath = "Data/SolarSystem";
    const string SitePath = "Data/User/Sites";

    [Fact]
    public void CheckVersion()
    {
        API api = new API();
        Assert.Equal("CSPICE_N0067", api.GetSpiceVersion());
    }

    [Fact]
    public void ExecuteLaunchScenario()
    {
        // Start
        //
        // 2021-03-02 00:00:00.000000 TDB
        //
        // 667915130.814600
        // End
        //
        // 2021-03-05 00:00:00.000000 TDB
        //
        // 668174330.814560
        double start = 667915130.814600;
        double end = 668174330.814560;
        API api = new API();
        API.LoadGenericKernelsProxy(SolarSystemKernelPath);
        CelestialBody celestialBody = new CelestialBody() { Id = 399, CenterOfMotionId = 10 };

        Site launchSite = new Site()
        {
            Id = 3, BodyId = 399, Coordinates = new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            Name = "S3", DirectoryPath = SitePath
        };

        Site recoverySite = new Site()
        {
            Id = 4, BodyId = 399, Coordinates = new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            Name = "S4", DirectoryPath = SitePath
        };


        StateVector parkingOrbit = new StateVector(celestialBody, start, "J2000",
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        StateVector target = new StateVector(celestialBody, start, "J2000",
            new Vector3D(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3D(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265));


        Launch launch = new Launch(launchSite, recoverySite, true, 1, parkingOrbit, new Window(start, end));
        API.LaunchProxy(ref launch);
        Assert.Equal(2, launch.Windows.Count(x => x.Start != 0 && x.End != 0));
        Assert.Equal(new Window(668084955.97088385, 668084955.97088385), launch.Windows[0]);
        Assert.Equal(new Window(668171119.44731534, 668171119.44731534) { }, launch.Windows[1]);
    }

    [Fact]
    public void ExecuteReachOrbitScenario()
    {
        // Start
        //
        // 2021-03-02 00:00:00.000000 TDB
        //
        // 667915130.814600
        // End
        //
        // 2021-03-05 00:00:00.000000 TDB
        //
        // 668174330.814560
        double start = 667915130.814600;
        double startPropagator = 668085625.01523638;
        double end = 668174330.814560;
        API api = new API();
        API.LoadGenericKernelsProxy(SolarSystemKernelPath);
        var scenario = new Scenario("titi", new Window(startPropagator, end));
        scenario.CelestialBodies[0].Id = 10;
        scenario.CelestialBodies[1].Id = 399;
        scenario.CelestialBodies[1].CenterOfMotionId = 10;
        scenario.CelestialBodies[2].Id = 301;
        scenario.CelestialBodies[2].CenterOfMotionId = 399;

        Site launchSite = new Site()
        {
            Id = 3, BodyId = 399, Coordinates = new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            Name = "S3", DirectoryPath = SitePath
        };

        Site recoverySite = new Site()
        {
            Id = 4, BodyId = 399, Coordinates = new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            Name = "S4", DirectoryPath = SitePath
        };


        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], start, "J2000",
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        StateVector target = new StateVector(scenario.CelestialBodies[1], start, "J2000",
            new Vector3D(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3D(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265));


        scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] = new FuelTank
            { Id = 1, Capacity = 9000.0, Quantity = 9000.0, SerialNumber = "fuelTank1" };
        scenario.Spacecraft.Engines[0] = new EngineDTO
        {
            Id = 1, Name = "engine1", Fuelflow = 50, SerialNumber = "serialNumber1", FuelTankSerialNumber = "fuelTank1",
            Isp = 450
        };
        scenario.Spacecraft.Payloads[0] = new Payload("PAY01", "Payload 01", 50.0);

        scenario.Spacecraft.Instruments[0].Name = "CAM600";
        scenario.Spacecraft.Instruments[0].Id = 600;
        scenario.Spacecraft.Instruments[0].Boresight.X = 0.0;
        scenario.Spacecraft.Instruments[0].Boresight.Y = 0.0;
        scenario.Spacecraft.Instruments[0].Boresight.Z = 1.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.X = 1.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.Y = 0.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.Z = 0.0;
        scenario.Spacecraft.Instruments[0].Orientation.X = 1.0;
        scenario.Spacecraft.Instruments[0].Orientation.Y = 0.0;
        scenario.Spacecraft.Instruments[0].Orientation.Z = 0.0;
        scenario.Spacecraft.Instruments[0].FieldOfView = 80.0 * Constants.DEG_RAD;
        scenario.Spacecraft.Instruments[0].Shape = "circular";

        scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ManeuverOrder = 0;
        scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].Engines[0] = Marshal.StringToHGlobalAnsi("serialNumber1");
        scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].TargetOrbit = target;

        scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ManeuverOrder = 1;
        scenario.Spacecraft.ApsidalAlignmentManeuvers[0].Engines[0] = Marshal.StringToHGlobalAnsi("serialNumber1");
        scenario.Spacecraft.ApsidalAlignmentManeuvers[0].TargetOrbit = target;

        scenario.Spacecraft.PhasingManeuverDto[0].ManeuverOrder = 2;
        scenario.Spacecraft.PhasingManeuverDto[0].Engines[0] = Marshal.StringToHGlobalAnsi("serialNumber1");
        scenario.Spacecraft.PhasingManeuverDto[0].TargetOrbit = target;
        scenario.Spacecraft.PhasingManeuverDto[0].NumberRevolutions = 1;

        scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ManeuverOrder = 3;
        scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].Engines[0] = Marshal.StringToHGlobalAnsi("serialNumber1");
        scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].TargetHeight = 15866666.666666666;


        API.PropagateProxy(ref scenario);
    }

    [Fact]
    public void BuildMinimumScenario()
    {
        API api = new API();
        API.LoadGenericKernelsProxy(SolarSystemKernelPath);
        var scenario = new Scenario("titi", new Window(10.0, 20.0));
        scenario.CelestialBodies[0].Id = 399;
        scenario.CelestialBodies[0].CenterOfMotionId = 10;
        scenario.CelestialBodies[1].Id = 10;
        scenario.Spacecraft = new Spacecraft(-1111, "spc1", 1000.0, 3000.0,
            new StateVector(centerOfMotion: new CelestialBody() { CenterOfMotionId = 10, Id = 399 }, epoch: 15.0,
                frame: "J2000", position: new Vector3D(x: 6800.0, y: 0.0, z: 0.0),
                velocity: new Vector3D(x: 0.0, y: 8.0, z: 0.0)), SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] = new FuelTank
            { Id = 1, Capacity = 1000.0, Quantity = 1000.0, SerialNumber = "ft1" };
        scenario.Spacecraft.Engines[0] = new EngineDTO
            { Id = 1, Name = "engine1", Fuelflow = 50, SerialNumber = "eng1", FuelTankSerialNumber = "ft1", Isp = 400 };
        scenario.Spacecraft.progradeAttitudes[0].Engines = new string[10];
        scenario.Spacecraft.progradeAttitudes[0].Engines[0] = "eng1";


        API.PropagateProxy(ref scenario);
    }

    [Fact]
    public void BuildScenarioAttitudes()
    {
        API api = new API();
        API.LoadGenericKernelsProxy(SolarSystemKernelPath);
        var scenario = new Scenario("titi", new Window(10.0, 20.0));

        scenario.CelestialBodies = new CelestialBody[10];
        scenario.CelestialBodies[0].Id = 399;
        scenario.CelestialBodies[0].CenterOfMotionId = 10;
        scenario.CelestialBodies[1].Id = 10;

        scenario.Spacecraft = new Spacecraft(-1111, "spc1", 1000.0, 3000.0,
            new StateVector(centerOfMotion: new CelestialBody() { CenterOfMotionId = 10, Id = 399 }, epoch: 15.0,
                frame: "J2000", position: new Vector3D(x: 6800.0, y: 0.0, z: 0.0),
                velocity: new Vector3D(x: 0.0, y: 8.0, z: 0.0)), SpacecraftPath);

        scenario.Spacecraft.Instruments[0].Name = "inst1";
        scenario.Spacecraft.Instruments[0].Id = 200;
        scenario.Spacecraft.Instruments[0].Boresight.X = 1.0;
        scenario.Spacecraft.Instruments[0].Boresight.Y = 0.0;
        scenario.Spacecraft.Instruments[0].Boresight.Z = 0.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.X = 0.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.Y = 1.0;
        scenario.Spacecraft.Instruments[0].FovRefVector.Z = 0.0;
        scenario.Spacecraft.Instruments[0].FieldOfView = 3.14;
        scenario.Spacecraft.Instruments[0].Shape = "circular";

        scenario.Spacecraft.FuelTanks[0].Id = 1;
        scenario.Spacecraft.FuelTanks[0].Capacity = 1000.0;
        scenario.Spacecraft.FuelTanks[0].Quantity = 1000.0;
        scenario.Spacecraft.FuelTanks[0].SerialNumber = "ft1";

        scenario.Spacecraft.Engines[0].Id = 1;
        scenario.Spacecraft.Engines[0].SerialNumber = "eng1";
        scenario.Spacecraft.Engines[0].FuelTankSerialNumber = "ft1";
        scenario.Spacecraft.Engines[0].Fuelflow = 50;
        scenario.Spacecraft.Engines[0].Isp = 400;
        scenario.Spacecraft.Engines[0].Name = "engine1";

        scenario.Spacecraft.progradeAttitudes[0].Engines[0] = "eng1";
        scenario.Spacecraft.progradeAttitudes[0].ManeuverOrder = 0;

        scenario.Spacecraft.retrogradeAttitudes[0].Engines[0] = Marshal.StringToHGlobalAnsi("eng1");
        scenario.Spacecraft.retrogradeAttitudes[0].ManeuverOrder = 1;

        scenario.Spacecraft.nadirAttitudes[0].Engines[0] = Marshal.StringToHGlobalAnsi("eng1");
        scenario.Spacecraft.nadirAttitudes[0].ManeuverOrder = 2;

        scenario.Spacecraft.zenithAttitudes[0].Engines[0] = Marshal.StringToHGlobalAnsi("eng1");
        scenario.Spacecraft.zenithAttitudes[0].ManeuverOrder = 3;

        scenario.Spacecraft.PointingToAttitudes[0].Engines[0] = Marshal.StringToHGlobalAnsi("eng1");
        scenario.Spacecraft.PointingToAttitudes[0].ManeuverOrder = 4;
        scenario.Spacecraft.PointingToAttitudes[0].TargetBodyId = 399;
        scenario.Spacecraft.PointingToAttitudes[0].InstrumentId = 200;


        API.PropagateProxy(ref scenario);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(810776, size);
    }
}