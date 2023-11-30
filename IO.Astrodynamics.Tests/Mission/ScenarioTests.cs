using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
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
        [Benchmark]
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
            Assert.Equal("2021-03-04T00:32:49.8175394 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:58.2100803 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:49.8175394 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T00:32:58.2100803 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
            Assert.Equal(8.3925409000000002, maneuver.ThrustWindow.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-97.0714098295484, 107.8305292474044, -119.8974902969949), ((ImpulseManeuver)maneuver).DeltaV);


            Assert.Equal(419.62704377403645, maneuver.FuelBurned);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T01:15:39.8402441 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:02.1873755 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:15:39.8402441 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:02.1873755 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
            Assert.Equal(22.347131399999999, maneuver.ThrustWindow.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-462.3769514474425, -165.79592575424203, 234.3310697732304), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(1117.3565715207112, maneuver.FuelBurned, 3);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T01:16:13.2426029 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T04:59:20.7658396 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:13.2426029 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T01:16:22.7850168 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
            Assert.Equal(9.5424138999999997, maneuver.ThrustWindow.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(-139.4951793647351, 85.72332642451474, 194.88236993709975), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(477.12069491886507, maneuver.FuelBurned, 3);

            maneuver = maneuver.NextManeuver;

            Assert.Equal("2021-03-04T05:23:58.7294586 (TDB)", maneuver.ManeuverWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:24:07.2981610 (TDB)", maneuver.ManeuverWindow.EndDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:23:58.7294586 (TDB)", maneuver.ThrustWindow.StartDate.ToFormattedString());
            Assert.Equal("2021-03-04T05:24:07.2981610 (TDB)", maneuver.ThrustWindow.EndDate.ToFormattedString());
            Assert.Equal(8.5687023999999994, maneuver.ThrustWindow.Length.TotalSeconds, 3);
            Assert.Equal(new Vector3(134.22795291868337, -81.45016347177679, -183.86968653289438), ((ImpulseManeuver)maneuver).DeltaV);
            Assert.Equal(428.43511797710403, maneuver.FuelBurned, 3);

            Assert.Equal(scenario.Window, summary.Window);
            Assert.Single(summary.SpacecraftSummaries);
            var maneuverWindow = summary.SpacecraftSummaries.First().ManeuverWindow;
            if (maneuverWindow != null)
            {
                Assert.Equal(new DateTime(2021, 3, 4, 0, 32, 49, 818, DateTimeKind.Unspecified), maneuverWindow.Value.StartDate, TimeSpan.FromMilliseconds(1));
                Assert.Equal(new DateTime(2021, 3, 4, 5, 27, 13, 014, DateTimeKind.Unspecified), maneuverWindow.Value.EndDate, TimeSpan.FromMilliseconds(1));
            }

            Assert.Equal(2442.5394281907165, summary.SpacecraftSummaries.First().FuelConsumption, 3);
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