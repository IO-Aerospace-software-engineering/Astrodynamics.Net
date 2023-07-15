using System;
using System.IO;
using System.Linq;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
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
        public void Propagate()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

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
                .SetNextManeuver(new ZenithAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new RetrogradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine))
                .SetNextManeuver(new ProgradeAttitude(DateTime.MinValue, TimeSpan.FromMinutes(1), engine));
            spacecraft.SetStandbyManeuver(planeAlignmentManeuver);

            scenario.AddSpacecraft(spacecraft);
            var summary = scenario.Simulate(Constants.OutputPath);

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

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(new DateTime(2021, 3, 4, 0, 32, 42, 853, DateTimeKind.Unspecified), maneuverWindow.Value.StartDate);
                Assert.Equal(new DateTime(2021, 3, 4, 5, 27, 55, 476, DateTimeKind.Unspecified), maneuverWindow.Value.EndDate);
            }

            Assert.Equal(2446.224510859071, summary.SpacecraftSummaries.First().FuelConsumption);
        }

        [Fact]
        public void Equality()
        {
            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
            Scenario scenario = new Scenario("scn1", mission, new Window(DateTime.MinValue, DateTime.MaxValue));

            Astrodynamics.Mission.Mission mission2 = new Astrodynamics.Mission.Mission("mission02");
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
        public void PropagateException()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission01");
            Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
            Assert.Throws<ArgumentNullException>(() => scenario.AddSpacecraft(null));
            Assert.Throws<InvalidOperationException>(() => scenario.Simulate(new DirectoryInfo("/")));
        }
    }
}