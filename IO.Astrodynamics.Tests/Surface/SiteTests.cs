using System;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;
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
            var epoch = DateTime.MinValue;
            CelestialBody earth = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366);
            Site site = new Site(33, "S1", earth,
                new Geodetic(30 * Constants.Deg2Rad, 10.0 * Constants.Deg2Rad, 100.0));

            var sv = site.GetEphemeris(epoch, earth, Frames.Frame.ICRF, Aberration.None);

            Assert.Equal(
                new StateVector(new Vector3(4113.332255456191, -4876.6144543658074, 1124.8677317992631),
                    new Vector3(0.355608338559514, 0.29994891922262568, -1.2671335428143015E-08), earth, epoch,
                    Frames.Frame.ICRF), sv);
        }


        [Fact]
        public void GetHorizontalCoordinates()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0);

            var earth = TestHelpers.GetEarthAtJ2000();
            Site site = new Site(13, "DSS-13", TestHelpers.GetEarthAtJ2000());
            var hor = site.GetHorizontalCoordinates(epoch, TestHelpers.GetMoonAtJ2000(), Aberration.None);
            Assert.Equal(-0.53861419209430739, hor.Azimuth);
            Assert.Equal(-1.1359034659274423, hor.Elevation);
            Assert.Equal(276702594.231908, hor.Altitude);
        }
    }
}