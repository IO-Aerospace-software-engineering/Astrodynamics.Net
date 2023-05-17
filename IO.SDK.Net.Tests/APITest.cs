using System;
using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;
using Xunit;
using System.Linq;
using IO.SDK.Net.SolarSystemObjects;

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
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        double start = api.ConvertUTCToTDB(667915130.814600);
        double end = api.ConvertUTCToTDB(668174330.814560);


        CelestialBody celestialBody =
            new CelestialBody(id: PlanetsAndMoons.EARTH.NaifId, centerOfMotionId: Stars.Sun.NaifId);

        Site launchSite = new Site(id: 399303, bodyId: PlanetsAndMoons.EARTH.NaifId,
            coordinates: new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), name: "S3",
            directoryPath: SitePath);

        Site recoverySite = new Site(id: 399304, bodyId: PlanetsAndMoons.EARTH.NaifId,
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
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        double start = api.ConvertUTCToTDB(667915269.18539762);
        double startPropagator = api.ConvertUTCToTDB(668085555.829810);
        double end = api.ConvertUTCToTDB(668174400.000000);

        var scenario = new Scenario("titi", new Window(startPropagator, end));
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[1].CenterOfMotionId = Stars.Sun.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;
        scenario.CelestialBodies[2].CenterOfMotionId = PlanetsAndMoons.EARTH.NaifId;

        Site launchSite = new Site(399303, PlanetsAndMoons.EARTH.NaifId,
            new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0),
            "S33",
            SitePath);

        Site recoverySite = new Site(399304, PlanetsAndMoons.EARTH.NaifId,
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

        Assert.Equal("2021-03-04 00:32:42.854653 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 00:32:51.175821 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 00:32:42.854653 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 00:32:51.175821 (TDB)",
            api.TDBToString(scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.End));
        Assert.Equal(8.321168541908264,
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(-96.249682169636841, 106.8756958946026, -118.85490552843048),
            scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].DeltaV);

        Assert.Equal(416.0584252471169, scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0].FuelBurned);

        Assert.Equal("2021-03-04 01:15:43.938777 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 01:16:06.412865 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 01:15:43.938777 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 01:16:06.412865 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.End));
        Assert.Equal(22.474087476730347,
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(-463.86026824667442, -168.446133153132, 236.66179218359866),
            scenario.Spacecraft.ApsidalAlignmentManeuvers[0].DeltaV);

        Assert.Equal(1123.704373112356, scenario.Spacecraft.ApsidalAlignmentManeuvers[0].FuelBurned);

        Assert.Equal("2021-03-04 01:16:14.640093 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 04:59:25.401665 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 01:16:14.640093 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 01:16:24.185636 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.End));
        Assert.Equal(9.54554295539856,
            scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.End -
            scenario.Spacecraft.PhasingManeuverDto[0].ThrustWindow.Start);

        Assert.Equal(
            new Vector3D(-139.74832471889238, 85.585884339402824, 194.98547637537283),
            scenario.Spacecraft.PhasingManeuverDto[0].DeltaV);

        Assert.Equal(477.2771488397447, scenario.Spacecraft.PhasingManeuverDto[0].FuelBurned);

        Assert.Equal("2021-03-04 05:24:43.893746 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 05:24:52.477527 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 05:24:43.893746 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 05:24:52.477527 (TDB)",
            api.TDBToString(scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.End));
        Assert.Equal(8.583780884742737,
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.End -
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].ThrustWindow.Start);
        Assert.Equal(new Vector3D(134.61136287435605, -81.418092113142166, -184.29863264945666),
            scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].DeltaV);

        Assert.Equal(429.18904439256715, scenario.Spacecraft.ApogeeHeightChangingManeuvers[0].FuelBurned);
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
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.TDBToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (TDB)", res);

        res = api.TDBToString(100.0);
        Assert.Equal("2000-01-01 12:01:40.000000 (TDB)", res);
    }

    [Fact]
    public void UTCToString()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.UTCToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (UTC)", res);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraintProxy()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.FindWindowsOnDistanceConstraint(new Window(220881665.18391809, 228657665.18565452),
            PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.MOON.NaifId, ">",
            400000000, "NONE", 86400.0);
        Assert.Equal(4, res.Length);
        Assert.Equal("2007-01-08 00:11:07.628591 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2007-01-13 06:37:47.948144 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2007-03-29 22:53:58.151896 (TDB)", api.TDBToString(res[3].Start));
        Assert.Equal("2007-04-01 00:01:05.185654 (TDB)", api.TDBToString(res[3].End));
    }

    [Fact]
    public void FindWindowsOnOccultationConstraint()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.FindWindowsOnOccultationConstraint(new Window(61473664.183390938, 61646464.183445148),
            PlanetsAndMoons.EARTH.NaifId, Stars.Sun.NaifId,
            "IAU_SUN", "Ellipsoid", PlanetsAndMoons.MOON.NaifId,
            "IAU_MOON",
            "Ellipsoid", "ANY", "LT", 3600.0);
        Assert.Single(res);
        Assert.Equal("2001-12-14 20:10:15.410588 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2001-12-14 21:35:49.100520 (TDB)", api.TDBToString(res[0].End));
    }

    [Fact]
    public void FindWindowsOnCoordinateConstraint()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.FindWindowsOnCoordinateConstraint(new Window(730036800.0, 730123200), 399013,
            PlanetsAndMoons.MOON.NaifId, GroundStations.DSS_13.Frame,
            "LATITUDINAL", "LATITUDE",
            ">",
            0.0, 0.0, "NONE", 60.0);

        Assert.Single(res);
        Assert.Equal("2023-02-19 14:33:08.918098 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2023-02-20 00:00:00.000000 (TDB)", api.TDBToString(res[0].End));
    }

    [Fact]
    public void FindWindowsOnIlluminationConstraint()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        var res = api.FindWindowsOnIlluminationConstraint(new Window(674524800, 674611200), Stars.Sun.NaifId,
            Stars.Sun.Name, PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.EARTH.Frame,
            new Geodetic(2.2 * Constants.DEG_RAD, 48.0 * Constants.DEG_RAD, 0.0),
            "INCIDENCE",
            "<", Math.PI * 0.5 - (-0.8 * Constants.DEG_RAD), 0.0, "CN+S", 60.0 * 60.0 * 4.5, "Ellipsoid");
        Assert.Equal(2, res.Length);
        Assert.Equal("2021-05-17 12:00:00.000000 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2021-05-17 19:35:42.885022 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2021-05-18 04:18:50.060742 (TDB)", api.TDBToString(res[1].Start));
        Assert.Equal("2021-05-18 12:00:00.000000 (TDB)", api.TDBToString(res[1].End));
    }

    [Fact]
    public void FindWindowsInFieldOfViewConstraint()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);

        double start = api.ConvertUTCToTDB(676555130.80);
        double end = api.ConvertUTCToTDB(start + 6448.0);

        var scenario = new Scenario("titi", new Window(start, end));
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[1].CenterOfMotionId = Stars.Sun.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;
        scenario.CelestialBodies[2].CenterOfMotionId = PlanetsAndMoons.EARTH.NaifId;

        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], start, "J2000",
            new Vector3D(6800000.0, 0.0, 0.0),
            new Vector3D(0.0, 7656.2204182967143, 0.0));


        scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 3000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.Instruments[0] = new Instrument(600, "CAM600", "circular", new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0), 1.5, double.NaN);

        api.ExecuteScenario(ref scenario);
        api.LoadKernels("Data/User/Spacecrafts/DRAGONFLY");
        var res = api.FindWindowsInFieldOfViewConstraint(new Window(676555200, 676561647), -178, 600,
            PlanetsAndMoons.EARTH.NaifId,
            "IAU_EARTH", "Ellipsoid",
            "LT", 3600.0);
        Assert.Equal(2, res.Length);
        Assert.Equal("2021-06-10 00:00:00.000000 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2021-06-10 00:30:12.445632 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2021-06-10 01:02:53.829736 (TDB)", api.TDBToString(res[1].Start));
        Assert.Equal("2021-06-10 01:47:27.000000 (TDB)", api.TDBToString(res[1].End));
    }

    [Fact]
    public void ReadEphemeris()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        Window searchWindow = new Window(0.0, 100.0);
        var res = api.ReadEphemeris(searchWindow, 399, 301, "J2000", "LT", 10.0);

        Assert.Equal(-291569264.48965073, res[0].Position.X);
        Assert.Equal(-266709187.1624887, res[0].Position.Y);
        Assert.Equal(-76099155.244104564, res[0].Position.Z);
        Assert.Equal(643.53061483971885, res[0].Velocity.X);
        Assert.Equal(-666.08181440799092, res[0].Velocity.Y);
        Assert.Equal(-301.32283209101018, res[0].Velocity.Z);
        Assert.Equal(399, res[0].CenterOfMotion.Id);
        Assert.Equal(10, res[0].CenterOfMotion.CenterOfMotionId);
        Assert.Equal("J2000", res[0].Frame);
        Assert.Equal(0.0, res[0].Epoch);
    }

    [Fact]
    public void ReadOrientation()
    {
        API api = new API();
        // Window utcSearchWindow = new Window(662777930.816060, 662777990.816060);
        Window tdbSearchWindow = new Window(662778000.0 + 0.0, 662778060.0);

        api.LoadKernels(SolarSystemKernelPath);

        var scenario = new Scenario("ReadOrientation", tdbSearchWindow);
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[1].CenterOfMotionId = Stars.Sun.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;
        scenario.CelestialBodies[2].CenterOfMotionId = PlanetsAndMoons.EARTH.NaifId;

        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], tdbSearchWindow.Start, "J2000",
            new Vector3D(6800000.0, 0.0, 0.0),
            new Vector3D(0.0, 7656.2204182967143, 0.0));


        scenario.Spacecraft = new Spacecraft(-1782, "DRAGONFLY2", 1000.0, 10000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] =
            new FuelTank(id: 1, capacity: 9000.0, quantity: 9000.0, serialNumber: "fuelTank1");
        scenario.Spacecraft.Engines[0] = new EngineDTO(id: 1, name: "engine1", fuelFlow: 50,
            serialNumber: "serialNumber1", fuelTankSerialNumber: "fuelTank1", isp: 450);

        scenario.Spacecraft.nadirAttitudes[0] = new NadirAttitude(0, 0.0, double.MinValue)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        api.ExecuteScenario(ref scenario);
        api.LoadKernels("Data/User/Spacecrafts/DRAGONFLY2");

        var res = api.ReadOrientation(tdbSearchWindow, -1782, Math.Pow(2, 16), "J2000", 10.0);

        Assert.Equal(0.7071067811865476, res[0].Orientation.W);
        Assert.Equal(0.0, res[0].Orientation.X);
        Assert.Equal(0.0, res[0].Orientation.Y);
        Assert.Equal(-0.7071067811865475, res[0].Orientation.Z);
        Assert.Equal(0.0, res[0].AngularVelocity.X);
        Assert.Equal(0.0, res[0].AngularVelocity.Y);
        Assert.Equal(0.0, res[0].AngularVelocity.Z);
        Assert.Equal(tdbSearchWindow.Start, res[0].Epoch);
        Assert.Equal("J2000", res[0].Frame);
    }

    [Fact]
    void ConvertTDBToUTC()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        Assert.Equal(-64.18392726322381, api.ConvertTDBToUTC(0.0));
    }

    [Fact]
    void ConverUTCToTDB()
    {
        API api = new API();
        api.LoadKernels(SolarSystemKernelPath);
        Assert.Equal(64.18392728466942, api.ConvertUTCToTDB(0.0));
    }
}