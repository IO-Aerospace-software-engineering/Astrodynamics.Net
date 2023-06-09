using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using System.Linq;
using IO.Astrodynamics.Converters;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Tests;
using IO.Astrodynamics.Models.Time;
using IO.Astrodynamics.SolarSystemObjects;
using ApsidalAlignmentManeuver = IO.Astrodynamics.DTO.ApsidalAlignmentManeuver;
using FuelTank = IO.Astrodynamics.DTO.FuelTank;
using Instrument = IO.Astrodynamics.DTO.Instrument;
using Payload = IO.Astrodynamics.DTO.Payload;
using PhasingManeuver = IO.Astrodynamics.DTO.PhasingManeuver;
using Quaternion = IO.Astrodynamics.Models.Math.Quaternion;
using Scenario = IO.Astrodynamics.DTO.Scenario;
using Site = IO.Astrodynamics.DTO.Site;
using Spacecraft = IO.Astrodynamics.DTO.Spacecraft;
using Window = IO.Astrodynamics.DTO.Window;

namespace IO.Astrodynamics.Tests;

public class APITest
{
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
        api.LoadKernels(Constants.SolarSystemKernelPath);

        var start = DateTime.Parse("2021-03-02 00:00:00.000000").ToTDB();
        var end = DateTime.Parse("2021-03-05 00:00:00.000000").ToTDB();

        Models.Time.Window window = new Models.Time.Window(start, end);

        //Define launch site
        LaunchSite launchSite = new LaunchSite(399303, "S3", TestHelpers.GetEarthAtJ2000(),
            new Models.Coordinates.Geodetic(-81.0 * Constants.DEG_RAD, 28.5 * Constants.DEG_RAD, 0.0));

        //Define the targeted parking orbit
        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.GetEarthAtJ2000(), start, Frame.ICRF);

        //Create launch object
        Models.Maneuver.Launch launch = new Models.Maneuver.Launch(launchSite, launchSite, parkingOrbit, Models.Constants.CivilTwilight, true);

        //Find launch windows
        var res = api.FindLaunchWindows(launch, window, Constants.OutputPath);

        //Read results
        Assert.Equal(2, res.Count());
        Assert.Equal(new Models.Time.Window(DateTime.Parse("2021-03-03 23:09:15.971").ToTDB(), DateTime.Parse("2021-03-03 23:09:15.971").ToTDB()), res.ElementAt(0).Window);
        Assert.Equal(new Models.Time.Window(DateTime.Parse("2021-03-04 23:05:19.447").ToTDB(), DateTime.Parse("2021-03-04 23:05:19.447").ToTDB()), res.ElementAt(1).Window);
        Assert.Equal(47.00587579161426, res.ElementAt(0).InertialAzimuth * Constants.RAD_DEG);
        Assert.Equal(45.125224583051406, res.ElementAt(0).NonInertialAzimuth * Constants.RAD_DEG);
        Assert.Equal(8794.33812148836, res.ElementAt(0).InertialInsertionVelocity);
        Assert.Equal(8499.727158006212, res.ElementAt(0).NonInertialInsertionVelocity);
    }

    [Fact]
    public void ExecuteReachOrbitScenario()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
        DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
        DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

        Mission mission = new Mission("mission01");
        Models.Mission.Scenario scenario = new Models.Mission.Scenario("scn1", mission, new Models.Time.Window(startPropagator, end));
        scenario.AddBody(TestHelpers.GetSun());
        scenario.AddBody(TestHelpers.GetEarthAtJ2000());
        scenario.AddBody(TestHelpers.GetMoonAtJ2000());

        //Define parking orbit
        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.GetEarthAtJ2000(), start, Frame.ICRF);

        //Define target orbit
        Models.OrbitalParameters.StateVector targetOrbit = new Models.OrbitalParameters.StateVector(new Vector3(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265), TestHelpers.GetEarthAtJ2000(), start, Frame.ICRF);

        //Create and configure spacecraft
        Models.Body.Spacecraft.Spacecraft spacecraft = new Models.Body.Spacecraft.Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0);
        Clock clock = new Clock("clk1", Math.Pow(2.0, 16.0));
        SpacecraftScenario spacecraftScenario = new SpacecraftScenario(spacecraft, clock, parkingOrbit, scenario);
        Models.Body.Spacecraft.FuelTank fuelTank = new Models.Body.Spacecraft.FuelTank("ft1", "model1", 9000.0);
        Engine engine = new Engine("engine1", "model1", 450.0, 50.0);
        spacecraftScenario.AddFuelTank(fuelTank, 9000.0, "fuelTank1");
        spacecraftScenario.AddEngine(engine, fuelTank, "serialNumber1");
        spacecraftScenario.AddPayload(new Models.Body.Spacecraft.Payload("payload1", 50.0, "pay01"));
        spacecraftScenario.AddInstrument(new Models.Body.Spacecraft.Instrument(600, "CAM600", "mod1", 80.0 * Constants.DEG_RAD, InstrumentShape.Circular),
            new Quaternion(1.0, 0.0, 0.0, 0.0));

        var planeAlignmentManeuver = new PlaneAlignmentManeuver(spacecraftScenario, DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraftScenario.Engines.First());
        planeAlignmentManeuver
            .SetNextManeuver(new Models.Maneuver.ApsidalAlignmentManeuver(spacecraftScenario, DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraftScenario.Engines.First()))
            .SetNextManeuver(new Models.Maneuver.PhasingManeuver(spacecraftScenario, DateTime.MinValue, TimeSpan.Zero, targetOrbit, 1, spacecraftScenario.Engines.First()))
            .SetNextManeuver(new Models.Maneuver.ApogeeHeightManeuver(spacecraftScenario, DateTime.MinValue, TimeSpan.Zero, 15866666.666666666,
                spacecraftScenario.Engines.First()));
        spacecraftScenario.SetStandbyManeuver(planeAlignmentManeuver);

        api.PropagateScenario(scenario, Constants.OutputPath);

        // Read maneuver results
        var maneuver = spacecraftScenario.StandbyManeuver;
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.322, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-96.24969153329532, 106.87570557408036, -118.8549175756141), ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(416.05846464958046, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:15:43.9380000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:06.4120000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:15:43.9380000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:06.4120000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(22.4740000, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-463.85710999496314, -168.44268760441446, 236.66234186526253), ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(1123.6976200120396, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:16:14.6390000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T04:59:25.4030000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:14.6390000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:24.1840000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(9.545, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-139.7485096203384, 85.58601299692951, 194.985748375168), ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(477.27816776049883, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T05:24:43.8920000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:52.4760000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:43.8920000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:52.4760000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.584, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(134.61069118237498, -81.41939868308344, -184.2992402533224), ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(429.19025843695215, maneuver.FuelBurned);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(18776, size);
    }

    [Fact]
    public void TDBToString()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

        var res = api.UTCToString(0.0);
        Assert.Equal("2000-01-01 12:00:00.000000 (UTC)", res);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraintProxy()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

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

    // [Fact]
    // public void FindWindowsInFieldOfViewConstraint()
    // {
    //     //Initialize API
    //     API api = new API();
    //
    //     //Load solar system kernels
    //     api.LoadKernels(Constants.SolarSystemKernelPath);
    //
    //     double start = api.ConvertUTCToTDB(676555130.80);
    //     double end = api.ConvertUTCToTDB(start + 6448.0);
    //
    //     //Configure scenario
    //     var scenario = new Scenario("Scenario_A", new Window(start, end));
    //     scenario.CelestialBodiesId[0] = Stars.Sun.NaifId;
    //     scenario.CelestialBodiesId[1] = PlanetsAndMoons.EARTH.NaifId;
    //     scenario.CelestialBodiesId[2] = PlanetsAndMoons.MOON.NaifId;
    //
    //     //Define parking orbit
    //     StateVector parkingOrbit = new StateVector(PlanetsAndMoons.EARTH.NaifId, start,
    //         InertialFrame.ICRF.GetDescription(),
    //         new Vector3D(6800000.0, 0.0, 0.0),
    //         new Vector3D(0.0, 7656.2204182967143, 0.0));
    //
    //
    //     //Configure spacecraft
    //     scenario.Spacecraft = new Spacecraft(-178, "DRAGONFLY", 1000.0, 3000.0, parkingOrbit, Constants.SpacecraftPath.FullName);
    //     scenario.Spacecraft.Instruments[0] = new Instrument(600, "CAM600", InstrumentShape.Circular.ToString(),
    //         new Vector3D(1.0, 0.0, 0.0),
    //         new Vector3D(0.0, 0.0, 1.0), new Vector3D(1.0, 0.0, 0.0), 1.5, double.NaN);
    //
    //     //Execute scenario
    //     api.ExecuteScenario(ref scenario);
    //
    //     //Load generated kernels
    //     api.LoadKernels(new DirectoryInfo("Data/User/Spacecrafts/DRAGONFLY"));
    //
    //     //Find windows when the earth is in field of view of camera 600 
    //     var res = api.FindWindowsInFieldOfViewConstraint(new Window(676555200, 676561647), -178, 600,
    //         PlanetsAndMoons.EARTH.NaifId,
    //         PlanetsAndMoons.EARTH.Frame, ShapeType.Ellipsoid,
    //         Aberration.LT, TimeSpan.FromHours(1.0));
    //
    //     //Read results
    //     Assert.Equal(2, res.Length);
    //     Assert.Equal("2021-06-10 00:00:00.000000 (TDB)", api.TDBToString(res[0].Start));
    //     Assert.Equal("2021-06-10 00:30:12.445511 (TDB)", api.TDBToString(res[0].End));
    //     Assert.Equal("2021-06-10 01:02:53.829783 (TDB)", api.TDBToString(res[1].Start));
    //     Assert.Equal("2021-06-10 01:47:27.000000 (TDB)", api.TDBToString(res[1].End));
    // }

    [Fact]
    public void ReadEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        Window searchWindow = new Window(0.0, 100.0);
        var res = api.ReadEphemeris(searchWindow, PlanetsAndMoons.EARTH.NaifId, PlanetsAndMoons.MOON.NaifId,
            InertialFrame.ICRF.GetDescription(), Aberration.LT, TimeSpan.FromSeconds(10.0));

        Assert.Equal(-291569264.48965073, res[0].Position.X);
        Assert.Equal(-266709187.1624887, res[0].Position.Y);
        Assert.Equal(-76099155.244104564, res[0].Position.Z);
        Assert.Equal(643.53061483971885, res[0].Velocity.X);
        Assert.Equal(-666.08181440799092, res[0].Velocity.Y);
        Assert.Equal(-301.32283209101018, res[0].Velocity.Z);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, res[0].CenterOfMotionId);
        Assert.Equal(InertialFrame.ICRF.GetDescription(), res[0].Frame);
        Assert.Equal(0.0, res[0].Epoch);
    }

    // [Fact]
    // public void ReadOrientation()
    // {
    //     //Initialize API
    //     API api = new API();
    //
    //     //Load solar system kernels
    //     api.LoadKernels(Constants.SolarSystemKernelPath);
    //     // Window utcSearchWindow = new Window(662777930.816060, 662777990.816060);
    //     Window tdbSearchWindow = new Window(662778000.0 + 0.0, 662778060.0);
    //
    //     //Configure scenario
    //     var scenario = new Scenario("ReadOrientation", tdbSearchWindow);
    //     scenario.CelestialBodiesId[0] = Stars.Sun.NaifId;
    //     scenario.CelestialBodiesId[1] = PlanetsAndMoons.EARTH.NaifId;
    //     scenario.CelestialBodiesId[2] = PlanetsAndMoons.MOON.NaifId;
    //
    //     //Configure parking orbit
    //     StateVector parkingOrbit = new StateVector(PlanetsAndMoons.EARTH.NaifId, tdbSearchWindow.Start,
    //         InertialFrame.ICRF.GetDescription(),
    //         new Vector3D(6800000.0, 0.0, 0.0),
    //         new Vector3D(0.0, 7656.2204182967143, 0.0));
    //
    //     //Configure spacecraft
    //     scenario.Spacecraft = new Spacecraft(-1782, "DRAGONFLY2", 1000.0, 10000.0, parkingOrbit, Constants.SpacecraftPath.FullName);
    //     scenario.Spacecraft.FuelTanks[0] =
    //         new FuelTank(id: 1, capacity: 9000.0, quantity: 9000.0, serialNumber: "fuelTank1");
    //     scenario.Spacecraft.Engines[0] = new EngineDTO(id: 1, name: "engine1", fuelFlow: 50,
    //         serialNumber: "serialNumber1", fuelTankSerialNumber: "fuelTank1", isp: 450);
    //
    //     //Spacecraft must point to nadir
    //     scenario.Spacecraft.NadirAttitudes[0] = new NadirAttitude(0, 0.0, double.MinValue)
    //     {
    //         Engines =
    //         {
    //             [0] = "serialNumber1"
    //         }
    //     };
    //
    //     //Execute scenario
    //     api.ExecuteScenario(ref scenario);
    //
    //     //Load generated kernels
    //     api.LoadKernels(new DirectoryInfo("Data/User/Spacecrafts/DRAGONFLY2"));
    //
    //     //Read spacecraft orientation
    //     var res = api.ReadOrientation(tdbSearchWindow, -1782, Math.Pow(2, 16), InertialFrame.ICRF.GetDescription(),
    //         TimeSpan.FromSeconds(10.0));
    //
    //     //Read results
    //     Assert.Equal(0.7071067811865476, res[0].Orientation.W);
    //     Assert.Equal(0.0, res[0].Orientation.X);
    //     Assert.Equal(0.0, res[0].Orientation.Y);
    //     Assert.Equal(-0.7071067811865475, res[0].Orientation.Z);
    //     Assert.Equal(0.0, res[0].AngularVelocity.X);
    //     Assert.Equal(0.0, res[0].AngularVelocity.Y);
    //     Assert.Equal(0.0, res[0].AngularVelocity.Z);
    //     Assert.Equal(tdbSearchWindow.Start, res[0].Epoch);
    //     Assert.Equal(InertialFrame.ICRF.GetDescription(), res[0].Frame);
    // }

    [Fact]
    void ConvertTDBToUTC()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);
        Assert.Equal(-64.18392726322381, api.ConvertTDBToUTC(0.0));
    }

    [Fact]
    void ConverUTCToTDB()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);
        Assert.Equal(64.18392728466942, api.ConvertUTCToTDB(0.0));
    }

    [Fact]
    void WriteEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);
        const int size = 10;
        StateVector[] sv = new StateVector[size];
        for (int i = 0; i < size; ++i)
        {
            sv[i] = new StateVector(PlanetsAndMoons.EARTH.NaifId, i, InertialFrame.ICRF.GetDescription(),
                new Vector3D(6800 + i, i, i), new Vector3D(i, 8.0 + i * 0.001, i));
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
            Assert.Equal(PlanetsAndMoons.EARTH.NaifId, svResult[i].CenterOfMotionId);
            Assert.Equal(InertialFrame.ICRF.GetDescription(), svResult[i].Frame);
        }
    }

    [Fact]
    void GetCelestialBodyInformation()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

        //Get the quaternion to transform
        var res = api.TransformFrame(Frame.ICRF, new Frame(PlanetsAndMoons.EARTH.Frame), DateTimeExtension.J2000);
        Assert.Equal(0.76713121189662548, res.Rotation.W);
        Assert.Equal(-1.8618846012434252e-05, res.Rotation.VectorPart.X);
        Assert.Equal(8.468919252183845e-07, res.Rotation.VectorPart.Y);
        Assert.Equal(0.64149022080358797, res.Rotation.VectorPart.Z);
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
        api.LoadKernels(Constants.SolarSystemKernelPath);

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

        var sv = api.ConvertToStateVector(eqDTO);

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
        api.LoadKernels(Constants.SolarSystemKernelPath);

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

        var sv = api.ConvertToStateVector(conicElements);

        Assert.Equal(-6119034.915639279, sv.Position.X, 8);
        Assert.Equal(-1546800.4544009243, sv.Position.Y, 6);
        Assert.Equal(2522970.821362097, sv.Position.Z, 8);
        Assert.Equal(-807.6748840709542, sv.Velocity.X, 8);
        Assert.Equal(-5476.5381803473965, sv.Velocity.Y, 8);
        Assert.Equal(-5296.561721841285, sv.Velocity.Z, 8);
        Assert.Equal(663724800.00001490, sv.Epoch);
        Assert.Equal(399, sv.CenterOfMotionId);
        Assert.Equal(InertialFrame.ICRF.GetDescription(), sv.Frame);
    }

    [Fact]
    void ConvertStateVectorToRaDec()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        var sv = new StateVector(PlanetsAndMoons.EARTH.NaifId, 0.0, InertialFrame.ICRF.GetDescription(),
            new Vector3D(-291608384.63344, -266716833.39423, -76102487.09990),
            new Vector3D(643.53139, -666.08768, -301.32570));

        var radec = api.ConvertToEquatorialCoordinates(sv);
        Assert.Equal(222.4472994995566, radec.RightAscencion * Constants.RAD_DEG);
        Assert.Equal(-10.900186051699306, radec.Declination * Constants.RAD_DEG);
        Assert.Equal(402448639.887328, radec.Radius);
    }

    [Fact]
    void ConvertConicOrbitalElementsToRaDec()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        var conics = new ConicElements();
        conics.CenterOfMotionId = PlanetsAndMoons.EARTH.NaifId;
        conics.Epoch = 0.0;
        conics.Frame = InertialFrame.ICRF.GetDescription();
        conics.PerifocalDistance = 365451161.74144;
        conics.Eccentricity = 0.05357474;
        conics.Inclination = 20.94230395 * Constants.DEG_RAD;
        conics.AscendingNodeLongitude = 12.23643846 * Constants.DEG_RAD;
        conics.PeriapsisArgument = 68.05335129 * Constants.DEG_RAD;
        conics.MeanAnomaly = 140.14966394 * Constants.DEG_RAD;

        var radec = api.ConvertToEquatorialCoordinates(conics);
        Assert.Equal(222.4472992707561, radec.RightAscencion * Constants.RAD_DEG);
        Assert.Equal(-10.900185977212049, radec.Declination * Constants.RAD_DEG);
        Assert.Equal(402448637.2542864, radec.Radius);
    }
}