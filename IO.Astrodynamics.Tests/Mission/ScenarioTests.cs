using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Cosmographia;
using IO.Astrodynamics.Maneuver;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Mission;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.SolarSystemObjects;
using IO.Astrodynamics.Surface;
using IO.Astrodynamics.Time;
using Xunit;

namespace IO.Astrodynamics.Tests.Mission
{
    public class ScenarioTests
    {
        public ScenarioTests()
        {
            API.Instance.LoadKernels(Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void Create()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Mission1");
            Scenario scenario = new Scenario("Scenario", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            Assert.Equal("Scenario", scenario.Name);
            Assert.Equal(mission, scenario.Mission);
        }

        [Fact]
        public void AddSite()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Mission1");
            Scenario scenario = new Scenario("Scenario", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            Assert.Equal("Scenario", scenario.Name);
            Assert.Equal(mission, scenario.Mission);
            var site = new Site(13, "DSS-13", new CelestialBody(PlanetsAndMoons.EARTH));
            scenario.AddSite(site);
            Assert.Single(scenario.Sites);
            Assert.Equal(site, scenario.Sites.First());
            Assert.Throws<ArgumentNullException>(() => scenario.AddSite(null));
        }

        [Fact]
        public void AddAdditionalCelestialBody()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("Mission1");
            Scenario scenario = new Scenario("Scenario", mission, new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);
            Assert.Single(scenario.AdditionalCelstialBodies);
            Assert.Equal(TestHelpers.MoonAtJ2000, scenario.AdditionalCelstialBodies.First());
            Assert.Throws<ArgumentNullException>(() => scenario.AddAdditionalCelestialBody(null));
        }

        [Fact]
        public async Task PropagateSite()
        {
            Window window = new Window(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Unspecified), new DateTime(2000, 1, 2, 12, 0, 0, DateTimeKind.Unspecified));
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("MissionSite");
            Scenario scenario = new Scenario("scnSite", mission, window);
            Site site = new Site(333, "S333", TestHelpers.EarthAtJ2000, new Planetodetic(30 * Astrodynamics.Constants.Deg2Rad, 10 * Astrodynamics.Constants.Deg2Rad, 1000.0));
            scenario.AddSite(site);

            //Propagate site
            await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            var res = site.GetEphemeris(window, TestHelpers.EarthAtJ2000, Frames.Frame.ICRF, Aberration.None, TimeSpan.FromHours(1));
            var orbitalParametersEnumerable = res as Astrodynamics.OrbitalParameters.OrbitalParameters[] ?? res.ToArray();
            Assert.Equal(new Vector3(4054783.0920394, -4799280.902678638, 1100391.2395513842), orbitalParametersEnumerable.ElementAt(0).ToStateVector().Position,
                TestHelpers.VectorComparer);
            Assert.Equal(new Vector3(349.9689414487369, 295.67943565441215, 0.00047467276487285595), orbitalParametersEnumerable.ElementAt(0).ToStateVector().Velocity,
                TestHelpers.VectorComparer);
            Assert.Equal(DateTimeExtension.J2000, orbitalParametersEnumerable.ElementAt(0).Epoch);
            Assert.Equal(Frames.Frame.ICRF, orbitalParametersEnumerable.ElementAt(0).Frame);
            Assert.Equal(TestHelpers.EarthAtJ2000, orbitalParametersEnumerable.ElementAt(0).Observer);

            Assert.Equal(5675531.4473050004, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Position.X, 6);
            Assert.Equal(2694837.3700879999, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Position.Y, 6);
            Assert.Equal(1100644.504743, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Position.Z, 6);
            Assert.Equal(-196.510785, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Velocity.X, 6);
            Assert.Equal(413.86626653238068, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Velocity.Y, 6);
            Assert.Equal(0.00077810438580775993, orbitalParametersEnumerable.ElementAt(5).ToStateVector().Velocity.Z, 6);
            Assert.Equal(18000.0, orbitalParametersEnumerable.ElementAt(5).Epoch.SecondsFromJ2000TDB());
            Assert.Equal(Frames.Frame.ICRF, orbitalParametersEnumerable.ElementAt(5).Frame);
            Assert.Equal(TestHelpers.EarthAtJ2000, orbitalParametersEnumerable.ElementAt(5).Observer);
        }

        [Fact]
        [Benchmark]
        public async Task PropagateSpacecraft()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission02");
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

            //Create and configure spacecraft
            Clock clock = new Clock("clk1", 65536);
            Spacecraft spacecraft = new Spacecraft(-1783, "DRAGONFLY3", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
            Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
            spacecraft.AddFuelTank(fuelTank);
            spacecraft.AddEngine(engine);
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddCircularInstrument(-1783601, "CAM601", "mod1", 80.0 * IO.Astrodynamics.Constants.Deg2Rad, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX);

            var planeAlignmentManeuver = new PlaneAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraft.Engines.First());
            planeAlignmentManeuver.SetNextManeuver(new ApsidalAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraft.Engines.First()))
                .SetNextManeuver(new PhasingManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, 1, spacecraft.Engines.First()))
                .SetNextManeuver(new ApogeeHeightManeuver(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.Zero, 15866666.666666666, spacecraft.Engines.First()))
                .SetNextManeuver(new ZenithAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new RetrogradeAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new ProgradeAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine));
            spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

            scenario.AddSpacecraft(spacecraft);

            var summary = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));

            // Read maneuver results
            Maneuver.Maneuver maneuver1 = planeAlignmentManeuver;
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.1349999999999998, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-94.14448820356235, 104.5303929755525, -115.98378254209923), ((ImpulseManeuver)maneuver1).DeltaV);

            Assert.Equal(406.75790669984616, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(18.389249299999999, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-373.2375968451479, -125.2159857526674, 201.3904220942568), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(919.46246904264035, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:19:30.9397124 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:17.2909478 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.8117900999999996, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.03536956665516, 85.94488147231478, 195.5576679432201), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(490.58999999999997, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T08:44:24.8235942 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:44:34.1182124 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:44:24.8235942 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:44:34.1182124 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.2949856999999998, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(140.54747802231253, -86.30580739826372, -196.3158766093938), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(464.73091174262282, maneuver1.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:34:35.5958002"), maneuverWindow.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T08:47:39.0138099"), maneuverWindow.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2281.5410000000002, summary.SpacecraftSummaries.First().FuelConsumption, 3);
        }

        [Fact]
        [Benchmark]
        public async Task MultipleSpacecraftPropagation()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission02");
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

            //Create and configure spacecraft
            Clock clock = new Clock("clk1", 65536);
            Spacecraft spacecraft = new Spacecraft(-1783, "DRAGONFLY31", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
            Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
            spacecraft.AddFuelTank(fuelTank);
            spacecraft.AddEngine(engine);
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddCircularInstrument(-1783601, "CAM601", "mod1", 80.0 * IO.Astrodynamics.Constants.Deg2Rad, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX);

            var planeAlignmentManeuver = new PlaneAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraft.Engines.First());
            planeAlignmentManeuver.SetNextManeuver(new ApsidalAlignmentManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, spacecraft.Engines.First()))
                .SetNextManeuver(new PhasingManeuver(DateTime.MinValue, TimeSpan.Zero, targetOrbit, 1, spacecraft.Engines.First()))
                .SetNextManeuver(new ApogeeHeightManeuver(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.Zero, 15866666.666666666, spacecraft.Engines.First()))
                .SetNextManeuver(new ZenithAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new RetrogradeAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new ProgradeAttitude(TestHelpers.EarthAtJ2000, DateTime.MinValue, TimeSpan.FromMinutes(1), engine));
            spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

            scenario.AddSpacecraft(spacecraft);

            var summary1 = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            // Read maneuver results
            Maneuver.Maneuver maneuver1 = spacecraft.InitialManeuver;
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.1349999999999998, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-94.14448820356235, 104.5303929755525, -115.98378254209923), ((ImpulseManeuver)maneuver1).DeltaV);

            Assert.Equal(406.75790669984616, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(18.389249299999999, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-373.2375968451479, -125.2159857526674, 201.3904220942568), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(919.46246904264035, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:19:30.9397124 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:17.2909478 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.8117900999999996, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.03536956665516, 85.94488147231478, 195.5576679432201), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(490.58999999999997, maneuver1.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary1.Window);
            Assert.Single(summary1.SpacecraftSummaries);
            var maneuverWindow1 = summary1.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow1 != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:34:35.5958002"), maneuverWindow1.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T08:47:39.0138099"), maneuverWindow1.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2281.540792804627, summary1.SpacecraftSummaries.First().FuelConsumption, 3);

            API.Instance.UnloadKernels(scenario.SpacecraftDirectory);
            API.Instance.UnloadKernels(scenario.SiteDirectory);
            var summary2 = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            // Read maneuver results
            Maneuver.Maneuver maneuver2 = spacecraft.InitialManeuver;
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:35.5957946 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:34:43.7309527 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.1349999999999998, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-94.14448820356235, 104.5303929755525, -115.98378254209923), ((ImpulseManeuver)maneuver2).DeltaV);

            Assert.Equal(406.75790669984616, maneuver2.FuelBurned, 3);

            maneuver2 = maneuver2.NextManeuver;

            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:18.7665699 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:19:37.1558192 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(18.389249299999999, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-373.2375968451479, -125.2159857526674, 201.3904220942568), ((ImpulseManeuver)maneuver2).DeltaV);
            Assert.Equal(919.46246904264035, maneuver2.FuelBurned, 3);

            maneuver2 = maneuver2.NextManeuver;

            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T08:19:30.9397124 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:07.4791577 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:36:17.2909478 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.8117900999999996, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.03536956665516, 85.94488147231478, 195.5576679432201), ((ImpulseManeuver)maneuver2).DeltaV);
            Assert.Equal(490.58999999999997, maneuver2.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary2.Window);
            Assert.Single(summary2.SpacecraftSummaries);
            var maneuverWindow2 = summary2.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow2 != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:34:35.5958002"), maneuverWindow2.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T08:47:39.0138099"), maneuverWindow2.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2281.540792804627, summary2.SpacecraftSummaries.First().FuelConsumption, 3);
        }

        [Fact]
        [Benchmark]
        public async Task PropagateWithoutManeuver()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission102");
            Scenario scenario = new Scenario("scn100", mission, new Window(startPropagator, end));
            scenario.AddAdditionalCelestialBody(TestHelpers.MoonAtJ2000);

            //Define parking orbit
            StateVector parkingOrbit = new StateVector(
                new Vector3(5056554.1874925727, 4395595.4942363985, 0.0),
                new Vector3(-3708.6305608890916, 4266.2914313011433, 6736.8538488755494), TestHelpers.EarthAtJ2000,
                start, Frames.Frame.ICRF);

            //Create and configure spacecraft
            Clock clock = new Clock("clk1", 65536);
            Spacecraft spacecraft = new Spacecraft(-1785, "DRAGONFLY32", 1000.0, 10000.0, clock, parkingOrbit);

            FuelTank fuelTank = new FuelTank("ft1", "model1", "sn1", 9000.0, 9000.0);
            Engine engine = new Engine("engine1", "model1", "sn1", 450.0, 50.0, fuelTank);
            spacecraft.AddFuelTank(fuelTank);
            spacecraft.AddEngine(engine);
            spacecraft.AddPayload(new Payload("payload1", 50.0, "pay01"));
            spacecraft.AddCircularInstrument(-1785601, "CAM601", "mod1", 80.0 * IO.Astrodynamics.Constants.Deg2Rad, Vector3.VectorZ, Vector3.VectorX, Vector3.VectorX);

            scenario.AddSpacecraft(spacecraft);
            var summary = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(new DateTime(2021, 3, 4, 0, 32, 53, 814, DateTimeKind.Unspecified), maneuverWindow.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(new DateTime(2021, 3, 4, 5, 27, 13, 014, DateTimeKind.Unspecified), maneuverWindow.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(0.0, summary.SpacecraftSummaries.First().FuelConsumption, 3);
        }

        [Fact]
        [Benchmark]
        public async Task PropagateStar()
        {
            var start = new DateTime(2001, 1, 1);
            var end = start.AddDays(365 * 4);
            var observer = new Barycenter(0);
            var star = new Star(2, "star2", 1E+30, "spec", 2, 0.3792, new Equatorial(1, 1), 0.1, 0.1, 0, 0, 0, 0, start);

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission102");
            Scenario scenario = new Scenario("scn100", mission, new Window(start, end));
            scenario.AddStar(star);

            var summary = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromDays(365.0));

            Assert.Equal(scenario.Window, summary.Window);

            var eph0 = star.GetEphemeris(start, observer, Frames.Frame.ICRF, Aberration.None);
            var eph1 = star.GetEphemeris(start.Add(TimeSpan.FromDays(365)), observer, Frames.Frame.ICRF, Aberration.None);
            var eph2 = star.GetEphemeris(start.Add(TimeSpan.FromDays(365 + 365)), observer, Frames.Frame.ICRF, Aberration.None);
            Assert.Equal(1.0, eph0.ToEquatorial().RightAscension, 12);
            Assert.Equal(1.0, eph0.ToEquatorial().Declination, 12);
            Assert.Equal(8.1373353929324900E+16, eph0.ToEquatorial().Distance);

            Assert.Equal(1.1, eph1.ToEquatorial().RightAscension, 3);
            Assert.Equal(1.1, eph1.ToEquatorial().Declination, 3);
            Assert.Equal(8.1373353929324910E+16, eph1.ToEquatorial().Distance);

            Assert.Equal(1.2, eph2.ToEquatorial().RightAscension, 3);
            Assert.Equal(1.2, eph2.ToEquatorial().Declination, 3);
            Assert.Equal(8.1373353929324910E+16, eph2.ToEquatorial().Distance);
        }


        [Fact]
        public void Equality()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission02");
            Scenario scenario = new Scenario("scn1", mission, new Window(DateTime.MinValue, DateTime.MaxValue));

            Astrodynamics.Mission.Mission mission2 = new Astrodynamics.Mission.Mission("mission03");
            Scenario scenario2 = new Scenario("scn1", mission2, new Window(DateTime.MinValue, DateTime.MaxValue));

            Assert.True(scenario != scenario2);
            Assert.False(scenario == scenario2);
            Assert.False(scenario.Equals(scenario2));
            Assert.False(scenario.Equals(null));
            Assert.True(scenario.Equals(scenario));
            Assert.False(scenario.Equals((object)scenario2));
            Assert.False(scenario.Equals((object)null));
            Assert.True(scenario.Equals((object)scenario));
        }

        [Fact]
        public async Task PropagateException()
        {
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission04");
            Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
            Assert.Throws<ArgumentNullException>(() => scenario.AddSpacecraft(null));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await scenario.SimulateAsync(new DirectoryInfo("/"), false, false, TimeSpan.FromSeconds(1.0)));
        }

        [Fact]
        public async Task UserFeedback()
        {
            var frame = Frames.Frame.ICRF;
            var start = DateTimeExtension.J2000;
            var end = DateTimeExtension.J2000.AddDays(30);
            var earth = new CelestialBody(PlanetsAndMoons.EARTH, frame, start);
            var moon = new CelestialBody(PlanetsAndMoons.MOON, frame, start);
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
            Scenario scenario = new Scenario("scn01", mission, new IO.Astrodynamics.Time.Window(start, end));

//Define test orbit
            StateVector testOrbit = moon.GetEphemeris(start, earth, frame, Aberration.None).ToStateVector();
            Clock clk = new Clock("My clock", 256);
            Spacecraft spc = new Spacecraft(-1001, "MySpacecraft", 100.0, 10000.0, clk, testOrbit);
            scenario.AddSpacecraft(spc);
            var summary = await scenario.SimulateAsync(new DirectoryInfo("Simulation"), false, false, TimeSpan.FromSeconds(10.0));
            //
            // var earthEphem = earth.GetEphemeris(new IO.Astrodynamics.Time.Window(start, end), earth, frame, Aberration.None, TimeSpan.FromMinutes(10)).ToArray();
            // var sunEphem = sun.GetEphemeris(new IO.Astrodynamics.Time.Window(start, end), earth, frame, Aberration.None, TimeSpan.FromMinutes(10)).ToArray();
            // var spcEphem = spc.GetEphemeris(new IO.Astrodynamics.Time.Window(start, end), earth, frame, Aberration.None, TimeSpan.FromMinutes(10)).ToArray();
            // var moonEphem = moon.GetEphemeris(new IO.Astrodynamics.Time.Window(start, end), earth, frame, Aberration.None, TimeSpan.FromMinutes(10)).ToArray();

            CosmographiaExporter exporter = new CosmographiaExporter();
            await exporter.ExportAsync(scenario, new DirectoryInfo("UserFeedback"));

// Mat img = new Mat(1024, 1024, MatType.CV_8UC3);
// img.SetTo(0);
// double d = 1000000.0;
// for (int n = 1; n < earthEphem.Length; n++)
// {
//     img.Line(new Point(img.Width / 2, img.Height / 2), new Point(img.Width / 2 + sunEphem[n - 1].ToStateVector().Position.X / d, img.Height / 2 + sunEphem[n - 1].ToStateVector().Position.Y / d), Scalar.Black, 1);
//     // Earth at center
//     img.Circle(new Point(img.Width / 2, img.Height / 2), (int)(earth.EquatorialRadius / d) + 1, Scalar.Blue, -1);
//     // Moon path
//     img.Circle(new Point(img.Width / 2 + moonEphem[n].ToStateVector().Position.X / d, img.Height / 2 + moonEphem[n].ToStateVector().Position.Y / d), (int)(moon.EquatorialRadius / d), Scalar.Gray, -1);
//     // Calculated path
//     img.Circle(new Point(img.Width / 2 + spcEphem[n].ToStateVector().Position.X / d, img.Height / 2 + spcEphem[n].ToStateVector().Position.Y / d), 1, Scalar.OrangeRed, -1);
//     // Sun orientation
//     img.Line(new Point(img.Width / 2, img.Height / 2), new Point(img.Width / 2 + sunEphem[n].ToStateVector().Position.X / d, img.Height / 2 + sunEphem[n].ToStateVector().Position.Y / d), Scalar.Yellow, 1);
//     Cv2.ImShow("sim", img);
//     Cv2.WaitKey(10);}
// img.SaveImage("test.png");
        }
    }
}