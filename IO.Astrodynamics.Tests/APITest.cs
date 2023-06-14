using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using System.Linq;
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
using NadirAttitude = IO.Astrodynamics.Models.Maneuver.NadirAttitude;
using Scenario = IO.Astrodynamics.Models.Mission.Scenario;
using Window = IO.Astrodynamics.Models.Time.Window;

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
        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(
            new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.GetEarthAtJ2000(),
            start, Frame.ICRF);

        //Create launch object
        Models.Maneuver.Launch launch = new Models.Maneuver.Launch(launchSite, launchSite, parkingOrbit,
            Models.Constants.CivilTwilight, true);

        //Find launch windows
        var res = api.FindLaunchWindows(launch, window, Constants.OutputPath).ToArray();

        //Read results
        Assert.Equal(2, res.Count());
        Assert.Equal(
            new Models.Time.Window(DateTime.Parse("2021-03-03 23:09:15.971").ToTDB(),
                DateTime.Parse("2021-03-03 23:09:15.971").ToTDB()), res.ElementAt(0).Window);
        Assert.Equal(
            new Models.Time.Window(DateTime.Parse("2021-03-04 23:05:19.447").ToTDB(),
                DateTime.Parse("2021-03-04 23:05:19.447").ToTDB()), res.ElementAt(1).Window);
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
        Scenario scenario =
            new Scenario("scn1", mission, new Models.Time.Window(startPropagator, end));
        scenario.AddBody(TestHelpers.GetSun());
        scenario.AddBody(TestHelpers.GetEarthAtJ2000());
        scenario.AddBody(TestHelpers.GetMoonAtJ2000());

        //Define parking orbit
        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(
            new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.GetEarthAtJ2000(),
            start, Frame.ICRF);

        //Define target orbit
        Models.OrbitalParameters.StateVector targetOrbit = new Models.OrbitalParameters.StateVector(
            new Vector3(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265), TestHelpers.GetEarthAtJ2000(),
            start, Frame.ICRF);

        //Create and configure spacecraft
        Clock clock = new Clock("clk1", Math.Pow(2.0, 16.0));
        Models.Body.Spacecraft.Spacecraft spacecraft =
            new Models.Body.Spacecraft.Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0,clock,parkingOrbit);
        
        Models.Body.Spacecraft.FuelTank fuelTank = new Models.Body.Spacecraft.FuelTank("ft1", "model1", 9000.0);
        Engine engine = new Engine("engine1", "model1", 450.0, 50.0);
        spacecraft.AddFuelTank(fuelTank, 9000.0, "fuelTank1");
        spacecraft.AddEngine(engine, fuelTank, "serialNumber1");
        spacecraft.AddPayload(new Models.Body.Spacecraft.Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(
            new Models.Body.Spacecraft.Instrument(600, "CAM600", "mod1", 80.0 * Constants.DEG_RAD,
                InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX), Vector3.VectorX);

        var planeAlignmentManeuver = new PlaneAlignmentManeuver(spacecraft, DateTime.MinValue, TimeSpan.Zero,
            targetOrbit, spacecraft.Engines.First());
        planeAlignmentManeuver
            .SetNextManeuver(new Models.Maneuver.ApsidalAlignmentManeuver(spacecraft, DateTime.MinValue,
                TimeSpan.Zero, targetOrbit, spacecraft.Engines.First()))
            .SetNextManeuver(new Models.Maneuver.PhasingManeuver(spacecraft, DateTime.MinValue, TimeSpan.Zero,
                targetOrbit, 1, spacecraft.Engines.First()))
            .SetNextManeuver(new ApogeeHeightManeuver(spacecraft, DateTime.MinValue,
                TimeSpan.Zero, 15866666.666666666,
                spacecraft.Engines.First()));
        spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

        api.PropagateScenario(scenario, Constants.OutputPath);

        // Read maneuver results
        var maneuver = spacecraft.StandbyManeuver;
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.322, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-96.24969153329532, 106.87570557408036, -118.8549175756141),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(416.05846464958046, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:15:43.9380000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:06.4120000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:15:43.9380000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:06.4120000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(22.4740000, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-463.85710999496314, -168.44268760441446, 236.66234186526253),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(1123.6976200120396, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:16:14.6390000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T04:59:25.4030000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:14.6390000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:24.1840000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(9.545, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-139.7485096203384, 85.58601299692951, 194.985748375168),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(477.27816776049883, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T05:24:43.8920000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:52.4760000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:43.8920000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:52.4760000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.584, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(134.61069118237498, -81.41939868308344, -184.2992402533224),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(429.19025843695215, maneuver.FuelBurned);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new DTO.Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(18776, size);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraintProxy()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        //Find time windows when the moon will be 400000 km away from the Earth
        var res = api.FindWindowsOnDistanceConstraint(new Models.Time.Window(DateTimeExtension.CreateTDB(220881665.18391809), DateTimeExtension.CreateTDB(228657665.18565452)),
            TestHelpers.GetEarthAtJ2000(), TestHelpers.GetMoonAtJ2000(), RelationnalOperator.Greater, 400000000, Aberration.None,
            TimeSpan.FromSeconds(86400.0));
        var windows = res as Models.Time.Window[] ?? res.ToArray();
        Assert.Equal(4, windows.Count());
        Assert.Equal("2007-01-08T00:11:07.6290000 (TDB)", windows.ElementAt(0).StartDate.ToFormattedString());
        Assert.Equal("2007-01-13T06:37:47.9480000 (TDB)", windows.ElementAt(0).EndDate.ToFormattedString());
        Assert.Equal("2007-03-29T22:53:58.1520000 (TDB)", windows.ElementAt(3).StartDate.ToFormattedString());
        Assert.Equal("2007-04-01T00:01:05.1860000 (TDB)", windows.ElementAt(3).EndDate.ToFormattedString());
    }

    [Fact]
    public void FindWindowsOnOccultationConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        //Find time windows when the Sun will be occulted by the moon
        var res = api.FindWindowsOnOccultationConstraint(new Models.Time.Window(DateTimeExtension.CreateTDB(61473664.183390938), DateTimeExtension.CreateTDB(61646464.183445148)),
            TestHelpers.GetEarthAtJ2000(), TestHelpers.GetSun(), ShapeType.Ellipsoid, TestHelpers.GetMoonAtJ2000(), ShapeType.Ellipsoid,
            OccultationType.Any, Aberration.LT, TimeSpan.FromSeconds(3600.0));
        var windows = res as Models.Time.Window[] ?? res.ToArray();
        Assert.Single(windows);
        Assert.Equal("2001-12-14T20:10:15.4110000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2001-12-14T21:35:49.1010000 (TDB)", windows[0].EndDate.ToFormattedString());
    }

    [Fact]
    public void FindWindowsOnCoordinateConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        Models.Surface.Site site = new Models.Surface.Site(13, "station13", TestHelpers.GetEarthAtJ2000(),
            new Models.Coordinates.Geodetic(-116.7944627147624 * Models.Constants.Deg2Rad, 35.2471635434595 * Models.Constants.Deg2Rad, 0.107));
        //Find time windows when the moon will be above the horizon relative to Deep Space Station 13
        var res = api.FindWindowsOnCoordinateConstraint(new Models.Time.Window(DateTimeExtension.CreateTDB(730036800.0), DateTimeExtension.CreateTDB(730123200)), site,
            TestHelpers.GetMoonAtJ2000(), new Frame(GroundStations.DSS_13.Frame), CoordinateSystem.Latitudinal, Coordinate.Latitude, RelationnalOperator.Greater,
            0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0));

        var windows = res as Models.Time.Window[] ?? res.ToArray();
        Assert.Single(windows);
        Assert.Equal("2023-02-19T14:33:08.9180000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2023-02-20T00:00:00.0000000 (TDB)", windows[0].EndDate.ToFormattedString());
    }

    [Fact]
    public void FindWindowsOnIlluminationConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        //Find time windows when the geodetic point is illuminated by the sun (Official twilight 0.8° bellow horizon)
        var res = api.FindWindowsOnIlluminationConstraint(new Models.Time.Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            TestHelpers.GetSun(), TestHelpers.GetEarthAtJ2000(), new Frame("ITRF93"),
            new Geodetic(2.2 * Constants.DEG_RAD, 48.0 * Constants.DEG_RAD, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower, Math.PI * 0.5 - (-0.8 * Constants.DEG_RAD), 0.0, Aberration.CNS, TimeSpan.FromHours(4.5));
        var windows = res as Models.Time.Window[] ?? res.ToArray();
        Assert.Equal(2, windows.Count());
        Assert.Equal("2021-05-17T12:00:00.0000000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2021-05-17T19:35:24.9090000 (TDB)", windows[0].EndDate.ToFormattedString());
        Assert.Equal("2021-05-18T04:18:32.4440000 (TDB)", windows[1].StartDate.ToFormattedString());
        Assert.Equal("2021-05-18T12:00:00.0000000 (TDB)", windows[1].EndDate.ToFormattedString());
    }

    [Fact]
    public void FindWindowsInFieldOfViewConstraint()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        DateTime start = DateTimeExtension.CreateUTC(676555130.80).ToTDB();
        DateTime end = start.AddSeconds(6448.0);

        //Configure scenario
        Scenario scenario = new Scenario("Scenario_A", new Mission("mission01"),
            new Models.Time.Window(start, end));
        scenario.AddBody(TestHelpers.GetSun());
        scenario.AddBody(TestHelpers.GetEarthAtJ2000());
        scenario.AddBody(TestHelpers.GetMoonAtJ2000());

        //Define parking orbit

        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(
            new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.GetEarthAtJ2000(),
            start, Frame.ICRF);

        //Configure spacecraft
        Clock clock = new Clock("clk1", Math.Pow(2.0, 16.0));
        Models.Body.Spacecraft.Spacecraft spacecraft =
            new Models.Body.Spacecraft.Spacecraft(-178, "DRAGONFLY", 1000.0, 10000.0,clock,parkingOrbit);
        
        Models.Body.Spacecraft.FuelTank fuelTank = new Models.Body.Spacecraft.FuelTank("ft1", "model1", 9000.0);
        Engine engine = new Engine("engine1", "model1", 450.0, 50.0);
        spacecraft.AddFuelTank(fuelTank, 9000.0, "fuelTank1");
        spacecraft.AddEngine(engine, fuelTank, "serialNumber1");
        spacecraft.AddPayload(new Models.Body.Spacecraft.Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(new Models.Body.Spacecraft.Instrument(600, "CAM600", "mod1", 1.5,
            InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX), Vector3.VectorX);


        //Execute scenario
        api.PropagateScenario(scenario, Constants.OutputPath);

        //Load generated kernels
        api.LoadKernels(new DirectoryInfo(Path.Combine(Constants.OutputPath.FullName, "Spacecrafts/DRAGONFLY")));

        //Find windows when the earth is in field of view of camera 600 
        var res = api.FindWindowsInFieldOfViewConstraint(new Models.Time.Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
            spacecraft.Intruments.First().Instrument,
            TestHelpers.GetEarthAtJ2000(),
            new Frame(PlanetsAndMoons.EARTH.Frame), ShapeType.Ellipsoid,
            Aberration.LT, TimeSpan.FromHours(1.0)).ToArray();

        //Read results
        Assert.Equal(2, res.Count());
        Assert.Equal("2021-06-10T00:00:00.0000000 (TDB)", res.ElementAt(0).StartDate.ToFormattedString());
        Assert.Equal("2021-06-10T00:30:12.4450000 (TDB)", res.ElementAt(0).EndDate.ToFormattedString());
        Assert.Equal("2021-06-10T01:02:53.8290000 (TDB)", res.ElementAt(1).StartDate.ToFormattedString());
        Assert.Equal("2021-06-10T01:47:27.0000000 (TDB)", res.ElementAt(1).EndDate.ToFormattedString());
    }

    [Fact]
    public void ReadEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        var searchWindow = new Window(DateTimeExtension.CreateTDB(0.0), DateTimeExtension.CreateTDB(100.0));
        var res = api.ReadEphemeris(searchWindow, TestHelpers.GetEarthAtJ2000(), TestHelpers.GetMoonAtJ2000(), Frame.ICRF, Aberration.LT,
            TimeSpan.FromSeconds(10.0)).Select(x => x.ToStateVector());

        var stateVectors = res as Models.OrbitalParameters.StateVector[] ?? res.ToArray();
        Assert.Equal(-291569264.48965073, stateVectors[0].Position.X);
        Assert.Equal(-266709187.1624887, stateVectors[0].Position.Y);
        Assert.Equal(-76099155.244104564, stateVectors[0].Position.Z);
        Assert.Equal(643.53061483971885, stateVectors[0].Velocity.X);
        Assert.Equal(-666.08181440799092, stateVectors[0].Velocity.Y);
        Assert.Equal(-301.32283209101018, stateVectors[0].Velocity.Z);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, stateVectors[0].CenterOfMotion.NaifId);
        Assert.Equal(Frame.ICRF, stateVectors[0].Frame);
        Assert.Equal(0.0, stateVectors[0].Epoch.SecondsFromJ2000TDB());
    }

    [Fact]
    public void ReadOrientation()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        DateTime start = DateTimeExtension.CreateTDB(662778000.0);
        DateTime end = start.AddSeconds(60.0);
        Models.Time.Window window = new Models.Time.Window(start, end);

        //Configure scenario
        Scenario scenario = new Scenario("Scenario_B", new Mission("mission01"), window);
        scenario.AddBody(TestHelpers.GetSun());
        scenario.AddBody(TestHelpers.GetEarthAtJ2000());
        scenario.AddBody(TestHelpers.GetMoonAtJ2000());

        //Define parking orbit

        Models.OrbitalParameters.StateVector parkingOrbit = new Models.OrbitalParameters.StateVector(
            new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.GetEarthAtJ2000(),
            start, Frame.ICRF);

        //Configure spacecraft
        Clock clock = new Clock("clk1", Math.Pow(2.0, 16.0));
        Models.Body.Spacecraft.Spacecraft spacecraft =
            new Models.Body.Spacecraft.Spacecraft(-179, "DRAGONFLY2", 1000.0, 10000.0,clock,parkingOrbit);
        
        Models.Body.Spacecraft.FuelTank fuelTank = new Models.Body.Spacecraft.FuelTank("ft1", "model1", 9000.0);
        Engine engine = new Engine("engine1", "model1", 450.0, 50.0);
        spacecraft.AddFuelTank(fuelTank, 9000.0, "fuelTank1");
        spacecraft.AddEngine(engine, fuelTank, "serialNumber1");
        spacecraft.AddPayload(new Models.Body.Spacecraft.Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(new Models.Body.Spacecraft.Instrument(600, "CAM600", "mod1", 1.5,
            InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX), Vector3.VectorX);

        spacecraft.SetStandbyManeuver(new NadirAttitude(spacecraft, DateTime.MinValue, TimeSpan.Zero, spacecraft.Engines.First()));


        //Execute scenario
        api.PropagateScenario(scenario, Constants.OutputPath);

        //Load generated kernels
        api.LoadKernels(new DirectoryInfo(Path.Combine(Constants.OutputPath.FullName, "Spacecrafts/DRAGONFLY2")));

        //Read spacecraft orientation
        var res = api.ReadOrientation(window, spacecraft, TimeSpan.FromSeconds(10.0), Frame.ICRF, TimeSpan.FromSeconds(10.0)).ToArray();

        //Read results
        Assert.Equal(0.7071067811865476, res.ElementAt(0).Rotation.W);
        Assert.Equal(0.0, res.ElementAt(0).Rotation.VectorPart.X);
        Assert.Equal(0.0, res.ElementAt(0).Rotation.VectorPart.Y);
        Assert.Equal(-0.7071067811865475, res.ElementAt(0).Rotation.VectorPart.Z);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.X);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.Y);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.Z);
        Assert.Equal(window.StartDate, res.ElementAt(0).Epoch);
        Assert.Equal(Frame.ICRF, res.ElementAt(0).ReferenceFrame);
    }


    [Fact]
    void WriteEphemeris()
    {
        //Initialize API
        API api = new API();

        //Load solar system kernels
        api.LoadKernels(Constants.SolarSystemKernelPath);

        Mission mission = new Mission("miss01");
        Scenario scenario = new Scenario("scn1", mission, new Window(DateTimeExtension.CreateTDB(0.0), DateTimeExtension.CreateTDB(10.0)));
        const int size = 10;
        Clock clock = new Clock("clk1", Math.Pow(2.0, 16.0));
        var spacecraft = new Models.Body.Spacecraft.Spacecraft(-135, "Spc1", 3000.0, 10000.0,clock,new Models.OrbitalParameters.StateVector(new Vector3(6800, 0, 0), new Vector3(0, 8.0, 0),
            TestHelpers.GetEarthAtJ2000(),
            DateTimeExtension.CreateTDB(0.0), Frame.ICRF));
        
        var sv = new Models.OrbitalParameters.StateVector[size];
        for (int i = 0; i < size; ++i)
        {
            sv[i] = new Models.OrbitalParameters.StateVector(new Vector3(6800 + i, i, i), new Vector3(i, 8.0 + i * 0.001, i), TestHelpers.GetEarthAtJ2000(),
                DateTimeExtension.CreateTDB(i), Frame.ICRF);
        }

        //Write ephemeris file
        FileInfo file = new FileInfo("EphemerisTestFile.spk");

        api.WriteEphemeris(file, spacecraft, sv);

        //Load ephemeris file
        api.LoadKernels(file);

        var window = new Models.Time.Window(DateTimeExtension.J2000, DateTimeExtension.J2000.AddSeconds(9.0));
        var stateVectors = api.ReadEphemeris(window, TestHelpers.GetEarthAtJ2000(), spacecraft, Frame.ICRF, Aberration.None, TimeSpan.FromSeconds(1.0))
            .Select(x => x.ToStateVector()).ToArray();
        for (int i = 0; i < size; ++i)
        {
            Assert.Equal(6800.0 + i, stateVectors[i].Position.X);
            Assert.Equal(i, stateVectors[i].Position.Y, 12);
            Assert.Equal(i, stateVectors[i].Position.Z, 12);
            Assert.Equal(i, stateVectors[i].Velocity.X, 12);
            Assert.Equal(8 + i * 0.001, stateVectors[i].Velocity.Y, 12);
            Assert.Equal(i, stateVectors[i].Velocity.Z, 12);
            Assert.Equal(i, stateVectors[i].Epoch.SecondsFromJ2000TDB());
            Assert.Equal(PlanetsAndMoons.EARTH.NaifId, stateVectors[i].CenterOfMotion.NaifId);
            Assert.Equal(Frame.ICRF, stateVectors[i].Frame);
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
        var res = api.GetCelestialBodyInfo(TestHelpers.GetEarthAtJ2000());
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
}