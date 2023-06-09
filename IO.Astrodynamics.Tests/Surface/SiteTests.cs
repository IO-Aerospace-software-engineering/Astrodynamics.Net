using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Surface
{
    public class SiteTests
    {
        private readonly API _api;

        public SiteTests()
        {
            _api = new API();
            _api.LoadKernels(Astrodynamics.Tests.Constants.SolarSystemKernelPath);
        }

        [Fact]
        public void StateVector()
        {
            Models.Mission.Mission mission = new Models.Mission.Mission("mission1");
            Scenario scenario = new Scenario("scn1", mission,
                new Window(new DateTime(2021, 1, 1), new DateTime(2021, 1, 2)));
            var epoch = DateTime.MinValue;
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            CelestialBodyScenario earthScn = new CelestialBodyScenario(earth, scenario);
            Site site = new Site(33, "S1", earthScn,
                new Geodetic(30 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 100.0));

            var sv = site.GetEphemeris(Frames.Frame.ICRF, epoch);

            Assert.Equal(
                new StateVector(new Vector3(4113.332255456191, -4876.6144543658074, 1124.8677317992631),
                    new Vector3(0.355608338559514, 0.29994891922262568, -1.2671335428143015E-08), earthScn, epoch,
                    Frames.Frame.ICRF), sv);
        }

        [Fact]
        public void RelativeStatevectorToBody()
        {
            var epoch = new DateTime(2021, 1, 1);


            var moon = TestHelpers.GetMoon();
            var earth = moon.InitialOrbitalParameters.CenterOfMotion;
            Site site = new Site(33, "S1", earth, new Geodetic(30 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 0.0));

            var sv = site.RelativeStateVector(Frames.Frame.ICRF, moon, epoch);

            Assert.Equal(
                new StateVector(new Vector3(-202831.34150844064, 284319.70678317308, 150458.88140126597),
                    new Vector3(-0.48702480142667454, -0.26438331399030518, -0.17175837261637006), earth, epoch,
                    Frames.Frame.ICRF), sv);
        }

        [Fact]
        public void FindDayWindows()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);


            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(33, "S1", earth, new Geodetic(2.2 * Constants.Deg2Rad, 48.0 * Constants.Deg2Rad, 0.0));

            var res = site.FindDayWindows(new Window(epoch, epoch.AddDays(1.0)), Constants.CivilTwilight);
            Assert.Equal(2, res.Length);
            Assert.Equal(new Window(new DateTime(2000, 1, 1, 12, 0, 0), new DateTime(630823419313206212)), res[0]);
            Assert.Equal(new Window(new DateTime(630823935750868526), new DateTime(630824283859829452)), res[1]);
        }

        [Fact]
        public void FindNightWindows()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);


            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(33, "S1", earth, new Geodetic(2.2 * Constants.Deg2Rad, 48.0 * Constants.Deg2Rad, 0.0));

            var res = site.FindNightWindows(new Window(epoch, epoch.AddDays(1.0)), Constants.CivilTwilight);
            Assert.Single(res);
            Assert.Equal(new Window(new DateTime(630823419313206212), new DateTime(630823935750868526)), res[0]);
        }

        [Fact]
        public void IsDay()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);


            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(33, "S1", earth, new Geodetic(0.0, 45.0 * Constants.Deg2Rad, 0.0));

            var isday = site.IsDay(epoch, Constants.CivilTwilight);
            Assert.True(isday);
        }

        [Fact]
        public void IsNight()
        {
            var epoch = new DateTime(2000, 1, 1, 0, 0, 0);


            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(33, "S1", earth, new Geodetic(0.0, 45.0 * Constants.Deg2Rad, 0.0));

            var isNight = site.IsNight(epoch, Constants.CivilTwilight);
            Assert.True(isNight);
        }

        [Fact]
        public void GetHorizontalCoordinates()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);

            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(33, "S1", earth,
                new Geodetic(-116.7944627147624 * Constants.Deg2Rad, 35.2471635434595 * Constants.Deg2Rad, 0.107));
            var hor = site.GetHorizontalCoordinates(TestHelpers.GetMarsAtJ2000(), epoch);
            Assert.Equal(-0.53861419209430739, hor.Azimuth);
            Assert.Equal(-1.1359034659274423, hor.Elevation);
            Assert.Equal(276702594.231908, hor.Altitude);
        }
    }
}