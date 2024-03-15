using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Coordinates;
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
            API.Instance.LoadKernels(scenario.SiteDirectory);
            var res = site.GetEphemeris(window, TestHelpers.EarthAtJ2000, Frames.Frame.ICRF, Aberration.None, TimeSpan.FromHours(1));
            var orbitalParametersEnumerable = res as Astrodynamics.OrbitalParameters.OrbitalParameters[] ?? res.ToArray();
            Assert.Equal(new Vector3(4054783.0920394, -4799280.902678638, 1100391.2395513842), orbitalParametersEnumerable.ElementAt(0).ToStateVector().Position,TestHelpers.VectorComparer);
            Assert.Equal(new Vector3(349.9689414487369, 295.67943565441215, 0.00047467276487285595), orbitalParametersEnumerable.ElementAt(0).ToStateVector().Velocity,TestHelpers.VectorComparer);
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
            scenario.AddAdditionalCelestialBody(TestHelpers.Sun);

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
                .SetNextManeuver(new ApogeeHeightManeuver(DateTime.MinValue, TimeSpan.Zero, 15866666.666666666, spacecraft.Engines.First()))
                .SetNextManeuver(new ZenithAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new RetrogradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new ProgradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine));
            spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

            scenario.AddSpacecraft(spacecraft);

            var summary = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            API.Instance.LoadKernels(scenario.SpacecraftDirectory);
            API.Instance.LoadKernels(scenario.SiteDirectory);
            // Read maneuver results
            Maneuver.Maneuver maneuver = planeAlignmentManeuver;
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.3670000000000009, maneuver.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-96.78279591853784, 107.4621743531532, -119.5488483724332), ((ImpulseManeuver)maneuver).DeltaV);

            Assert.Equal(418.3569435850257, maneuver.FuelBurned);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(23.216, maneuver.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-480.86782901253173, -189.23162389163735, 232.6785007948747), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(1160.792, maneuver.FuelBurned, 3);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:59:16.6930561 (TDB)", maneuver.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:03.2054735 (TDB)", maneuver.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.556, maneuver.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.48031676408738, 86.1993776765905, 196.15692847302532), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(477.794, maneuver.FuelBurned, 3);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.623, maneuver.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(132.17464382071327, -86.77032419711468, -186.7460052743688), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(431.133, maneuver.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:32:26.4412955"), maneuverWindow.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T05:25:28.0138099"), maneuverWindow.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2488.076, summary.SpacecraftSummaries.First().FuelConsumption, 3);
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
            scenario.AddAdditionalCelestialBody(TestHelpers.Sun);

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
                .SetNextManeuver(new ApogeeHeightManeuver(DateTime.MinValue, TimeSpan.Zero, 15866666.666666666, spacecraft.Engines.First()))
                .SetNextManeuver(new ZenithAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new RetrogradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new ProgradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine));
            spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

            scenario.AddSpacecraft(spacecraft);

            var summary1 = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            API.Instance.LoadKernels(scenario.SpacecraftDirectory);
            API.Instance.LoadKernels(scenario.SiteDirectory);
            // Read maneuver results
            Maneuver.Maneuver maneuver1 = spacecraft.InitialManeuver;
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.3670000000000009, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-96.78279591853784, 107.4621743531532, -119.5488483724332), ((ImpulseManeuver)maneuver1).DeltaV);

            Assert.Equal(418.3569435850257, maneuver1.FuelBurned);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(23.216, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-480.86782901253173, -189.23162389163735, 232.6785007948747), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(1160.792, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:59:16.6930561 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:03.2054735 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.556, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.48031676408738, 86.1993776765905, 196.15692847302532), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(477.794, maneuver1.FuelBurned, 3);

            maneuver1 = maneuver1.NextManeuver;

            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver1.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver1.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver1.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver1.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.623, maneuver1.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(132.17464382071327, -86.77032419711468, -186.7460052743688), ((ImpulseManeuver)maneuver1).DeltaV);
            Assert.Equal(431.133, maneuver1.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary1.Window);
            Assert.Single(summary1.SpacecraftSummaries);
            var maneuverWindow1 = summary1.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow1 != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:32:26.4412955"), maneuverWindow1.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T05:25:28.0138099"), maneuverWindow1.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2488.076, summary1.SpacecraftSummaries.First().FuelConsumption, 3);
            
            API.Instance.UnloadKernels(scenario.SpacecraftDirectory);
            API.Instance.UnloadKernels(scenario.SiteDirectory);
            var summary2 = await scenario.SimulateAsync(Constants.OutputPath, false, false, TimeSpan.FromSeconds(1.0));
            API.Instance.LoadKernels(scenario.SpacecraftDirectory);
            API.Instance.LoadKernels(scenario.SiteDirectory);
            // Read maneuver results
            Maneuver.Maneuver maneuver2 = spacecraft.InitialManeuver;
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:26.4412955 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:34.8084343 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.3670000000000009, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-96.78279591853784, 107.4621743531532, -119.5488483724332), ((ImpulseManeuver)maneuver2).DeltaV);

            Assert.Equal(418.3569435850257, maneuver2.FuelBurned);

            maneuver2 = maneuver2.NextManeuver;

            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:18.5520621 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:41.7678997 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(23.216, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-480.86782901253173, -189.23162389163735, 232.6785007948747), ((ImpulseManeuver)maneuver2).DeltaV);
            Assert.Equal(1160.792, maneuver2.FuelBurned, 3);

            maneuver2 = maneuver2.NextManeuver;

            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:59:16.6930561 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:53.6495944 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:03.2054735 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(9.556, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-140.48031676408738, 86.1993776765905, 196.15692847302532), ((ImpulseManeuver)maneuver2).DeltaV);
            Assert.Equal(477.794, maneuver2.FuelBurned, 3);

            maneuver2 = maneuver2.NextManeuver;

            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver2.ManeuverWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver2.ManeuverWindow?.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:15.2711135 (TDB)", maneuver2.ThrustWindow?.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:22:23.8937808 (TDB)", maneuver2.ThrustWindow?.EndDate.ToFormattedString());
            Assert.Equal(8.623, maneuver2.ThrustWindow.Value.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(132.17464382071327, -86.77032419711468, -186.7460052743688), ((ImpulseManeuver)maneuver2).DeltaV);
            Assert.Equal(431.133, maneuver2.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary2.Window);
            Assert.Single(summary2.SpacecraftSummaries);
            var maneuverWindow2 = summary2.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow2 != null)
            {
                Assert.Equal(DateTime.Parse("2021-03-04T00:32:26.4412955"), maneuverWindow2.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(DateTime.Parse("2021-03-04T05:25:28.0138099"), maneuverWindow2.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2488.076, summary2.SpacecraftSummaries.First().FuelConsumption, 3);
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
            Spacecraft spacecraft =
                new Spacecraft(-1785, "DRAGONFLY32", 1000.0, 10000.0, clock, parkingOrbit);

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
    }
}