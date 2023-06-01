using System;
using System.IO;
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
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Convert UTC time to TDB
        double start = api.ConvertUTCToTDB(667915130.814600);
        double end = api.ConvertUTCToTDB(668174330.814560);

        //Define the celestial body from which the launch will occur
        CelestialBody celestialBody =
            new CelestialBody(id: PlanetsAndMoons.EARTH.NaifId, centerOfMotionId: Stars.Sun.NaifId, "",
                new Vector3D(0.0, 0.0, 0.0), 0.0, "", 0, "");

        //Define launch site
        Site launchSite = new Site(id: 399303, bodyId: PlanetsAndMoons.EARTH.NaifId,
            coordinates: new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), name: "S3",
            directoryPath: SitePath);

        //Define recovery site
        Site recoverySite = new Site(id: 399304, bodyId: PlanetsAndMoons.EARTH.NaifId,
            coordinates: new Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0), name: "S4",
            directoryPath: SitePath);

        //Define the targeted parking orbit
        StateVector parkingOrbit = new StateVector(celestialBody, start, InertialFrame.ICRF.GetDescription(),
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        //Create launch object
        Launch launch = new Launch(launchSite, recoverySite, true, 1, parkingOrbit, new Window(start, end));

        //Find launch windows
        api.FindLaunchWindows(ref launch);

        //Read results
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
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Define some keys datetime
        double start = api.ConvertUTCToTDB(667915269.18539762);
        double startPropagator = api.ConvertUTCToTDB(668085555.829810);
        double end = api.ConvertUTCToTDB(668174400.000000);

        //Create and configure scenario
        var scenario = new Scenario("titi", new Window(startPropagator, end));
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;

        //Define parking orbit
        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], start,
            InertialFrame.ICRF.GetDescription(),
            new Vector3D(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3D(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494));

        //Define target orbit
        StateVector target = new StateVector(scenario.CelestialBodies[1], start, InertialFrame.ICRF.GetDescription(),
            new Vector3D(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3D(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265));


        //Create and configure spacecraft
        scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] =
            new FuelTank(id: 1, capacity: 9000.0, quantity: 9000.0, serialNumber: "fuelTank1");
        scenario.Spacecraft.Engines[0] = new EngineDTO(id: 1, name: "engine1", fuelFlow: 50,
            serialNumber: "serialNumber1", fuelTankSerialNumber: "fuelTank1", isp: 450);
        scenario.Spacecraft.Payloads[0] = new Payload("PAY01", "Payload 01", 50.0);
        scenario.Spacecraft.Instruments[0] = new Instrument(600, "CAM600", InstrumentShape.Circular.ToString(),
            new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0), 80.0 * Constants.DEG_RAD, double.NaN);


        //Configure the OrbitalPlaneChangingManeuver
        scenario.Spacecraft.OrbitalPlaneChangingManeuvers[0] = new OrbitalPlaneChangingManeuver(0, 0.0, 0.0, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        //Configure the ApsidalAlignmentManeuver
        scenario.Spacecraft.ApsidalAlignmentManeuvers[0] = new ApsidalAlignmentManeuver(1, 0.0, int.MinValue, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        //Configure the PhasingManeuver
        scenario.Spacecraft.PhasingManeuver[0] = new PhasingManeuver(2, 0.0, double.MinValue, 1, target)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        //Configure ApogeeHeightChangingManeuver
        scenario.Spacecraft.ApogeeHeightChangingManeuvers[0] =
            new ApogeeHeightChangingManeuver(3, 0.0, double.MinValue, 15866666.666666666)
            {
                Engines =
                {
                    [0] = "serialNumber1"
                }
            };

        api.ExecuteScenario(ref scenario);

        //Read maneuver results
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
            api.TDBToString(scenario.Spacecraft.PhasingManeuver[0].ManeuverWindow.Start));
        Assert.Equal("2021-03-04 04:59:25.401665 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuver[0].ManeuverWindow.End));
        Assert.Equal("2021-03-04 01:16:14.640093 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuver[0].ThrustWindow.Start));
        Assert.Equal("2021-03-04 01:16:24.185636 (TDB)",
            api.TDBToString(scenario.Spacecraft.PhasingManeuver[0].ThrustWindow.End));
        Assert.Equal(9.54554295539856,
            scenario.Spacecraft.PhasingManeuver[0].ThrustWindow.End -
            scenario.Spacecraft.PhasingManeuver[0].ThrustWindow.Start);

        Assert.Equal(
            new Vector3D(-139.74832471889238, 85.585884339402824, 194.98547637537283),
            scenario.Spacecraft.PhasingManeuver[0].DeltaV);

        Assert.Equal(477.2771488397447, scenario.Spacecraft.PhasingManeuver[0].FuelBurned);

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
        Assert.Equal(21440, size);
    }

    [Fact]
    public void TDBToString()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        var res = api.TDBToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (TDB)", res);

        res = api.TDBToString(100.0);
        Assert.Equal("2000-01-01 12:01:40.000000 (TDB)", res);
    }

    [Fact]
    public void UTCToString()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        var res = api.UTCToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (UTC)", res);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraintProxy()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Find time windows when the moon will be 400000 km away from the Earth
        var res = api.FindWindowsOnDistanceConstraint(new Window(220881665.18391809, 228657665.18565452),
            PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.MOON.NaifId, RelationnalOperator.Greater,
            400000000, Aberration.None, TimeSpan.FromSeconds(86400.0));
        Assert.Equal(4, res.Length);
        Assert.Equal("2007-01-08 00:11:07.628591 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2007-01-13 06:37:47.948144 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2007-03-29 22:53:58.151896 (TDB)", api.TDBToString(res[3].Start));
        Assert.Equal("2007-04-01 00:01:05.185654 (TDB)", api.TDBToString(res[3].End));
    }

    [Fact]
    public void FindWindowsOnOccultationConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Find time windows when the Sun will be occulted by the moon
        var res = api.FindWindowsOnOccultationConstraint(new Window(61473664.183390938, 61646464.183445148),
            PlanetsAndMoons.EARTH.NaifId, Stars.Sun.NaifId,
            Stars.Sun.Frame, ShapeType.Ellipsoid, PlanetsAndMoons.MOON.NaifId,
            PlanetsAndMoons.MOON.Frame,
            ShapeType.Ellipsoid, OccultationType.Any, Aberration.LT, TimeSpan.FromSeconds(3600.0));
        Assert.Single(res);
        Assert.Equal("2001-12-14 20:10:15.410588 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2001-12-14 21:35:49.100520 (TDB)", api.TDBToString(res[0].End));
    }

    [Fact]
    public void FindWindowsOnCoordinateConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Find time windows when the moon will be above the horizon relative to Deep Space Station 13
        var res = api.FindWindowsOnCoordinateConstraint(new Window(730036800.0, 730123200), 399013,
            PlanetsAndMoons.MOON.NaifId, GroundStations.DSS_13.Frame,
            CoordinateSystem.Latitudinal, Coordinate.Latitude,
            RelationnalOperator.Greater,
            0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0));

        Assert.Single(res);
        Assert.Equal("2023-02-19 14:33:08.918098 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2023-02-20 00:00:00.000000 (TDB)", api.TDBToString(res[0].End));
    }

    [Fact]
    public void FindWindowsOnIlluminationConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Find time windows when the geodetic point is illuminated by the sun (Official twilight 0.8° bellow horizon)
        var res = api.FindWindowsOnIlluminationConstraint(new Window(674524800, 674611200), Stars.Sun.NaifId,
            PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.EARTH.Frame,
            new Geodetic(2.2 * Constants.DEG_RAD, 48.0 * Constants.DEG_RAD, 0.0),
            IlluminationAngle.Incidence,
            RelationnalOperator.Lower, Math.PI * 0.5 - (-0.8 * Constants.DEG_RAD), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5));
        Assert.Equal(2, res.Length);
        Assert.Equal("2021-05-17 12:00:00.000000 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2021-05-17 19:35:24.908834 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2021-05-18 04:18:32.443750 (TDB)", api.TDBToString(res[1].Start));
        Assert.Equal("2021-05-18 12:00:00.000000 (TDB)", api.TDBToString(res[1].End));
    }

    [Fact]
    public void FindWindowsInFieldOfViewConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        double start = api.ConvertUTCToTDB(676555130.80);
        double end = api.ConvertUTCToTDB(start + 6448.0);

        //Configure scenario
        var scenario = new Scenario("Scenario_A", new Window(start, end));
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;

        //Define parking orbit
        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], start,
            InertialFrame.ICRF.GetDescription(),
            new Vector3D(6800000.0, 0.0, 0.0),
            new Vector3D(0.0, 7656.2204182967143, 0.0));


        //Configure spacecraft
        scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 3000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.Instruments[0] = new Instrument(600, "CAM600", InstrumentShape.Circular.ToString(),
            new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0), 1.5, double.NaN);

        //Execute scenario
        api.ExecuteScenario(ref scenario);

        //Load generated kernels
        api.LoadKernels(new DirectoryInfo("Data/User/Spacecrafts/DRAGONFLY"));

        //Find windows when the earth is in field of view of camera 600 
        var res = api.FindWindowsInFieldOfViewConstraint(new Window(676555200, 676561647), -178, 600,
            PlanetsAndMoons.EARTH.NaifId,
            PlanetsAndMoons.EARTH.Frame, ShapeType.Ellipsoid,
            Aberration.LT, TimeSpan.FromHours(1.0));

        //Read results
        Assert.Equal(2, res.Length);
        Assert.Equal("2021-06-10 00:00:00.000000 (TDB)", api.TDBToString(res[0].Start));
        Assert.Equal("2021-06-10 00:30:12.445511 (TDB)", api.TDBToString(res[0].End));
        Assert.Equal("2021-06-10 01:02:53.829783 (TDB)", api.TDBToString(res[1].Start));
        Assert.Equal("2021-06-10 01:47:27.000000 (TDB)", api.TDBToString(res[1].End));
    }

    [Fact]
    public void ReadEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        Window searchWindow = new Window(0.0, 100.0);
        var res = api.ReadEphemeris(searchWindow, PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.MOON.NaifId,
            InertialFrame.ICRF.GetDescription(), Aberration.LT, TimeSpan.FromSeconds(10.0));

        Assert.Equal(-291569264.48965073, res[0].Position.X);
        Assert.Equal(-266709187.1624887, res[0].Position.Y);
        Assert.Equal(-76099155.244104564, res[0].Position.Z);
        Assert.Equal(643.53061483971885, res[0].Velocity.X);
        Assert.Equal(-666.08181440799092, res[0].Velocity.Y);
        Assert.Equal(-301.32283209101018, res[0].Velocity.Z);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, res[0].CenterOfMotion.Id);
        Assert.Equal(Stars.Sun.NaifId, res[0].CenterOfMotion.CenterOfMotionId);
        Assert.Equal(InertialFrame.ICRF.GetDescription(), res[0].Frame);
        Assert.Equal(0.0, res[0].Epoch);
    }

    [Fact]
    public void ReadOrientation()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));
        // Window utcSearchWindow = new Window(662777930.816060, 662777990.816060);
        Window tdbSearchWindow = new Window(662778000.0 + 0.0, 662778060.0);

        //Configure scenario
        var scenario = new Scenario("ReadOrientation", tdbSearchWindow);
        scenario.CelestialBodies[0].Id = Stars.Sun.NaifId;
        scenario.CelestialBodies[1].Id = PlanetsAndMoons.EARTH.NaifId;
        scenario.CelestialBodies[2].Id = PlanetsAndMoons.MOON.NaifId;

        //Configure parking orbit
        StateVector parkingOrbit = new StateVector(scenario.CelestialBodies[1], tdbSearchWindow.Start,
            InertialFrame.ICRF.GetDescription(),
            new Vector3D(6800000.0, 0.0, 0.0),
            new Vector3D(0.0, 7656.2204182967143, 0.0));

        //Configure spacecraft
        scenario.Spacecraft = new Spacecraft(-1782, "DRAGONFLY2", 1000.0, 10000.0, parkingOrbit, SpacecraftPath);
        scenario.Spacecraft.FuelTanks[0] =
            new FuelTank(id: 1, capacity: 9000.0, quantity: 9000.0, serialNumber: "fuelTank1");
        scenario.Spacecraft.Engines[0] = new EngineDTO(id: 1, name: "engine1", fuelFlow: 50,
            serialNumber: "serialNumber1", fuelTankSerialNumber: "fuelTank1", isp: 450);

        //Spacecraft must point to nadir
        scenario.Spacecraft.nadirAttitudes[0] = new NadirAttitude(0, 0.0, double.MinValue)
        {
            Engines =
            {
                [0] = "serialNumber1"
            }
        };

        //Execute scenario
        api.ExecuteScenario(ref scenario);

        //Load generated kernels
        api.LoadKernels(new DirectoryInfo("Data/User/Spacecrafts/DRAGONFLY2"));

        //Read spacecraft orientation
        var res = api.ReadOrientation(tdbSearchWindow, -1782, Math.Pow(2, 16), InertialFrame.ICRF.GetDescription(),
            TimeSpan.FromSeconds(10.0));

        //Read results
        Assert.Equal(0.7071067811865476, res[0].Orientation.W);
        Assert.Equal(0.0, res[0].Orientation.X);
        Assert.Equal(0.0, res[0].Orientation.Y);
        Assert.Equal(-0.7071067811865475, res[0].Orientation.Z);
        Assert.Equal(0.0, res[0].AngularVelocity.X);
        Assert.Equal(0.0, res[0].AngularVelocity.Y);
        Assert.Equal(0.0, res[0].AngularVelocity.Z);
        Assert.Equal(tdbSearchWindow.Start, res[0].Epoch);
        Assert.Equal(InertialFrame.ICRF.GetDescription(), res[0].Frame);
    }

    [Fact]
    void ConvertTDBToUTC()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));
        Assert.Equal(-64.18392726322381, api.ConvertTDBToUTC(0.0));
    }

    [Fact]
    void ConverUTCToTDB()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));
        Assert.Equal(64.18392728466942, api.ConvertUTCToTDB(0.0));
    }

    [Fact]
    void WriteEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));
        const int size = 10;
        StateVector[] sv = new StateVector[size];
        for (int i = 0; i < size; ++i)
        {
            sv[i].Position.X = 6800 + i;
            sv[i].Position.Y = i;
            sv[i].Position.Z = i;
            sv[i].Velocity.X = i;
            sv[i].Velocity.Y = 8.0 + i * 0.001;
            sv[i].Velocity.Z = i;
            sv[i].Epoch = i;
            sv[i].CenterOfMotion.Id = PlanetsAndMoons.EARTH.NaifId;
            sv[i].Frame = InertialFrame.ICRF.GetDescription();
        }

        //Write ephemeris file
        FileInfo file = new FileInfo("EphemerisTestFile.spk");
        api.WriteEphemeris(file, -135, sv, size);

        //Load ephemeris file
        api.LoadKernels(file);

        Window window = new Window(0.0, 9.0);
        var svResult = api.ReadEphemeris(window, PlanetsAndMoons.EARTH.NaifId, -135,
            InertialFrame.ICRF.GetDescription(), Aberration.None, TimeSpan.FromSeconds(1.0));
        for (int i = 0; i < size; ++i)
        {
            Assert.Equal(6800.0 + i, svResult[i].Position.X);
            Assert.Equal(i, svResult[i].Position.Y, 12);
            Assert.Equal(i, svResult[i].Position.Z, 12);
            Assert.Equal(i, svResult[i].Velocity.X, 12);
            Assert.Equal(8 + i * 0.001, svResult[i].Velocity.Y, 12);
            Assert.Equal(i, svResult[i].Velocity.Z, 12);
            Assert.Equal(i, svResult[i].Epoch);
            Assert.Equal(PlanetsAndMoons.EARTH.NaifId, svResult[i].CenterOfMotion.Id);
            Assert.Equal(Stars.Sun.NaifId, svResult[i].CenterOfMotion.CenterOfMotionId);
            Assert.Equal(InertialFrame.ICRF.GetDescription(), svResult[i].Frame);
        }
    }

    [Fact]
    void GetCelestialBodyInformation()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Read celestial body information from spice kernels
        var res = api.GetCelestialBodyInfo(PlanetsAndMoons.EARTH.NaifId);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, res.Id);
        Assert.Equal(Stars.Sun.NaifId, res.CenterOfMotionId);
        Assert.Equal(PlanetsAndMoons.EARTH.Name, res.Name);
        Assert.Equal("", res.Error);
        Assert.Equal(13000, res.FrameId);
        Assert.Equal("ITRF93", res.FrameName);
        Assert.Equal(398600.43543609593, res.GM);
        Assert.Equal(6378.1365999999998, res.Radii.X);
        Assert.Equal(6378.1365999999998, res.Radii.Y);
        Assert.Equal(6356.7519000000002, res.Radii.Z);
    }

    [Fact]
    void TransformFrame()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        //Get the quaternion to transform
        var res = api.TransformFrame(InertialFrame.ICRF.GetDescription(), PlanetsAndMoons.EARTH.Frame, 0.0);
        Assert.Equal(0.76713121189662548, res.Rotation.W);
        Assert.Equal(-1.8618846012434252e-05, res.Rotation.X);
        Assert.Equal(8.468919252183845e-07, res.Rotation.Y);
        Assert.Equal(0.64149022080358797, res.Rotation.Z);
        Assert.Equal(-1.9637714059853662e-09, res.AngularVelocity.X);
        Assert.Equal(-2.0389340573814659e-09, res.AngularVelocity.Y);
        Assert.Equal(7.2921150642488516e-05, res.AngularVelocity.Z);
    }

    [Fact]
    void ConvertEquinoctialElementsToStateVector()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        double p = 1.0e7;
        double ecc = 0.1;
        double a = p / (1.0 - ecc);
        double argp = 30.0 * Constants.DEG_RAD;
        double node = 15.0 * Constants.DEG_RAD;
        double inc = 10.0 * Constants.DEG_RAD;
        double m0 = 45.0 * Constants.DEG_RAD;

        //equinoctial elements
        double h = ecc * Math.Sin(argp + node);
        double k = ecc * Math.Cos(argp + node);
        double p2 = Math.Tan(inc / 2.0) * Math.Sin(node);
        double q = Math.Tan(inc / 2.0) * Math.Cos(node);
        double L = m0 + argp + node;

        EquinoctialElements eqDTO = new EquinoctialElements();
        eqDTO.Frame = InertialFrame.ICRF.GetDescription();
        eqDTO.DeclinationOfThePole = Math.PI * 0.5;
        eqDTO.RightAscensionOfThePole = -Math.PI * 0.5;
        eqDTO.AscendingNodeLongitudeRate = 0.0;
        eqDTO.PeriapsisLongitudeRate = 0.0;
        eqDTO.H = h;
        eqDTO.P = p2;
        eqDTO.SemiMajorAxis = a;
        eqDTO.Epoch = 10.0;
        eqDTO.CenterOfMotionId = 399;
        eqDTO.L = L;
        eqDTO.K = k;
        eqDTO.Q = q;

        var sv = api.ConvertEquinoctialElementsToStateVector(eqDTO);

        Assert.Equal(-1557343.2179623565, sv.Position.X);
        Assert.Equal(10112046.56492505, sv.Position.Y);
        Assert.Equal(1793343.6111546031, sv.Position.Z);
        Assert.Equal(-6369.0795341145204, sv.Velocity.X);
        Assert.Equal(-517.51239201161684, sv.Velocity.Y);
        Assert.Equal(202.52220483204573, sv.Velocity.Z);
    }

    [Fact]
    void ConvertConicElementsToStateVector()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(new DirectoryInfo(SolarSystemKernelPath));

        double perifocalDist = Math.Sqrt(Math.Pow(-6.116559469556896E+06, 2) + Math.Pow(-1.546174698676721E+06, 2) +
                                         Math.Pow(2.521950157430313E+06, 2));

        ConicElements conicElements = new ConicElements();
        conicElements.Frame = InertialFrame.ICRF.GetDescription();
        conicElements.Epoch = 663724800.00001490; //"2021-01-12T11:58:50.816" UTC
        conicElements.MeanAnomaly = 4.541224977546975E+01 * Constants.DEG_RAD;
        conicElements.PeriapsisArgument = 1.062574316262159E+02 * Constants.DEG_RAD;
        conicElements.AscendingNodeLongitude = 3.257605322534260E+01 * Constants.DEG_RAD;
        conicElements.Inclination = 5.171921958517460E+01 * Constants.DEG_RAD;
        conicElements.Eccentricity = 1.353139738203394E-03;
        conicElements.PerifocalDistance = perifocalDist;
        conicElements.CenterOfMotionId = 399;

        var sv = api.ConvertConicElementsToStateVector(conicElements);

        Assert.Equal(-6119034.915639279, sv.Position.X);
        Assert.Equal(-1546800.4544009243, sv.Position.Y);
        Assert.Equal(2522970.821362097, sv.Position.Z);
        Assert.Equal(-807.6748840709542, sv.Velocity.X);
        Assert.Equal(-5476.5381803473965, sv.Velocity.Y);
        Assert.Equal(-5296.561721841285, sv.Velocity.Z);
        Assert.Equal(663724800.00001490, sv.Epoch);
        Assert.Equal(399, sv.CenterOfMotion.Id);
        Assert.Equal(InertialFrame.ICRF.GetDescription(), sv.Frame);
    }
}