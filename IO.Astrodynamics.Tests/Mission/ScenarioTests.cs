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
        Assert.Equal("2021-03-04T00:32:49.8180000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:58.2100000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:49.8180000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T00:32:58.2100000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.392, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(
                new Vector3(-97.07142021113363, 107.83054000114183, -119.89750368038308),
            ((ImpulseManeuver)maneuver).DeltaV);


        Assert.Equal(419.6270875080256, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:15:39.8410000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:02.1870000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:15:39.8410000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:02.1870000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(22.346, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-462.37361865043476, -165.7923214353268, 234.3316344286659),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(1117.349455081975, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T01:16:13.2430000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T04:59:20.7690000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:13.2430000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T01:16:22.7850000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(9.542, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(-139.4953930672767, 85.72345306943599, 194.88266433187357),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(477.1217927049922, maneuver.FuelBurned);

        maneuver = maneuver.NextManeuver;

        Assert.Equal("2021-03-04T05:23:58.7300000 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:07.2980000 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:23:58.7300000 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
        Assert.Equal("2021-03-04T05:24:07.2980000 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
        Assert.Equal(8.568, maneuver.ThrustWindow.Length.TotalSeconds);
        Assert.Equal(new Vector3(134.22725061486776, -81.4515454171397, -183.87034411714475),
            ((ImpulseManeuver)maneuver).DeltaV);
        Assert.Equal(428.43642901863194, maneuver.FuelBurned);

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(new DateTime(2021, 3, 4, 0, 32, 49, 818, DateTimeKind.Unspecified), maneuverWindow.Value.StartDate);
                Assert.Equal(new DateTime(2021, 3, 4, 5, 27, 13, 014, DateTimeKind.Unspecified), maneuverWindow.Value.EndDate);
            }

            Assert.Equal(2442.5347643136247, summary.SpacecraftSummaries.First().FuelConsumption);
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
        public void PropagateException()
        {
            DateTime start = DateTimeExtension.CreateUTC(667915269.18539762).ToTDB();
            DateTime startPropagator = DateTimeExtension.CreateUTC(668085555.829810).ToTDB();
            DateTime end = DateTimeExtension.CreateUTC(668174400.000000).ToTDB();

            Astrodynamics.Mission.Mission mission = new Astrodynamics.Mission.Mission("mission04");
            Scenario scenario = new Scenario("scn1", mission, new Window(startPropagator, end));
            Assert.Throws<ArgumentNullException>(() => scenario.AddSpacecraft(null));
            Assert.Throws<InvalidOperationException>(() => scenario.Simulate(new DirectoryInfo("/")));
        }
    }
}