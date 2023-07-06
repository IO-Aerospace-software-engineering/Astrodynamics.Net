using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.DTO;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using IO.Astrodynamics.SolarSystemObjects;
using Xunit;
using ApsidalAlignmentManeuver = IO.Astrodynamics.Maneuver.ApsidalAlignmentManeuver;
using CombinedManeuver = IO.Astrodynamics.Maneuver.CombinedManeuver;
using FuelTank = IO.Astrodynamics.Body.Spacecraft.FuelTank;
using Instrument = IO.Astrodynamics.Body.Spacecraft.Instrument;
using InstrumentPointingToAttitude = IO.Astrodynamics.Maneuver.InstrumentPointingToAttitude;
using Launch = IO.Astrodynamics.Maneuver.Launch;
using NadirAttitude = IO.Astrodynamics.Maneuver.NadirAttitude;
using Payload = IO.Astrodynamics.Body.Spacecraft.Payload;
using PhasingManeuver = IO.Astrodynamics.Maneuver.PhasingManeuver;
using Planetodetic = IO.Astrodynamics.Coordinates.Planetodetic;
using ProgradeAttitude = IO.Astrodynamics.Maneuver.ProgradeAttitude;
using RetrogradeAttitude = IO.Astrodynamics.Maneuver.RetrogradeAttitude;
using Scenario = IO.Astrodynamics.Mission.Scenario;
using Site = IO.Astrodynamics.Surface.Site;
using Spacecraft = IO.Astrodynamics.Body.Spacecraft.Spacecraft;
using StateVector = IO.Astrodynamics.OrbitalParameters.StateVector;
using Window = IO.Astrodynamics.Time.Window;
using ZenithAttitude = IO.Astrodynamics.Maneuver.ZenithAttitude;

namespace IO.Astrodynamics.Tests;

public class APITest
{
    public APITest()
    {
        API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
    }

    [Fact]
    public void CheckVersion()
    {
        Assert.Equal("CSPICE_N0067", API.Instance.GetSpiceVersion());
    }

    [Fact]
    public void ExecuteLaunchScenario()
    {
        //Load solar system kernels
        var start = DateTime.Parse("2021-03-02 00:00:00.000000").ToTDB();
        var end = DateTime.Parse("2021-03-05 00:00:00.000000").ToTDB();

        Window window = new Window(start, end);

        //Define launch site
        LaunchSite launchSite = new LaunchSite(399303, "S3", TestHelpers.EarthAtJ2000,
            new Planetodetic(-81.0 * IO.Astrodynamics.Constants.Deg2Rad, 28.5 * IO.Astrodynamics.Constants.Deg2Rad, 0.0));

        //Define the targeted parking orbit
        StateVector parkingOrbit = new StateVector(
            new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.EarthAtJ2000,
            start, Frames.Frame.ICRF);

        //Create launch object
        Launch launch = new Launch(launchSite, launchSite, parkingOrbit, IO.Astrodynamics.Constants.CivilTwilight,
            true);

        //Find launch windows
        var res = API.Instance.FindLaunchWindows(launch, window, Constants.OutputPath).ToArray();

        //Read results
        Assert.Equal(2, res.Count());
        Assert.Equal(
            new Window(DateTime.Parse("2021-03-03 23:09:15.971").ToTDB(),
                DateTime.Parse("2021-03-03 23:09:15.971").ToTDB()), res.ElementAt(0).Window);
        Assert.Equal(
            new Window(DateTime.Parse("2021-03-04 23:05:19.447").ToTDB(),
                DateTime.Parse("2021-03-04 23:05:19.447").ToTDB()), res.ElementAt(1).Window);
        Assert.Equal(47.00587579161426, res.ElementAt(0).InertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg);
        Assert.Equal(45.125224583051406, res.ElementAt(0).NonInertialAzimuth * IO.Astrodynamics.Constants.Rad2Deg);
        Assert.Equal(8794.33812148836, res.ElementAt(0).InertialInsertionVelocity);
        Assert.Equal(8499.727158006212, res.ElementAt(0).NonInertialInsertionVelocity);
    }

    [Fact]
    public void PropagateScenario()
    {
        var earth = TestHelpers.EarthAtJ2000;
        DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
        DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
        DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
        Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
        scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);
        scenario.AddSite(new Site(132, "MySite", earth, new Planetodetic(0.5, 0.3, 0.0)));

        //Define parking orbit
        StateVector parkingOrbit = new StateVector(
            new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
            new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), earth,
            start, Frames.Frame.ICRF);

        //Define target orbit
        StateVector targetOrbit = new StateVector(
            new Vector3(4390853.7278876612, 5110607.0005866792, 917659.86391987884),
            new Vector3(-4979.4693432656513, 3033.2639866911495, 6933.1803797017265), earth,
            start, Frames.Frame.ICRF);

        //Create and configure spacecraft
        Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft =
            new Spacecraft(-1783, "DRAGONFLY3", 1000.0, 10000.0, clock, parkingOrbit);

        FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
        spacecraft.AddFuelTank(fuelTank);
        spacecraft.AddEngine(engine);
        spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(
            new Instrument(-1783601, "CAM601", "mod1", 80.0 * IO.Astrodynamics.Constants.Deg2Rad,
                InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX));

        var planeAlignmentManeuver = new PlaneAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero,
            targetOrbit, spacecraft.Engines.First());
        planeAlignmentManeuver.SetNextManeuver(new ApsidalAlignmentManeuver(DateTime.MinValue,
                TimeSpan.Zero, targetOrbit, spacecraft.Engines.First()))
            .SetNextManeuver(new PhasingManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, 1,
                spacecraft.Engines.First()))
            .SetNextManeuver(new ApogeeHeightManeuver(DateTime.MinValue, TimeSpan.Zero, 15866666.666666666,
                spacecraft.Engines.First()))
            .SetNextManeuver(new ZenithAttitude(DateTime.MinValue, TimeSpan.Zero, engine))
            .SetNextManeuver(new RetrogradeAttitude(DateTime.MinValue, TimeSpan.Zero, engine))
            .SetNextManeuver(new ProgradeAttitude(DateTime.MinValue, TimeSpan.Zero, engine))
            .SetNextManeuver(new InstrumentPointingToAttitude(DateTime.MinValue, TimeSpan.Zero,
                spacecraft.Intruments.First(), earth, engine));
        spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

        scenario.AddSpacecraft(spacecraft);
        API.Instance.PropagateScenario(scenario, Constants.OutputPath);
        Assert.Throws<ArgumentNullException>(() => API.Instance.PropagateScenario(null, Constants.OutputPath));
        Assert.Throws<ArgumentNullException>(() => API.Instance.PropagateScenario(scenario, null));

        // Read maneuver results
        var maneuver = spacecraft.StandbyManeuver;
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:42.8530000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:51.1750000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.322, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(
            OperatingSystem.IsWindows()
                ? new Vector3(-96.24969153329536, 106.87570557408037, -118.85491757561407)
                : new Vector3(-96.24969153329532, 106.87570557408036, -118.8549175756141),
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
    public void PropagateScenario2()
    {
        var earth = TestHelpers.EarthAtJ2000;
        DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
        DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
        DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

        Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
        Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
        scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);
        scenario.AddSite(new Site(132, "MySite", earth, new Planetodetic(0.5, 0.3, 0.0)));

        //Define parking orbit
        KeplerianElements parkingOrbit = new KeplerianElements(10000000.0, 0.5, 1.0, 0.0, 0.0, 0.0, earth, DateTimeExtension.J2000, Frames.Frame.ICRF);

        //Define target orbit
        KeplerianElements targetOrbit = new KeplerianElements(10000000.0, 0.5, 1.0, 0.0, 0.0, 0.0, earth, DateTimeExtension.J2000, Frames.Frame.ICRF);

        //Create and configure spacecraft
        Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft =
            new Spacecraft(-1783, "DRAGONFLY3", 1000.0, 10000.0, clock, parkingOrbit);

        FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
        spacecraft.AddFuelTank(fuelTank);
        spacecraft.AddEngine(engine);
        spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(
            new Instrument(-1783601, "CAM601", "mod1", 80.0 * IO.Astrodynamics.Constants.Deg2Rad,
                InstrumentShape.Circular, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX));

        var combinedManeuver = new CombinedManeuver(DateTime.MinValue, TimeSpan.Zero, parkingOrbit.PerigeeVector().Magnitude() - 1000.0, parkingOrbit.Inclination() + 0.01,
            spacecraft.Engines.First());
        combinedManeuver.SetNextManeuver(new PerigeeHeightManeuver(DateTimeExtension.J2000, TimeSpan.Zero, combinedManeuver.TargetPerigeeHeight - 10000.0, engine));

        spacecraft.SetStandbyManeuver(combinedManeuver);

        scenario.AddSpacecraft(spacecraft);
        API.Instance.PropagateScenario(scenario, Constants.OutputPath);

        // Read maneuver results
        var maneuver = spacecraft.StandbyManeuver;
        Assert.Equal("2021-03-04T01:33:26.9870000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:29.0410000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:26.9870000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:29.0410000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(2.054, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(
            OperatingSystem.IsWindows()
                ? new Vector3(-0.7082645446926676, 42.94509438155569, -14.496533116862793)
                : new Vector3(-0.7082645446926676, 42.94509438155569, -14.496533116862793),
            ((ImpulseManeuver)maneuver).DeltaV);


        Assert.Equal(102.70768592609926, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:33:29.9790000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:30.1030000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:29.9790000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:33:30.1030000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(0.124, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(0.03180296581310596, 1.461860986217318, 2.327581019451343), ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(6.194029765369872, maneuver.FuelBurned);
    }

    [Fact]
    public void CheckSize()
    {
        var scenario = new DTO.Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(19032, size);
    }

    [Fact]
    public void FindWindowsOnDistanceConstraintProxy()
    {
        //Find time windows when the moon will be 400000 km away from the Earth
        var res = API.Instance.FindWindowsOnDistanceConstraint(
            new Window(DateTimeExtension.CreateTDB(220881665.18391809),
                DateTimeExtension.CreateTDB(228657665.18565452)),
            TestHelpers.EarthAtJ2000, TestHelpers.MoonAtJ2000, RelationnalOperator.Greater, 400000000, Aberration.None,
            TimeSpan.FromSeconds(86400.0));
        var windows = res as Window[] ?? res.ToArray();
        Assert.Equal(4, windows.Count());
        Assert.Equal("2007-01-08T00:11:07.6290000 (TDB)", windows.ElementAt(0).StartDate.ToFormattedString());
        Assert.Equal("2007-01-13T06:37:47.9480000 (TDB)", windows.ElementAt(0).EndDate.ToFormattedString());
        Assert.Equal("2007-03-29T22:53:58.1520000 (TDB)", windows.ElementAt(3).StartDate.ToFormattedString());
        Assert.Equal("2007-04-01T00:01:05.1860000 (TDB)", windows.ElementAt(3).EndDate.ToFormattedString());
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnDistanceConstraint(new Window(
                DateTimeExtension.CreateTDB(220881665.18391809),
                DateTimeExtension.CreateTDB(228657665.18565452)), null, TestHelpers.MoonAtJ2000,
            RelationnalOperator.Greater, 400000000, Aberration.None,
            TimeSpan.FromSeconds(86400.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnDistanceConstraint(new Window(
                DateTimeExtension.CreateTDB(220881665.18391809),
                DateTimeExtension.CreateTDB(228657665.18565452)), TestHelpers.EarthAtJ2000, null,
            RelationnalOperator.Greater, 400000000, Aberration.None,
            TimeSpan.FromSeconds(86400.0)));
    }

    [Fact]
    public void FindWindowsOnOccultationConstraint()
    {
        //Find time windows when the Sun will be occulted by the moon
        var res = API.Instance.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938),
                DateTimeExtension.CreateTDB(61646464.183445148)), TestHelpers.EarthAtJ2000, TestHelpers.Sun,
            ShapeType.Ellipsoid, TestHelpers.MoonAtJ2000, ShapeType.Ellipsoid, OccultationType.Any, Aberration.LT,
            TimeSpan.FromSeconds(3600.0));
        var windows = res as Window[] ?? res.ToArray();
        Assert.Single(windows);
        Assert.Equal("2001-12-14T20:10:15.4110000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2001-12-14T21:35:49.1010000 (TDB)", windows[0].EndDate.ToFormattedString());
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938),
                DateTimeExtension.CreateTDB(61646464.183445148)), null, TestHelpers.Sun,
            ShapeType.Ellipsoid, TestHelpers.MoonAtJ2000, ShapeType.Ellipsoid, OccultationType.Any, Aberration.LT,
            TimeSpan.FromSeconds(3600.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938),
                DateTimeExtension.CreateTDB(61646464.183445148)), TestHelpers.EarthAtJ2000, null,
            ShapeType.Ellipsoid, TestHelpers.MoonAtJ2000, ShapeType.Ellipsoid, OccultationType.Any, Aberration.LT,
            TimeSpan.FromSeconds(3600.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnOccultationConstraint(
            new Window(DateTimeExtension.CreateTDB(61473664.183390938),
                DateTimeExtension.CreateTDB(61646464.183445148)), TestHelpers.EarthAtJ2000, TestHelpers.Sun,
            ShapeType.Ellipsoid, null, ShapeType.Ellipsoid, OccultationType.Any, Aberration.LT,
            TimeSpan.FromSeconds(3600.0)));
    }

    [Fact]
    public void FindWindowsOnCoordinateConstraint()
    {
        Site site = new Site(13, "DSS-13", TestHelpers.EarthAtJ2000,
            new Planetodetic(-116.7944627147624 * IO.Astrodynamics.Constants.Deg2Rad,
                35.2471635434595 * IO.Astrodynamics.Constants.Deg2Rad, 0.107));
        //Find time windows when the moon will be above the horizon relative to Deep Space Station 13
        var res = API.Instance.FindWindowsOnCoordinateConstraint(
            new Window(DateTimeExtension.CreateTDB(730036800.0), DateTimeExtension.CreateTDB(730123200)), site,
            TestHelpers.MoonAtJ2000, site.Frame, CoordinateSystem.Latitudinal, Coordinate.Latitude,
            RelationnalOperator.Greater,
            0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0));

        var windows = res as Window[] ?? res.ToArray();
        Assert.Single(windows);
        Assert.Equal("2023-02-19T14:33:08.9180000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2023-02-20T00:00:00.0000000 (TDB)", windows[0].EndDate.ToFormattedString());
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnCoordinateConstraint(
            new Window(DateTimeExtension.CreateTDB(730036800.0), DateTimeExtension.CreateTDB(730123200)), null,
            TestHelpers.MoonAtJ2000, site.Frame, CoordinateSystem.Latitudinal, Coordinate.Latitude,
            RelationnalOperator.Greater, 0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnCoordinateConstraint(
            new Window(DateTimeExtension.CreateTDB(730036800.0), DateTimeExtension.CreateTDB(730123200)), site,
            null, site.Frame, CoordinateSystem.Latitudinal, Coordinate.Latitude,
            RelationnalOperator.Greater, 0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnCoordinateConstraint(
            new Window(DateTimeExtension.CreateTDB(730036800.0), DateTimeExtension.CreateTDB(730123200)), site,
            TestHelpers.MoonAtJ2000, null, CoordinateSystem.Latitudinal, Coordinate.Latitude,
            RelationnalOperator.Greater, 0.0, 0.0, Aberration.None, TimeSpan.FromSeconds(60.0)));
    }

    [Fact]
    public void FindWindowsOnIlluminationConstraint()
    {
        //Find time windows when the planetodetic point is illuminated by the sun (Official twilight 0.8° bellow horizon)
        var res = API.Instance.FindWindowsOnIlluminationConstraint(
            new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            TestHelpers.Sun, TestHelpers.EarthAtJ2000, new Frames.Frame("ITRF93"),
            new Planetodetic(2.2 * IO.Astrodynamics.Constants.Deg2Rad, 48.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower,
            System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5), TestHelpers.Sun);
        var windows = res as Window[] ?? res.ToArray();
        Assert.Equal(2, windows.Count());
        Assert.Equal("2021-05-17T12:00:00.0000000 (TDB)", windows[0].StartDate.ToFormattedString());
        Assert.Equal("2021-05-17T19:35:24.9090000 (TDB)", windows[0].EndDate.ToFormattedString());
        Assert.Equal("2021-05-18T04:18:32.4440000 (TDB)", windows[1].StartDate.ToFormattedString());
        Assert.Equal("2021-05-18T12:00:00.0000000 (TDB)", windows[1].EndDate.ToFormattedString());
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnIlluminationConstraint(
            new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            null, TestHelpers.EarthAtJ2000, new Frames.Frame("ITRF93"),
            new Planetodetic(2.2 * IO.Astrodynamics.Constants.Deg2Rad, 48.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower,
            System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5), TestHelpers.Sun));

        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnIlluminationConstraint(
            new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            TestHelpers.Sun, null, new Frames.Frame("ITRF93"),
            new Planetodetic(2.2 * IO.Astrodynamics.Constants.Deg2Rad, 48.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower,
            System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5), TestHelpers.Sun));

        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnIlluminationConstraint(
            new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            TestHelpers.Sun, TestHelpers.EarthAtJ2000, null,
            new Planetodetic(2.2 * IO.Astrodynamics.Constants.Deg2Rad, 48.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower,
            System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5), TestHelpers.Sun));

        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsOnIlluminationConstraint(
            new Window(DateTimeExtension.CreateTDB(674524800), DateTimeExtension.CreateTDB(674611200)),
            TestHelpers.Sun, TestHelpers.EarthAtJ2000, new Frames.Frame("ITRF93"),
            new Planetodetic(2.2 * IO.Astrodynamics.Constants.Deg2Rad, 48.0 * IO.Astrodynamics.Constants.Deg2Rad, 0.0),
            IlluminationAngle.Incidence, RelationnalOperator.Lower,
            System.Math.PI * 0.5 - (-0.8 * IO.Astrodynamics.Constants.Deg2Rad), 0.0, Aberration.CNS,
            TimeSpan.FromHours(4.5), null));
    }

    [Fact]
    public void FindWindowsInFieldOfViewConstraint()
    {
        DateTime start = DateTimeExtension.CreateUTC(676555130.80).ToTDB();
        DateTime end = start.AddSeconds(6448.0);

        //Configure scenario
        Scenario scenario = new Scenario("Scenario_A", new Astrodynamics.Mission.Mission("mission01"),
            new Window(start, end));
        scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);

        //Define parking orbit

        StateVector parkingOrbit = new StateVector(
            new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000,
            start, Frames.Frame.ICRF);

        //Configure spacecraft
        Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft =
            new Spacecraft(-172, "DRAGONFLY2", 1000.0, 10000.0, clock, parkingOrbit);

        FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
        spacecraft.AddFuelTank(fuelTank);
        spacecraft.AddEngine(engine);
        spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(new Instrument(-172600, "CAM602", "mod1", 1.5, InstrumentShape.Circular,
            Vector3.VectorZ, Vector3.VectorY,
            new Vector3(0.0, -System.Math.PI * 0.5, 0.0)));
        scenario.AddSpacecraft(spacecraft);

        //Execute scenario
        API.Instance.PropagateScenario(scenario, Constants.OutputPath);

        //Find windows when the earth is in field of view of camera 600 
        var res = API.Instance.FindWindowsInFieldOfViewConstraint(
            new Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
            spacecraft.Intruments.First(), TestHelpers.EarthAtJ2000, TestHelpers.EarthAtJ2000.Frame,
            ShapeType.Ellipsoid, Aberration.LT,
            TimeSpan.FromHours(1.0)).ToArray();

        //Read results
        Assert.Equal(2, res.Count());
        Assert.Equal("2021-06-10T00:00:00.0000000 (TDB)", res.ElementAt(0).StartDate.ToFormattedString());
        Assert.Equal("2021-06-10T00:29:03.3090000 (TDB)", res.ElementAt(0).EndDate.ToFormattedString());
        Assert.Equal("2021-06-10T01:04:03.5060000 (TDB)", res.ElementAt(1).StartDate.ToFormattedString());
        Assert.Equal("2021-06-10T01:47:27.0000000 (TDB)", res.ElementAt(1).EndDate.ToFormattedString());

        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsInFieldOfViewConstraint(
            new Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), null,
            spacecraft.Intruments.First(), TestHelpers.EarthAtJ2000, TestHelpers.EarthAtJ2000.Frame,
            ShapeType.Ellipsoid, Aberration.LT, TimeSpan.FromHours(1.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsInFieldOfViewConstraint(
            new Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
            null, TestHelpers.EarthAtJ2000, TestHelpers.EarthAtJ2000.Frame, ShapeType.Ellipsoid, Aberration.LT,
            TimeSpan.FromHours(1.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsInFieldOfViewConstraint(
            new Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
            spacecraft.Intruments.First(), null, TestHelpers.EarthAtJ2000.Frame, ShapeType.Ellipsoid, Aberration.LT,
            TimeSpan.FromHours(1.0)));
        Assert.Throws<ArgumentNullException>(() => API.Instance.FindWindowsInFieldOfViewConstraint(
            new Window(DateTimeExtension.CreateTDB(676555200.0), DateTimeExtension.CreateTDB(676561647.0)), spacecraft,
            spacecraft.Intruments.First(), TestHelpers.EarthAtJ2000, null, ShapeType.Ellipsoid, Aberration.LT,
            TimeSpan.FromHours(1.0)));
    }

    [Fact]
    public void ReadEphemeris()
    {
        var searchWindow = new Window(DateTimeExtension.CreateTDB(0.0), DateTimeExtension.CreateTDB(100.0));
        var res = API.Instance.ReadEphemeris(searchWindow, TestHelpers.EarthAtJ2000, TestHelpers.MoonAtJ2000,
            Frames.Frame.ICRF, Aberration.LT, TimeSpan.FromSeconds(10.0)).Select(x => x.ToStateVector());

        var stateVectors = res as StateVector[] ?? res.ToArray();
        Assert.Equal(-291569264.48965073, stateVectors[0].Position.X);
        Assert.Equal(-266709187.1624887, stateVectors[0].Position.Y);
        Assert.Equal(-76099155.244104564, stateVectors[0].Position.Z);
        Assert.Equal(643.53061483971885, stateVectors[0].Velocity.X);
        Assert.Equal(-666.08181440799092, stateVectors[0].Velocity.Y);
        Assert.Equal(-301.32283209101018, stateVectors[0].Velocity.Z);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, stateVectors[0].CenterOfMotion.NaifId);
        Assert.Equal(Frames.Frame.ICRF, stateVectors[0].Frame);
        Assert.Equal(0.0, stateVectors[0].Epoch.SecondsFromJ2000TDB());

        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow, null,
            TestHelpers.MoonAtJ2000,
            Frames.Frame.ICRF, Aberration.LT, TimeSpan.FromSeconds(10.0)).Select(x => x.ToStateVector()));
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow, TestHelpers.EarthAtJ2000,
            null,
            Frames.Frame.ICRF, Aberration.LT, TimeSpan.FromSeconds(10.0)).Select(x => x.ToStateVector()));
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow, TestHelpers.EarthAtJ2000,
            TestHelpers.MoonAtJ2000,
            null, Aberration.LT, TimeSpan.FromSeconds(10.0)).Select(x => x.ToStateVector()));
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow.StartDate, null,
            TestHelpers.MoonAtJ2000,
            Frames.Frame.ICRF, Aberration.LT));
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow.StartDate,
            TestHelpers.EarthAtJ2000, null,
            Frames.Frame.ICRF, Aberration.LT));
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadEphemeris(searchWindow.StartDate,
            TestHelpers.EarthAtJ2000, TestHelpers.MoonAtJ2000,
            null, Aberration.LT));
    }

    [Fact]
    public void ReadOrientation()
    {
        DateTime start = DateTimeExtension.CreateTDB(662778000.0);
        DateTime end = start.AddSeconds(60.0);
        Window window = new Window(start, end);

        //Configure scenario
        Scenario scenario = new Scenario("Scenario_B", new Astrodynamics.Mission.Mission("mission01"), window);
        scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);

        //Define parking orbit

        StateVector parkingOrbit = new StateVector(
            new Vector3(6800000.0, 0.0, 0.0), new Vector3(0.0, 7656.2204182967143, 0.0), TestHelpers.EarthAtJ2000,
            start, Frames.Frame.ICRF);

        //Configure spacecraft
        Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        Spacecraft spacecraft =
            new Spacecraft(-1794, "DRAGONFLY4", 1000.0, 10000.0, clock, parkingOrbit);

        FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
        Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
        spacecraft.AddFuelTank(fuelTank);
        spacecraft.AddEngine(engine);
        spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
        spacecraft.AddInstrument(new Instrument(-1794600, "CAM600", "mod1", 1.5, InstrumentShape.Circular,
            Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX));

        spacecraft.SetStandbyManeuver(new NadirAttitude(DateTime.MinValue, TimeSpan.Zero,
            spacecraft.Engines.First()));

        scenario.AddSpacecraft(spacecraft);

        //Execute scenario
        API.Instance.PropagateScenario(scenario, Constants.OutputPath);

        //Read spacecraft orientation
        var res = API.Instance.ReadOrientation(window, spacecraft, TimeSpan.FromSeconds(10.0), Frames.Frame.ICRF,
            TimeSpan.FromSeconds(10.0)).ToArray();


        //Read results
        Assert.Equal(0.7071067811865476, res.ElementAt(0).Rotation.W);
        Assert.Equal(0.0, res.ElementAt(0).Rotation.VectorPart.X);
        Assert.Equal(0.0, res.ElementAt(0).Rotation.VectorPart.Y);
        Assert.Equal(-0.7071067811865475, res.ElementAt(0).Rotation.VectorPart.Z);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.X);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.Y);
        Assert.Equal(0.0, res.ElementAt(0).AngularVelocity.Z);
        Assert.Equal(window.StartDate, res.ElementAt(0).Epoch);
        Assert.Equal(Frames.Frame.ICRF, res.ElementAt(0).ReferenceFrame);

        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadOrientation(window, null,
            TimeSpan.FromSeconds(10.0), Frames.Frame.ICRF,
            TimeSpan.FromSeconds(10.0)).ToArray());
        Assert.Throws<ArgumentNullException>(() => API.Instance.ReadOrientation(window, spacecraft,
            TimeSpan.FromSeconds(10.0), null,
            TimeSpan.FromSeconds(10.0)).ToArray());
    }


    [Fact]
    void WriteEphemeris()
    {
        //Load solar system kernels
        const int size = 10;
        Clock clock = new Clock("clk1", System.Math.Pow(2.0, 16.0));
        var spacecraft = new Spacecraft(-135, "Spc1", 3000.0, 10000.0, clock, new StateVector(new Vector3(6800, 0, 0),
            new Vector3(0, 8.0, 0),
            TestHelpers.EarthAtJ2000,
            DateTimeExtension.CreateTDB(0.0), Frames.Frame.ICRF));

        var sv = new StateVector[size];
        for (int i = 0; i < size; ++i)
        {
            sv[i] = new StateVector(new Vector3(6800 + i, i, i), new Vector3(i, 8.0 + i * 0.001, i),
                TestHelpers.EarthAtJ2000,
                DateTimeExtension.CreateTDB(i), Frames.Frame.ICRF);
        }

        //Write ephemeris file
        FileInfo file = new FileInfo("EphemerisTestFile.spk");

        Assert.Throws<ArgumentNullException>(() => API.Instance.WriteEphemeris(null, spacecraft, sv));
        Assert.Throws<ArgumentNullException>(() => API.Instance.WriteEphemeris(file, null, sv));
        Assert.Throws<ArgumentNullException>(() => API.Instance.WriteEphemeris(file, spacecraft, null));
        API.Instance.WriteEphemeris(file, spacecraft, sv);

        //Load ephemeris file
        API.Instance.LoadKernels(file);

        var window = new Window(DateTimeExtension.J2000, DateTimeExtension.J2000.AddSeconds(9.0));
        var stateVectors = API.Instance.ReadEphemeris(window, TestHelpers.EarthAtJ2000, spacecraft, Frames.Frame.ICRF,
                Aberration.None, TimeSpan.FromSeconds(1.0))
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
            Assert.Equal(Frames.Frame.ICRF, stateVectors[i].Frame);
        }
    }

    [Fact]
    void GetCelestialBodyInformation()
    {
        //Read celestial body information from spice kernels
        var res = API.Instance.GetCelestialBodyInfo(TestHelpers.EarthAtJ2000.NaifId);
        Assert.Equal(PlanetsAndMoons.EARTH.NaifId, res.Id);
        Assert.Equal(Stars.Sun.NaifId, res.CenterOfMotionId);
        Assert.Equal(PlanetsAndMoons.EARTH.Name, res.Name);
        Assert.Equal(13000, res.FrameId);
        Assert.Equal("ITRF93", res.FrameName);
        Assert.Equal(398600435436095.93, res.GM);
        Assert.Equal(6378136.5999999998, res.Radii.X);
        Assert.Equal(6378136.5999999998, res.Radii.Y);
        Assert.Equal(6356751.9000000002, res.Radii.Z);
    }

    [Fact]
    void GetInvalidCelestialBodyInformation()
    {
        Assert.Throws<InvalidOperationException>(() => API.Instance.GetCelestialBodyInfo(398));
    }

    [Fact]
    void TransformFrame()
    {
        //Get the quaternion to transform
        var res = API.Instance.TransformFrame(Frames.Frame.ICRF, new Frames.Frame(PlanetsAndMoons.EARTH.Frame),
            DateTimeExtension.J2000);
        Assert.Equal(0.76713121189662548, res.Rotation.W);
        Assert.Equal(-1.8618846012434252e-05, res.Rotation.VectorPart.X);
        Assert.Equal(8.468919252183845e-07, res.Rotation.VectorPart.Y);
        Assert.Equal(0.64149022080358797, res.Rotation.VectorPart.Z);
        Assert.Equal(-1.9637714059853662e-09, res.AngularVelocity.X);
        Assert.Equal(-2.0389340573814659e-09, res.AngularVelocity.Y);
        Assert.Equal(7.2921150642488516e-05, res.AngularVelocity.Z);
    }

    [Fact]
    void TransformFrameExceptions()
    {
        //Get the quaternion to transform
        Assert.Throws<ArgumentNullException>(() =>
            API.Instance.TransformFrame(Frames.Frame.ICRF, null, DateTimeExtension.J2000));
        Assert.Throws<ArgumentNullException>(() =>
            API.Instance.TransformFrame(null, new Frames.Frame(PlanetsAndMoons.EARTH.Frame), DateTimeExtension.J2000));
    }

    [Fact]
    void Quaternion()
    {
        DTO.Quaternion q = new DTO.Quaternion(1.0, 2.0, 3.0, 4.0);
        Assert.Equal(1.0, q.W);
        Assert.Equal(2.0, q.X);
        Assert.Equal(3.0, q.Y);
        Assert.Equal(4.0, q.Z);
    }

    [Fact]
    void RaDec()
    {
        DTO.RaDec raDec = new DTO.RaDec(1.0, 2.0, 3.0);
        Assert.Equal(1.0, raDec.RightAscencion);
        Assert.Equal(2.0, raDec.Declination);
        Assert.Equal(3.0, raDec.Radius);
    }

    [Fact]
    void StateOrientation()
    {
        DTO.StateOrientation so = new DTO.StateOrientation(new DTO.Quaternion(1.0, 2.0, 3.0, 4.0),
            new Vector3D(1.0, 2.0, 3.0), 10.0, "J2000");
        Assert.Equal(new DTO.Quaternion(1.0, 2.0, 3.0, 4.0), so.Rotation);
        Assert.Equal(new Vector3D(1.0, 2.0, 3.0), so.AngularVelocity);
        Assert.Equal(10.0, so.Epoch);
        Assert.Equal("J2000", so.Frame);
    }

    [Fact]
    void AzimuthRange()
    {
        DTO.AzimuthRange so = new DTO.AzimuthRange(10.0, 20.0);
        Assert.Equal(10.0, so.Start);
        Assert.Equal(20.0, so.End);
    }

    [Fact]
    void UnloadKernelException()
    {
        Assert.Throws<ArgumentNullException>(() => API.Instance.UnloadKernels(null));
    }

    [Fact]
    void LoadKernelException()
    {
        Assert.Throws<ArgumentNullException>(() => API.Instance.LoadKernels(null));
    }

    [Fact]
    void CelestialBody()
    {
        DTO.CelestialBody celestialBody = new CelestialBody(1, 2, "body", new Vector3D(1.0, 2.0, 3.0), 123, "frame", 147);
        Assert.Equal(1, celestialBody.Id);
        Assert.Equal(2, celestialBody.CenterOfMotionId);
        Assert.Equal("body", celestialBody.Name);
        Assert.Equal(new Vector3D(1.0, 2.0, 3.0), celestialBody.Radii);
        Assert.Equal(147, celestialBody.FrameId);
        Assert.Equal("frame", celestialBody.FrameName);
        Assert.Equal(123, celestialBody.GM);
    }

    [Fact]
    void TLEElements()
    {
        DTO.TLEElements tle = new TLEElements(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        Assert.Equal(1, tle.BalisticCoefficient);
        Assert.Equal(2, tle.SecondDerivativeOfMeanMotion);
        Assert.Equal(3, tle.DragTerm);
        Assert.Equal(4, tle.Epoch);
        Assert.Equal(5, tle.A);
        Assert.Equal(6, tle.E);
        Assert.Equal(7, tle.I);
        Assert.Equal(8, tle.W);
        Assert.Equal(9, tle.O);
        Assert.Equal(10, tle.M);
    }
}