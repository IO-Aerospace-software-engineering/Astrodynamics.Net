using System;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Mission
{
    public class CelestialBodyScenarioTests
    {
        [Fact]
        public void Create()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            CelestialBody sun = new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
            CelestialBodyScenario sunScn = new CelestialBodyScenario(sun, scenario);
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            var ke = new KeplerianElements(150000000.0, 0.0, 0.0, 0.0, 0.0, 0.0, sunScn, DateTime.UtcNow, Frames.Frame.ECLIPTIC);
            CelestialBodyScenario cbs = new CelestialBodyScenario(earth, ke, scenario);
            Assert.Equal(earth, cbs.PhysicalBody);
            Assert.Equal(ke, cbs.InitialOrbitalParameters);
            Assert.Equal(1, sunScn.Satellites.Count);
        }

        [Fact]
        public void RelativeStateVectorSatellites()
        {
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;

            var sv = earth.RelativeStateVector(moon, epoch);

            Assert.Equal(new StateVector(new Vector3(-206886.4826237993, 289114.63909820508, 151574.68843800441), new Vector3(-0.83667643898339206, -0.56025436631740733, -0.17104593905855481), earth, epoch, Frames.Frame.ICRF), sv);
        }

        [Fact]
        public void RelativeStateVectorSibling()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;

            Clock clk1 = new Clock("My clock", 1.0 / 256.0);
            Spacecraft spc1 = new Spacecraft(-1001, "My spacecraft", 1000.0,10000.0);

            var sv = new StateVector(new Vector3(6800.0, 0.0, 0.0), new Vector3(0.0, 8.0, 0.0), earth, new DateTime(2021, 1, 1), Frames.Frame.ICRF);
            SpacecraftScenario sc = new SpacecraftScenario(spc1, clk1, sv, scenario);

            var res = sc.RelativeStateVector(moon, epoch);

            Assert.Equal(moon.GetEphemeris(epoch) - sv, res);
        }


        [Fact]
        public void RelativeStateVectorSun()
        {
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;

            var res = sun.RelativeStateVector(moon, epoch);

            Assert.Equal(new StateVector(new Vector3(-27002262.034789011, 132990228.16070271, 57676909.359087259), new Vector3(-30.602256528804432, -5.63559431906432, -2.371975915812508), sun, epoch, Frames.Frame.ICRF), res);
        }


        [Fact]
        public void RelativeStateVectorOtherBranch()
        {
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;
            var mars = TestHelpers.GetMars();

            var res = moon.RelativeStateVector(mars, epoch);

            Assert.Equal(new StateVector(new Vector3(119883897.84258185, 55016482.438638419, 26051146.221691012), new Vector3(9.43567384880443, 16.363385989064319, 7.8636912558125083), sun, epoch, Frames.Frame.ICRF), res);
        }

        [Fact]
        public void RelativeStateVectorFromSun()
        {
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;

            var res = sun.RelativeStateVector(moon, epoch);

            Assert.Equal(new StateVector(new Vector3(-27002262.034789011, 132990228.16070271, 57676909.359087259), new Vector3(-30.602256528804432, -5.63559431906432, -2.371975915812508), sun, epoch, Frames.Frame.ICRF), res);
        }

        [Fact]
        public void RelativeStateVectorToSun()
        {
            var epoch = new DateTime(2021, 1, 1);
            
            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;

            var res = moon.RelativeStateVector(sun, epoch);

            Assert.Equal(new StateVector(new Vector3(27002262.034789011, -132990228.16070271, -57676909.359087259), new Vector3(30.602256528804432, 5.63559431906432, 2.371975915812508), sun, epoch, Frames.Frame.ICRF), res);
        }

        [Fact]
        public void IsOcculted()
        {
            var epoch = new DateTime(2000, 1, 1);
            
            var moon = TestHelpers.GetMoonAt20011214();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;
            var res = sun.IsOcculted(moon, earth, new DateTime(2001, 12, 14, 21, 0, 0));
            Assert.True(res == IO.Astrodynamics.Models.Mission.OccultationType.Partial);

        }

        [Fact]
        public void FindOccultationsEclipse()
        {
            var epoch = new DateTime(2000, 1, 1);
            
            var moon = TestHelpers.GetMoonAt20011214();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;
            var res = sun.FindOccultations(moon, earth, new Window(new DateTime(2001, 12, 14, 20, 0, 0), new DateTime(2001, 12, 14, 22, 0, 0)), TimeSpan.FromMinutes(1.0));
            Assert.Equal(3, res.Length);
            Assert.Equal(new DateTime(2001, 12, 14, 20, 0, 0, 0), res[0].Window.StartDate);
            Assert.Equal(new DateTime(2001, 12, 14, 20, 11, 7, 500), res[0].Window.EndDate);
            Assert.Equal(IO.Astrodynamics.Models.Mission.OccultationType.None, res[0].OccultationType);

            Assert.Equal(new DateTime(2001, 12, 14, 20, 11, 7, 500), res[1].Window.StartDate);
            Assert.Equal(new DateTime(2001, 12, 14, 21, 36, 50, 625), res[1].Window.EndDate);
            Assert.Equal(IO.Astrodynamics.Models.Mission.OccultationType.Partial, res[1].OccultationType);

            Assert.Equal(new DateTime(2001, 12, 14, 21, 36, 50, 625), res[2].Window.StartDate);
            Assert.Equal(new DateTime(2001, 12, 14, 22, 0, 0, 0), res[2].Window.EndDate);
            Assert.Equal(IO.Astrodynamics.Models.Mission.OccultationType.None, res[2].OccultationType);

        }

        [Fact]
        public void FindOccultations()
        {
            var epoch = new DateTime(2000, 1, 1);
            
            var moon = TestHelpers.GetMoonAt20011214();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            var sun = earth.InitialOrbitalParameters.CenterOfMotion;
            var res = sun.FindOccultations(moon, earth, new Window(new DateTime(2001, 12, 14, 3, 0, 0), new DateTime(2001, 12, 14, 4, 0, 0)), TimeSpan.FromMinutes(1.0));
            Assert.Single(res);
            Assert.Equal(new DateTime(2001, 12, 14, 3, 0, 0), res[0].Window.StartDate);
            Assert.Equal(new DateTime(2001, 12, 14, 4, 0, 0), res[0].Window.EndDate);
            Assert.Equal(IO.Astrodynamics.Models.Mission.OccultationType.None, res[0].OccultationType);

            

        }

        [Fact]
        public void AngularSize()
        {
            var epoch = new DateTime(2000, 1, 1);
            
            var moon = TestHelpers.GetMoonAtJ2000();
            var sun = TestHelpers.GetSun();
            var earth = TestHelpers.GetEarthAtJ2000();
            var res = sun.AngularSize(earth.InitialOrbitalParameters.ToStateVector().Position.Magnitude());
            Assert.Equal(0.009456, res, 6);

        }
    }
}