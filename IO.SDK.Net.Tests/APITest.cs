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
        api.LoadGenericKernel(SolarSystemKernelPath);
        CelestialBody celestialBody = new CelestialBody(id: 399, centerOfMotionId: 10);

        Site launchSite = new Site(id: 399303, bodyId: 399,
            coordinates: new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), name: "S3",
            directoryPath: SitePath);

        Site recoverySite = new Site(id: 399304, bodyId: 399,
            coordinates: new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), name: "S4",
            directoryPath: SitePath);


        StateVector parkingOrbit = new StateVector(celestialBody, start, "J2000",
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        Launch launch = new Launch(launchSite, recoverySite, true, 1, parkingOrbit, new Window(start, end));
        api.FindLaunchWindows(ref launch);
        Assert.Equal(2, launch.Windows.Count(x => x.Start != 0 && x.End != 0));
        Assert.Equal(new Window(668084955.97088385, 668084955.97088385), launch.Windows[0]);
        Assert.Equal(new Window(668171119.44731534, 668171119.44731534), launch.Windows[1]);
        Assert.Equal(47.00587579161426, launch.InertialAzimuth * Constants.RAD_DEG);
        Assert.Equal(45.125224583051406, launch.NonInertialAzimuth * Constants.RAD_DEG);
        Assert.Equal(8794.33812148836, launch.InertialInsertionVelocity);
        Assert.Equal(8499.727158006212, launch.NonInertialInsertionVelocity);
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
        double start = 667915269.18539762;
        double startPropagator = 668085555.829810;
        double end = 668174400.000000;
        API api = new API();
        api.LoadGenericKernel(SolarSystemKernelPath);
        var scenario = new Scenario("titi", new Window(startPropagator, end));
        scenario.CelestialBodies[0].Id = 10;
        scenario.CelestialBodies[1].Id = 399;
        scenario.CelestialBodies[1].CenterOfMotionId = 10;
        scenario.CelestialBodies[2].Id = 301;
        scenario.CelestialBodies[2].CenterOfMotionId = 399;

        Site launchSite = new Site(399303, 399, new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            "S33",
            SitePath);

        Site recoverySite = new Site(399304, 399,
            new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), "S44", SitePath);

        scenario.Sites[0] = launchSite;
        scenario.Sites[1] = recoverySite;

        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], start, "J2000",
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        StateVector target = new StateVector(scenario.CelestialBodies[1], start, "J2000",
            new Vector3D(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3D(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265));


        scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] =
            new FuelTank(id: 1, capacity: 9000.0, quantity: 9000.0, serialNumber: "fuelTank1");
        scenario.Spacecraft.Engines[0] = new EngineDTO(id: 1, name: "engine1", fuelFlow: 50,
            serialNumber: "serialNumber1", fuelTankSerialNumber: "fuelTank1", isp: 450);
        scenario.Spacecraft.Payloads[0] = new Payload("PAY01", "Payload 01", 50.0);
        scenario.Spacecraft.Instruments[0] = new Instrument(600, "CAM600", "circular", new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0), 80.0 * Constants.DEG_RAD, double.NaN);


        scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0] = new OrbitalPlaneChangingManeuver(0, 0.0, 0.0, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        scenario.Spacecraft.ApsidalAlignmentManeuvers[0] = new ApsidalAlignmentManeuver(1, 0.0, int.MinValue, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        scenario.Spacecraft.PhasingManeuverDto[0] = new PhasingManeuver(2, 0.0, double.MinValue, 1, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        scenario.Spacecraft.ApogeeHeightChangingManeuvers[0] =
            new ApogeeHeightChangingManeuver(3, 0.0, double.MinValue, 15866666.666666666)
            {
                Engines =
                {
                    [0] = "serialNumber1"
                }
            };


        api.ExecuteScenario(ref scenario);

        Assert.Equal("2021-03-04 00:31:35.852044 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 00:31:44.178429 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 00:31:35.852044 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 00:31:44.178429 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.End));
        Assert.Equal(8.326384663581848,
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(-96.310132502235291, 106.94716089267334, -118.92923688022945),
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].DeltaV);
        Assert.Equal(416.3192335596316, scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].FuelBurned);

        Assert.Equal("2021-03-04 01:14:35.908715 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 01:14:58.448143 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 01:14:35.908715 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 01:14:58.448143 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.End));
        Assert.Equal(22.539427876472473,
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(-465.43237249809499, -170.79467001831654, 235.85199549814843),
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].DeltaV);
        Assert.Equal(1126.971395681102, scenario.Spacecraft.ApsidalAlignmentManeuvers[0].FuelBurned);

        Assert.Equal("2021-03-04 01:15:06.675929 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 04:58:19.564061 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 01:15:06.675929 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 01:15:16.220357 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.End));
        Assert.Equal(9.544427156448364,
            scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.End -
            scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(-140.06591089740422, 85.292644161629369, 194.98809324843614),
            scenario.Spacecraft.PhasingManeuverDto[0].DeltaV);
        Assert.Equal(477.2213550574124, scenario.Spacecraft.PhasingManeuverDto[0].FuelBurned);

        Assert.Equal("2021-03-04 05:23:34.930489 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 05:23:43.510224 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 05:23:34.930489 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 05:23:43.510224 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.End));
        Assert.Equal(8.5797358751297,
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(134.75015047648623, -81.245837445793484, -184.26044152450118),
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].DeltaV);
        Assert.Equal(428.98679499077326, scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].FuelBurned);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(18816, size);
    }

    [Fact]
    public void TDBToString()
    {
        API api = new API();
        api.LoadGenericKernel(SolarSystemKernelPath);
        var res = api.TDBToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (TDB)", res);

        res = api.TDBToString(100.0);
        Assert.Equal("2000-01-01 12:01:40.000000 (TDB)", res);
    }

    [Fact]
    public void UTCToString()
    {
        API api = new API();
        api.LoadGenericKernel(SolarSystemKernelPath);
        var res = api.UTCToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (UTC)", res);
    }
}